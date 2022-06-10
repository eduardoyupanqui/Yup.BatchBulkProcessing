using Yup.Soporte.Api.Application.Services.Interfaces;
using Yup.Soporte.Domain.AggregatesModel.Bloques;
using System;
using System.Collections.Generic;
using System.Linq;
using Yup.BulkProcess.Contracts.Response;
using Yup.Soporte.Api.Application.Services;

namespace Yup.Soporte.Api.Application.Services.CargaService.STUDENTS;

public class CargaConsultaService : CargaConsultaBaseService<BloquePersonas, FilaArchivoPersona>,
                                                                ICargaConsultaService<BloquePersonas, FilaArchivoPersona, DatosPersonaResponse>
{
    public CargaConsultaService(IBloqueCargaGenericRepository genericRepository) : base(genericRepository)
    {

    }
    public DatosPersonaResponse ConvertirAResponse(FilaArchivoPersona x)
    {
        return new DatosPersonaResponse()
        {
            NumeroElemento = x.NumeroFila,
            Evaluado = x.Evaluado,
            Registrado = x.Registrado,
            EsValido = x.EsValido,
            Observaciones = x.Observaciones ?? new List<string>(),

            CodigoORCID = x.codigo_orcid,
            CondicionDiscapacidad = x.condicion_discapacidad,
            IdiomaExtranjero = x.idioma_extranjero,
            LenguaNativa = x.lengua_nativa,
            NroDocumento = x.nro_documento,
            TipoDocumento = x.tipo_documento,
            UbigeoDomicilio = x.ubigeo_residencia
        };
    }

    public IEnumerable<DatosPersonaResponse> ObtenerFilas(Guid idArchivoCarga, bool? esValido = null)
    {
        return ObtenerFilasDeArchivoCarga(idArchivoCarga, esValido).Select(x => ConvertirAResponse(x)).ToList();
    }
}
