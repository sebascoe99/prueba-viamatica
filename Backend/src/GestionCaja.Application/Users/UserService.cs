using GestionCaja.Application.Common;
using GestionCaja.Application.Common.Exceptions;
using GestionCaja.Application.Common.Interfaces;
using GestionCaja.Application.Common.Validation;
using GestionCaja.Domain.Constants;
using GestionCaja.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace GestionCaja.Application.Users;

public interface IUserService
{
    Task<List<UserResponse>> ListAsync(int? rolId, string? statusId, string? search, CancellationToken cancellationToken = default);
    Task<UserResponse> GetAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserResponse> CreateAsync(UserCreateRequest request, CancellationToken cancellationToken = default);
    Task<BulkResult> BulkCreateAsync(List<UserCreateRequest> rows, CancellationToken cancellationToken = default);
    Task<UserResponse> UpdateAsync(int userId, UserUpdateRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> ChangeStatusAsync(int userId, string statusId, CancellationToken cancellationToken = default);
    Task<UserResponse> ApproveAsync(int userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int userId, CancellationToken cancellationToken = default);
    Task RecoverPasswordAsync(RecoverPasswordRequest request, CancellationToken cancellationToken = default);
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEncryptionService _encryptionService;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IEncryptionService encryptionService,
        ICurrentUser currentUser,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _encryptionService = encryptionService;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<List<UserResponse>> ListAsync(int? rolId, string? statusId, string? search, CancellationToken cancellationToken = default)
    {
        // El administrador ve todos los usuarios; el gestor solo los que él creó
        int? createdBy = _currentUser.RolId == Roles.Gestor ? _currentUser.UserId : null;

        var users = await _userRepository.ListAsync(rolId, statusId, search, createdBy, cancellationToken);
        return users.Select(ToResponse).ToList();
    }

    public async Task<UserResponse> GetAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await FindOrThrowAsync(userId, cancellationToken);
        return ToResponse(user);
    }

    public async Task<UserResponse> CreateAsync(UserCreateRequest request, CancellationToken cancellationToken = default)
    {
        EnsureManageableRol(request.RolId);

        var userName = request.UserName.Trim();
        if (await _userRepository.ExistsUserNameAsync(userName, cancellationToken))
        {
            throw new AppException($"El nombre de usuario {userName} ya está registrado", 409);
        }

        // Regla del enunciado: lo que crea un administrador queda activo;
        // lo que crea un gestor queda pendiente hasta que un administrador lo apruebe.
        var createdByAdmin = _currentUser.RolId == Roles.Administrador;

        var user = new User
        {
            UserName = userName,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = _encryptionService.Encrypt(request.Email.Trim().ToLowerInvariant()),
            Identification = _encryptionService.Encrypt(request.Identification.Trim()),
            Password = _passwordHasher.Hash(request.Password),
            RolId = request.RolId,
            UserStatusId = createdByAdmin ? UserStatuses.Activo : UserStatuses.PendienteAprobacion,
            UserCreate = _currentUser.UserId,
            UserApproval = createdByAdmin ? _currentUser.UserId : null,
            DateApproval = createdByAdmin ? DateTime.UtcNow : null
        };

        _userRepository.Add(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CREACIÓN de usuario {UserName} (id {UserId}, rol {RolId}, estado {Status}) por {CurrentUser}",
            user.UserName, user.UserId, user.RolId, user.UserStatusId, _currentUser.UserName);

        return ToResponse(await FindOrThrowAsync(user.UserId, cancellationToken));
    }

    public async Task<BulkResult> BulkCreateAsync(List<UserCreateRequest> rows, CancellationToken cancellationToken = default)
    {
        var result = new BulkResult { Total = rows.Count };

        // Cada fila se procesa de forma independiente: una fila con problemas no detiene a las demás
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var rowResult = new BulkRowResult { Row = i + 1, Key = row.UserName };

            try
            {
                DtoValidator.ValidateOrThrow(row);
                await CreateAsync(row, cancellationToken);

                rowResult.Success = true;
                rowResult.Message = "Usuario creado";
                result.Created++;
            }
            catch (AppException ex)
            {
                rowResult.Message = ex.Message;
                result.Failed++;
            }

            result.Rows.Add(rowResult);
        }

        _logger.LogInformation("CARGA MASIVA de usuarios por {CurrentUser}: {Created} creados, {Failed} rechazados de {Total}",
            _currentUser.UserName, result.Created, result.Failed, result.Total);

