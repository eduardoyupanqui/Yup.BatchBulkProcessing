namespace Yup.Validation;

public class ValorRequeridoAttribute : ValidatorAttribute
{
    public override bool Validate(object input, out string mensaje)
    {
        mensaje = "{0} es requerido";

        if (input is string)
            return !string.IsNullOrWhiteSpace((string)input);

        return input != null;
    }
}
