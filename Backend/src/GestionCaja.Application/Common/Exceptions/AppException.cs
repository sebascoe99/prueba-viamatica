namespace GestionCaja.Application.Common.Exceptions;

/// <summary>
/// Error controlado de negocio. La Api lo convierte en una respuesta HTTP
/// con el código indicado y el mensaje para el usuario.
/// </summary>
public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}
