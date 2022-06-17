using Yup.Student.Domain.AggregatesModel.StudentAggregate;
using Yup.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Yup.Student.Domain.Validations;

public class StudentValidator : BaseValidator, IValidator<Yup.Student.Domain.AggregatesModel.StudentAggregate.Student>
{
    private ValidationResult _result;
    private StudentValidationContext _context;

    public StudentValidator(StudentValidationContext context)
    {
        _context = context;
        _result = new ValidationResult();
    }
    public ValidationResult Validar(Yup.Student.Domain.AggregatesModel.StudentAggregate.Student registro)
    {
        _result = new ValidationResult();
        if (_context.operacion == OperacionCurso.Registrar)
        {
            ValidarStudentsRepetidosEnDiccionario(registro);
            ValidarLenguaNativaValido(registro);
            ValidarIdiomaExtranjeroValido(registro);
        }
        else if (_context.operacion == OperacionCurso.Modificar)
        {
            ValidarStudentsRepetidosEnDiccionario(registro);
            ValidarLenguaNativaValido(registro);
            ValidarIdiomaExtranjeroValido(registro);
        }
        else if (_context.operacion == OperacionCurso.Eliminar)
        {
        }
        return _result;
    }
    private void ValidarStudentsRepetidosEnDiccionario(Yup.Student.Domain.AggregatesModel.StudentAggregate.Student registro)
    {
        if (_context.ListaDeLlaves.Any(s => s.Equals(registro.TipoDocumento + ":" + registro.NroDocumento, StringComparison.OrdinalIgnoreCase)))
        { 
            _result.Observaciones.Add("Documentos de identidad repetidos");
        }
    }
    public void ValidarLenguaNativaValido(Yup.Student.Domain.AggregatesModel.StudentAggregate.Student student)
    {
        if (_context.dLenguasNativas.ContainsKey(student.LenguaNativa) == false)
        {
            _result.Observaciones.Add("Lengua Nativa ingresado no existe.");
        }
    }
    public void ValidarIdiomaExtranjeroValido(Yup.Student.Domain.AggregatesModel.StudentAggregate.Student student)
    {
        if (_context.dIdiomasExtranjero.ContainsKey(student.IdiomaExtranjero) == false)
        {
            _result.Observaciones.Add("Idioma Extranjero ingresado no existe.");
        }
    }
}

public enum OperacionCurso
{
    Registrar,
    Modificar,
    Eliminar
}
public class StudentValidationContext : BaseValidationContext
{
    /// <summary>
    /// Codigo:Nombre Students TipoDocumento-NumDocumento
    /// </summary>
    public List<string> ListaDeLlaves { get; set; }

    /// <summary>
    /// Hashset clave=IdTblLenguasNativas
    /// </summary>
    /// 
    public Dictionary<string, string> dLenguasNativas;

    /// <summary>
    /// Hashset clave=IdTblIdiomaExtranjero
    /// </summary>
    /// 
    public Dictionary<string, string> dIdiomasExtranjero;

    /// <summary>
    /// Representa a la entidad antes de la modificación
    /// </summary>
    /// 
    public Yup.Student.Domain.AggregatesModel.StudentAggregate.Student studentEstadoActual;

    public OperacionCurso operacion;
}


