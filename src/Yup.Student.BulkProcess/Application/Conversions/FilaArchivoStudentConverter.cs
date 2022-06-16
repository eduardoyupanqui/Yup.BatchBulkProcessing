using Yup.BulkProcess;
using Yup.Soporte.Domain.AggregatesModel.Bloques;
using System;

namespace Yup.Student.BulkProcess.Application.Conversions;

/// <summary>
/// Conversor especializado para validación masiva
/// </summary>
public class FilaArchivoStudentConverter : IFilaArchivoCargaConverter<FilaArchivoPersona, Yup.Student.Domain.AggregatesModel.StudentAggregate.Student>
{
    public Yup.Student.Domain.AggregatesModel.StudentAggregate.Student CovertToModel(FilaArchivoPersona fila)
    {
        var result = new Yup.Student.Domain.AggregatesModel.StudentAggregate.Student(
                tipoDocumento: fila.tipo_documento,
                nroDocumento: fila.nro_documento.ToUpper(),
                lenguaNativa: fila.lengua_nativa,
                idiomaExtranjero: fila.idioma_extranjero,
                condicionDiscapacidad: fila.condicion_discapacidad,
                codigoORCID: fila.codigo_orcid,
                ubigeoDomicilio: fila.ubigeo_residencia,
                numeroFila: fila.NumeroFila,
                usuarioRegistro: Guid.Parse(fila.UsuarioCreacion),
                fechaRegistro: DateTime.Now,
                ipRegistro: fila.IpCreacion
            );
        return result;
    }

    public FilaArchivoPersona ReadModel(Yup.Student.Domain.AggregatesModel.StudentAggregate.Student model)
    {
        return new FilaArchivoPersona()
        {
            tipo_documento = model.TipoDocumento,
            nro_documento = model.NroDocumento,
            lengua_nativa = model.LenguaNativa ?? "",
            idioma_extranjero = model.IdiomaExtranjero ?? "",
            condicion_discapacidad = model.CondicionDiscapacidad ?? "",
            codigo_orcid = model.CodigoORCID ?? "",
            ubigeo_residencia = model.UbigeoDomicilio ?? "",
            UsuarioCreacion = model.UsuarioCreacion.ToString(),
            IpCreacion = model.IpCreacion,
            NumeroFila = model.NumeroFila,

        };
    }

    public FilaArchivoPersona ReadFilaModel(Yup.Student.Domain.AggregatesModel.StudentAggregate.Student model, FilaArchivoPersona fila)
    {
        return fila;
    }
}
