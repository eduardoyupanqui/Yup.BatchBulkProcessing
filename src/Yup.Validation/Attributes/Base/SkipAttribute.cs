namespace Yup.Validation;

/// <summary>
/// De uso interno
/// </summary>
internal class SkipAttribute : ValidatorAttribute
{
    public override bool Validate(object input, out string mensaje)
    {
        mensaje = null;
        return true;
    }
}
