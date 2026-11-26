using System.ComponentModel.DataAnnotations;
using GestionCaja.Application.Common.Exceptions;

namespace GestionCaja.Application.Common.Validation;

/// <summary>
/// Ejecuta las mismas validaciones declaradas en los DTO (atributos) desde un servicio.
/// Se usa en la carga masiva, donde las filas no pasan por la validación automática del controlador.
/// </summary>
public static class DtoValidator
{
    public static void ValidateOrThrow(object dto)
    {
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), results, validateAllProperties: true);

        if (!isValid)
        {
            throw new AppException(string.Join(". ", results.Select(r => r.ErrorMessage)));
        }
    }
}
