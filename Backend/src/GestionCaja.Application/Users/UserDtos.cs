using System.ComponentModel.DataAnnotations;

namespace GestionCaja.Application.Users;

public static class UserRules
{
    public const string UserName = @"^(?=.*[A-Za-z])(?=.*[0-9])[A-Za-z0-9]{8,20}$";
    public const string UserNameMessage = "El nombre de usuario debe tener entre 8 y 20 caracteres, solo letras y números, con al menos un número";

    public const string Password = @"^(?=.*[0-9])(?=.*[A-Z]).{8,30}$";
    public const string PasswordMessage = "La contraseña debe tener entre 8 y 30 caracteres, al menos un número y una letra mayúscula";

    public const string Identification = @"^[0-9]{10,13}$";
    public const string IdentificationMessage = "La identificación debe tener entre 10 y 13 dígitos numéricos";
}

public class UserCreateRequest
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
    [RegularExpression(UserRules.UserName, ErrorMessage = UserRules.UserNameMessage)]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [RegularExpression(UserRules.Password, ErrorMessage = UserRules.PasswordMessage)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los nombres son obligatorios")]
    [StringLength(50, ErrorMessage = "Los nombres no pueden superar 50 caracteres")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios")]
    [StringLength(50, ErrorMessage = "Los apellidos no pueden superar 50 caracteres")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido")]
    [StringLength(100, ErrorMessage = "El correo no puede superar 100 caracteres")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La identificación es obligatoria")]
    [RegularExpression(UserRules.Identification, ErrorMessage = UserRules.IdentificationMessage)]
    public string Identification { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "El rol es obligatorio")]
    public int RolId { get; set; }
}

public class UserUpdateRequest
{
    [Required(ErrorMessage = "Los nombres son obligatorios")]
    [StringLength(50, ErrorMessage = "Los nombres no pueden superar 50 caracteres")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios")]
    [StringLength(50, ErrorMessage = "Los apellidos no pueden superar 50 caracteres")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido")]
    [StringLength(100, ErrorMessage = "El correo no puede superar 100 caracteres")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La identificación es obligatoria")]
    [RegularExpression(UserRules.Identification, ErrorMessage = UserRules.IdentificationMessage)]
    public string Identification { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "El rol es obligatorio")]
    public int RolId { get; set; }
}

public class UserStatusRequest
{
    [Required(ErrorMessage = "El estado es obligatorio")]
    public string StatusId { get; set; } = string.Empty;
}

public class RecoverPasswordRequest
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La identificación es obligatoria")]
    public string Identification { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
    [RegularExpression(UserRules.Password, ErrorMessage = UserRules.PasswordMessage)]
    public string NewPassword { get; set; } = string.Empty;
}

public class UserResponse
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Identification { get; set; } = string.Empty;
    public int RolId { get; set; }
    public string RolName { get; set; } = string.Empty;
    public string StatusId { get; set; } = string.Empty;
    public string StatusDescription { get; set; } = string.Empty;
    public bool SessionActive { get; set; }
    public DateTime CreationDate { get; set; }
    public int? UserCreate { get; set; }
    public int? UserApproval { get; set; }
    public DateTime? DateApproval { get; set; }
    public DateTime? LastLogin { get; set; }
}

public class UserBulkRow
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Identification { get; set; }
    public int RolId { get; set; }

    public UserCreateRequest ToRequest() => new()
    {
        UserName = UserName ?? string.Empty,
        Password = Password ?? string.Empty,
        FirstName = FirstName ?? string.Empty,
        LastName = LastName ?? string.Empty,
        Email = Email ?? string.Empty,
        Identification = Identification ?? string.Empty,
        RolId = RolId
    };
}
