using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Yup.Validation;

public static class IValidableExtensions
{
    public static List<string> lstIdsPropiedadesListas = new List<string> { "System.Collections.Generic.IEnumerable" };

    public static ValidationResult ValidarAtributos(this IValidable obj)
    {
        ValidationResult result = new ValidationResult();

        var arrayProperties = obj.GetType().GetProperties();
        int cantidadPropiedadesVacias = 0;

        for (int i = 0; i < arrayProperties.Length; i++)
        {
            PropertyInfo p = arrayProperties[i];
            object currentVal = p.GetValue(obj);

            if (!p.PropertyType.AssemblyQualifiedName.StartsWith(lstIdsPropiedadesListas[0])) //TODO: matchear todos los elementos de la lista
            {
                //1) Validando campos vacíos
                if (
                    currentVal == null ||
                    (currentVal is string && string.IsNullOrWhiteSpace((string)currentVal)) ||
                    (currentVal is int && (int)currentVal == 0) ||
                    (currentVal is decimal && (decimal)currentVal == 0.0M) ||
                    (currentVal is double && (double)currentVal == 0.0) ||
                    (currentVal is DateTime && (DateTime)currentVal == DateTime.MinValue)
                    )
                { cantidadPropiedadesVacias++; }

                //2) Obteniendo atributos de validacion customizados de la propiedad
                var arrValidationAttributes = p.GetCustomAttributes(typeof(ValidatorAttribute));
                if (arrValidationAttributes == null) continue;

                foreach (ValidatorAttribute validationAttr in arrValidationAttributes)
                {
                    if (validationAttr == null) continue;
                    if (validationAttr.Validate(currentVal, out string mensajeValidacion) == false)
                    {
                        result.Observaciones.Add(string.Format(mensajeValidacion, p.Name));
                    }
                }
                if (currentVal is ICollection)
                {
                    ValidarAtributosHijos((ICollection)currentVal, result);
                }

                //3) Obteniendo maestros de validacion customizados de la propiedad
                var arrValidationDictionary = p.GetCustomAttributes(typeof(ValidatorDictionary));
                if (arrValidationDictionary == null) continue;

                foreach (ValidatorDictionary validationDict in arrValidationDictionary)
                {
                    if (validationDict == null) continue;
                    if (validationDict.Validate(currentVal, out string mensajeValidacion) == false)
                    {
                        result.Observaciones.Add(string.Format(mensajeValidacion, p.Name));
                    }
                }
            }

            if (cantidadPropiedadesVacias == arrayProperties.Length)
            {
                return new EmptyValidationResult();
            }
        }
        return result;
    }       

    public static void ValidarAtributosHijos(ICollection collection, ValidationResult result)
    {
        if (collection != null && collection.Count != 0)
        {
            foreach (var item in collection)
            {
                if (typeof(IValidable).IsAssignableFrom(item.GetType()))
                {
                    var obj = (IValidable)item;
                    var arrayProperties = obj.GetType().GetProperties();
                    for (int i = 0; i < arrayProperties.Length; i++)
                    {
                        PropertyInfo p = arrayProperties[i];
                        object currentVal = p.GetValue(obj);
                        if (currentVal is ICollection)
                        {
                            ValidarAtributosHijos((ICollection)currentVal, result);
                        }
                        else
                        {
                            var arrValidationAttributes = p.GetCustomAttributes(typeof(ValidatorAttribute));
                            if (arrValidationAttributes == null) continue;

                            foreach (ValidatorAttribute validationAttr in arrValidationAttributes)
                            {
                                if (validationAttr == null) continue;
                                if (validationAttr.Validate(currentVal, out string mensajeValidacion) == false)
                                {
                                    result.Observaciones.Add(string.Format(mensajeValidacion, p.Name));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

}