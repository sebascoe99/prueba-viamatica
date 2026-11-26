# 06 - Seguridad: cifrado, hash, JWT con claims cifradas y login

**Herramienta:** Claude Code (agente de IA en terminal)

## Prompt

> Sigamos con la seguridad: cifrado, hash, JWT con claims cifrados y el login.

## Resultado

- **Application**: interfaces `IEncryptionService`, `IPasswordHasher`, `IJwtTokenService`, `IUserRepository`;
  `AuthService` con la lógica de inicio y cierre de sesión; `AppException` para errores de negocio.
- **Infrastructure**: `AesEncryptionService` (AES-256), `BcryptPasswordHasher`, `JwtTokenService`,
  `UserRepository` y `DbInitializer` (aplica migraciones y crea el administrador inicial).
- **Api**: `AuthController` (`login`, `logout`, `me`), configuración de JWT Bearer que descifra
  los claims y `ExceptionMiddleware` para respuestas de error uniformes.

### Comandos usados

```bash
dotnet add src/GestionCaja.Application package Microsoft.Extensions.Logging.Abstractions --version "10.*"
dotnet add src/GestionCaja.Application package Microsoft.Extensions.DependencyInjection.Abstractions --version "10.*"
dotnet add src/GestionCaja.Infrastructure package BCrypt.Net-Next
dotnet add src/GestionCaja.Infrastructure package Microsoft.IdentityModel.JsonWebTokens
dotnet add src/GestionCaja.Api package Microsoft.AspNetCore.Authentication.JwtBearer --version "10.*"

dotnet run --project src/GestionCaja.Api
```

### Decisiones

- **Contraseña con hash BCrypt** (irreversible) y **correo e identificación con AES-256** (reversible,
  porque deben mostrarse). Cada cifrado usa un IV aleatorio.
- **Claims cifradas**: el id, el usuario y el rol se cifran con AES antes de firmar el token. La Api
  los descifra en el evento `OnTokenValidated`, después de validar firma y vigencia.
- **Bloqueo automático** al tercer intento fallido consecutivo.
- El login responde el mismo mensaje si el usuario no existe o la clave es incorrecta.

### Pruebas realizadas

| Caso | Resultado |
|---|---|
| Login con credenciales correctas | 200 y token |
| Payload del token decodificado | claims ilegibles (cifradas) |
| `GET /api/auth/me` con token | 200 con los claims descifrados |
| `GET /api/auth/me` sin token o con token alterado | 401 |
| Login con contraseña incorrecta | 401 |
| Login con campos vacíos | 400 |
| Logout | 204 y `session_active = false` |