        return result;
    }

    public async Task<UserResponse> UpdateAsync(int userId, UserUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var user = await FindOrThrowAsync(userId, cancellationToken);
        EnsureManageableRol(user.RolId);
        EnsureManageableRol(request.RolId);

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = _encryptionService.Encrypt(request.Email.Trim().ToLowerInvariant());
        user.Identification = _encryptionService.Encrypt(request.Identification.Trim());
        user.RolId = request.RolId;

        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ACTUALIZACIÓN de usuario {UserName} (id {UserId}) por {CurrentUser}",
            user.UserName, user.UserId, _currentUser.UserName);

        return ToResponse(await FindOrThrowAsync(userId, cancellationToken));
    }

    public async Task<UserResponse> ChangeStatusAsync(int userId, string statusId, CancellationToken cancellationToken = default)
    {
        var allowed = new[] { UserStatuses.Activo, UserStatuses.Inactivo, UserStatuses.Bloqueado };
        if (!allowed.Contains(statusId))
        {
            throw new AppException("El estado debe ser ACT, INA o BLO");
        }

        var user = await FindOrThrowAsync(userId, cancellationToken);
        EnsureManageableRol(user.RolId);

        if (user.UserStatusId == UserStatuses.PendienteAprobacion)
        {
            throw new AppException("El usuario está pendiente de aprobación. Debe aprobarlo primero");
        }

        var previousStatus = user.UserStatusId;
        user.UserStatusId = statusId;

        if (statusId == UserStatuses.Activo)
        {
            user.FailedAttempts = 0;
        }
        else
        {
            user.SessionActive = false;
        }

        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ACTUALIZACIÓN de estado del usuario {UserName} (id {UserId}): {Previous} -> {New} por {CurrentUser}",
            user.UserName, user.UserId, previousStatus, statusId, _currentUser.UserName);

        return ToResponse(await FindOrThrowAsync(userId, cancellationToken));
    }

    public async Task<UserResponse> ApproveAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await FindOrThrowAsync(userId, cancellationToken);

        if (user.UserStatusId != UserStatuses.PendienteAprobacion)
        {
            throw new AppException("El usuario no está pendiente de aprobación");
        }

        user.UserStatusId = UserStatuses.Activo;
        user.UserApproval = _currentUser.UserId;
        user.DateApproval = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("APROBACIÓN del usuario {UserName} (id {UserId}) por {CurrentUser}",
            user.UserName, user.UserId, _currentUser.UserName);

        return ToResponse(await FindOrThrowAsync(userId, cancellationToken));
    }

    public async Task DeleteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await FindOrThrowAsync(userId, cancellationToken);
        EnsureManageableRol(user.RolId);

        user.SessionActive = false;
        // Remove() termina como eliminación lógica: lo intercepta AppDbContext.SaveChangesAsync
        _userRepository.Remove(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ELIMINACIÓN lógica del usuario {UserName} (id {UserId}) por {CurrentUser}",
            user.UserName, user.UserId, _currentUser.UserName);
    }

    public async Task RecoverPasswordAsync(RecoverPasswordRequest request, CancellationToken cancellationToken = default)
    {
        const string genericError = "Los datos ingresados no coinciden con ningún usuario";

        var user = await _userRepository.GetByUserNameAsync(request.UserName.Trim(), cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Recuperación de contraseña fallida: el usuario {UserName} no existe", request.UserName);
            throw new AppException(genericError);
        }

        // Se verifica la identidad comparando contra los datos descifrados del usuario
        var sameIdentification = _encryptionService.Decrypt(user.Identification) == request.Identification.Trim();
        var sameEmail = string.Equals(_encryptionService.Decrypt(user.Email), request.Email.Trim(), StringComparison.OrdinalIgnoreCase);

        if (!sameIdentification || !sameEmail)
        {
            _logger.LogWarning("Recuperación de contraseña fallida: datos incorrectos para {UserName}", user.UserName);
            throw new AppException(genericError);
        }

        if (user.UserStatusId is UserStatuses.PendienteAprobacion or UserStatuses.Inactivo)
        {
            throw new AppException("El usuario no está habilitado. Contacte al administrador", 403);
        }

        user.Password = _passwordHasher.Hash(request.NewPassword);
        user.FailedAttempts = 0;
        user.SessionActive = false;

        // Si estaba bloqueado por intentos fallidos, recupera el acceso al demostrar su identidad
        if (user.UserStatusId == UserStatuses.Bloqueado)
        {
            user.UserStatusId = UserStatuses.Activo;
        }

        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("ACTUALIZACIÓN de contraseña por recuperación del usuario {UserName}", user.UserName);
    }

    private async Task<User> FindOrThrowAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new AppException("El usuario no existe", 404);
        }

        // Un gestor solo puede consultar los usuarios que él creó
        if (_currentUser.RolId == Roles.Gestor && user.UserCreate != _currentUser.UserId)
        {
            throw new AppException("El usuario no existe", 404);
        }

        return user;
    }

    private static void EnsureManageableRol(int rolId)
    {
        if (rolId != Roles.Gestor && rolId != Roles.Cajero)
        {
            throw new AppException("Solo se pueden gestionar usuarios con rol Gestor o Cajero");
        }
    }

    private UserResponse ToResponse(User user)
    {
        return new UserResponse
        {
            UserId = user.UserId,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = _encryptionService.Decrypt(user.Email),
            Identification = _encryptionService.Decrypt(user.Identification),
            RolId = user.RolId,
            RolName = user.Rol.RolName,
            StatusId = user.UserStatusId,
            StatusDescription = user.UserStatus.Description,
            SessionActive = user.SessionActive,
            CreationDate = user.CreationDate,
            UserCreate = user.UserCreate,
            UserApproval = user.UserApproval,
            DateApproval = user.DateApproval,
            LastLogin = user.LastLogin
        };
    }
}
