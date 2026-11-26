using GestionCaja.Application.Common.Exceptions;
using GestionCaja.Application.Common.Interfaces;
using GestionCaja.Domain.Constants;
using Microsoft.Extensions.Logging;

namespace GestionCaja.Application.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(int userId, CancellationToken cancellationToken = default);
}

public class AuthService : IAuthService
{
    // Intentos fallidos consecutivos permitidos antes de bloquear al usuario
    private const int MaxFailedAttempts = 3;

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEncryptionService _encryptionService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IEncryptionService encryptionService,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _encryptionService = encryptionService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUserNameAsync(request.UserName.Trim(), cancellationToken);

        // Mismo mensaje si el usuario no existe o la clave es incorrecta,
        // para no revelar qué nombres de usuario están registrados.
        if (user is null)
        {
            _logger.LogWarning("Inicio de sesión fallido: el usuario {UserName} no existe", request.UserName);
            throw new AppException("Usuario o contraseña incorrectos", 401);
        }

        switch (user.UserStatusId)
        {
            case UserStatuses.Bloqueado:
                _logger.LogWarning("Inicio de sesión rechazado: el usuario {UserName} está bloqueado", user.UserName);
                throw new AppException("El usuario está bloqueado. Contacte al administrador", 403);

            case UserStatuses.PendienteAprobacion:
                _logger.LogWarning("Inicio de sesión rechazado: el usuario {UserName} está pendiente de aprobación", user.UserName);
                throw new AppException("El usuario está pendiente de aprobación por un administrador", 403);

            case UserStatuses.Inactivo:
                _logger.LogWarning("Inicio de sesión rechazado: el usuario {UserName} está inactivo", user.UserName);
                throw new AppException("El usuario está inactivo", 403);
        }

        if (!_passwordHasher.Verify(request.Password, user.Password))
        {
            user.FailedAttempts++;

            if (user.FailedAttempts >= MaxFailedAttempts)
            {
                user.UserStatusId = UserStatuses.Bloqueado;
                user.SessionActive = false;
                _logger.LogWarning("El usuario {UserName} fue bloqueado por {Attempts} intentos fallidos", user.UserName, user.FailedAttempts);
            }
            else
            {
                _logger.LogWarning("Inicio de sesión fallido: contraseña incorrecta para {UserName} (intento {Attempts})", user.UserName, user.FailedAttempts);
            }

            await _userRepository.SaveChangesAsync(cancellationToken);
            throw new AppException("Usuario o contraseña incorrectos", 401);
        }

        user.FailedAttempts = 0;
        user.SessionActive = true;
        user.LastLogin = DateTime.UtcNow;
        await _userRepository.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenService.GenerateToken(user);

        _logger.LogInformation("Inicio de sesión exitoso del usuario {UserName} con rol {Rol}", user.UserName, user.Rol.RolName);

        return new LoginResponse
        {
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
            UserId = user.UserId,
            UserName = user.UserName,
            FullName = $"{user.FirstName} {user.LastName}",
            Email = _encryptionService.Decrypt(user.Email),
            RolId = user.RolId,
            RolName = user.Rol.RolName
        };
    }

    public async Task LogoutAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return;
        }

        user.SessionActive = false;
        await _userRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cierre de sesión del usuario {UserName}", user.UserName);
    }
}
