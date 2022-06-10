namespace Yup.Validation;

public interface ISIUValidator<TEntidad> where TEntidad : IValidable
{
    ValidationResult Validar(TEntidad entidad);
}
