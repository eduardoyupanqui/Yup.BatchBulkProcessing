using Yup.BulkProcess.Contracts.Response;
using Yup.Soporte.Domain.AggregatesModel.Bloques;
using System;
using System.Collections.Generic;

namespace Yup.Soporte.Api.Application.Services.Interfaces;

public interface ICargaConsultaService<TBloquePersistencia, TFilaPersistencia, TFilaResponse>
                                                  where TBloquePersistencia : BloqueCarga<TFilaPersistencia>
                                                  where TFilaPersistencia : FilaArchivoCarga
                                                  where TFilaResponse : ProcesoMasivoResponseBase, new()

{
    IEnumerable<TFilaResponse> ObtenerFilas(Guid idArchivoCarga, bool? esValido = null);
    TFilaResponse ConvertirAResponse(TFilaPersistencia fila);
}
