using System;

namespace Yup.Validation;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public abstract class ValidatorAttribute : Attribute
{
    public abstract bool Validate(object input, out string mensaje);
}
