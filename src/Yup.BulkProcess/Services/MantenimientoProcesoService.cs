using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using Yup.Soporte.Domain.AggregatesModel.ArchivoCargaAggregate;
using Yup.Soporte.Domain.AggregatesModel.Bloques;
using Yup.Soporte.Domain.SeedworkMongoDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Yup.BulkProcess;

public abstract class MantenimientoProcesoService<TBloque, TFila> : IMantenimientoProcesoService
                                                where TBloque : BloqueCarga<TFila>
                                                where TFila : FilaArchivoCarga
{
    protected readonly ILogger _logger;
    protected readonly IBloqueCargaGenericRepository _bloqueGenericRepository;
    protected string _usuarioAutor;
    protected string _ipOrigen;
    protected string _hostNameOrigen;


    public MantenimientoProcesoService(IBloqueCargaGenericRepository bloqueGenericRepository, ILogger logger)
    {
        _bloqueGenericRepository = bloqueGenericRepository ?? throw new ArgumentNullException(nameof(bloqueGenericRepository));
        _logger = logger;
    }

    public virtual void SetAuditoriaInfo(string usuarioAutor, string ipOrigen, string hostNameOrigen) 
    {
        #region Validacion/Asimilacion de parámetros de entrada
        Guid tmpGuidUsuarioAutor;
        if (Guid.TryParse(usuarioAutor, out tmpGuidUsuarioAutor) == false) throw new ArgumentNullException(nameof(usuarioAutor));
        if (string.IsNullOrWhiteSpace(ipOrigen)) throw new ArgumentNullException(nameof(ipOrigen));
        if (string.IsNullOrWhiteSpace(hostNameOrigen)) throw new ArgumentNullException(nameof(hostNameOrigen));

        _usuarioAutor = usuarioAutor;
        _ipOrigen = ipOrigen;
        _hostNameOrigen = hostNameOrigen;
        #endregion
    }

    public bool ActualizarFilasDeArchivo(Guid idArchivoCarga, IEnumerable<FilaArchivoCargaUpdate> updates)
    {
        ContadoresProceso contadoresArchivo = null;
        return ActualizarFilasDeArchivo(idArchivoCarga, updates, out contadoresArchivo);
    }

    public bool ActualizarFilasDeArchivo(Guid idArchivoCarga, IEnumerable<FilaArchivoCargaUpdate> updates, out ContadoresProceso contadoresArchivo)
    {
        contadoresArchivo = new ContadoresProceso();

        var objArchivoCarga = _bloqueGenericRepository.GetById<ArchivoCarga>(idArchivoCarga);
        if (objArchivoCarga == null || objArchivoCarga.EsEliminado)
        {
            throw new ProcesoMasivoException("ArchivoCarga no válido");
        }
        if (updates == null || !updates.Any()) return false;

        //Obteniendo bloques de archivo
        var lstBloquesDeArchivoCarga = _bloqueGenericRepository.GetAll<TBloque>(x =>
                                                  x.IdCarga == idArchivoCarga &&
                                                  x.EsActivo == true &&
                                                  x.EsEliminado == false);

        if (lstBloquesDeArchivoCarga == null || lstBloquesDeArchivoCarga.Any() == false) return false;


        var lstUpdatesPorNumeroFilaParaBloque = new List<FilaArchivoCargaUpdate>();
        var lstUpdatesPorUniqueKey = new List<FilaArchivoCargaUpdate>();
        var dictUpdatesPorUniqueKey = new Dictionary<string, FilaArchivoCargaUpdate>();

        var existenUpdatesPorUniqueKey = false;
        var existenUpdatesPorNumeroFilaParaBloque = false;
        var cambiosAplicadosEnBloque = false;
        TFila filaEnProceso = null;

        #region Obtencion de Updates por llave única
        lstUpdatesPorUniqueKey = updates.Where(x => x.TipoUpdate == FilaArchivoCargaUpdateType.PorUniqueKey &&
                                                    string.IsNullOrWhiteSpace(x.UniqueKey) == false)
                                                    .ToList();
        lstUpdatesPorUniqueKey.ForEach(x =>
        {
            if (dictUpdatesPorUniqueKey.ContainsKey(x.UniqueKey) == false)
            {
                dictUpdatesPorUniqueKey.Add(x.UniqueKey, x);
            }
        });
        existenUpdatesPorUniqueKey = dictUpdatesPorUniqueKey.Any();
        #endregion

        var cambiosAplicadosEnArchivo = false;
        foreach (var bloque in lstBloquesDeArchivoCarga)
        {
            var contadoresBloque = new ContadoresProceso();
            cambiosAplicadosEnBloque = false;
            lstUpdatesPorNumeroFilaParaBloque = updates.Where(x => x.TipoUpdate == FilaArchivoCargaUpdateType.PorNumeroFila &&
                                                      x.NumeroFila >= bloque.FilaInicial &&
                                                      x.NumeroFila <= bloque.FilaFinal)
                                                .ToList();

            existenUpdatesPorNumeroFilaParaBloque = lstUpdatesPorNumeroFilaParaBloque.Any();

            if (existenUpdatesPorNumeroFilaParaBloque == false && existenUpdatesPorUniqueKey == false) continue; //Buscar en siguiente bloque

            for (var i = 0; i < bloque.Filas.Count; i++)
            {
                filaEnProceso = bloque.Filas[i];

                if (existenUpdatesPorNumeroFilaParaBloque)
                    foreach (var updatesDeBloque in lstUpdatesPorNumeroFilaParaBloque)
                    {
                        if (filaEnProceso.NumeroFila != updatesDeBloque.NumeroFila) continue;

                        cambiosAplicadosEnBloque = AplicarCambioAFila(bloque.Filas[i], updatesDeBloque);
                        cambiosAplicadosEnArchivo = cambiosAplicadosEnArchivo || cambiosAplicadosEnBloque;
                    }

                if (existenUpdatesPorUniqueKey && string.IsNullOrWhiteSpace(filaEnProceso.UniqueKey) == false)
                {
                    if (dictUpdatesPorUniqueKey.ContainsKey(filaEnProceso.UniqueKey))
                    {
                        cambiosAplicadosEnBloque = AplicarCambioAFila(bloque.Filas[i], dictUpdatesPorUniqueKey[filaEnProceso.UniqueKey]);
                        cambiosAplicadosEnArchivo = cambiosAplicadosEnArchivo || cambiosAplicadosEnBloque;
                    }
                }
                if (filaEnProceso.EsValido) contadoresBloque.EvaluadosValidos++; else contadoresBloque.EvaluadosObservados++;
            }

            if (cambiosAplicadosEnBloque)
            {
                var updFilasYContadoresDeBloque = Builders<TBloque>.Update
                     .Set(b => b.Filas, bloque.Filas)
                     .Set(b => b.CantidadEvaluadosValidos, contadoresBloque.EvaluadosValidos)
                     .Set(b => b.CantidadEvaluadosObservados, contadoresBloque.EvaluadosObservados)
                     .Set(b => b.FechaModificacion, DateTime.Now)
                     .Set(b => b.UsuarioModificacion, _usuarioAutor)
                     .Set(b => b.IpModificacion, _ipOrigen)
                     ;
                _bloqueGenericRepository.UpdateOne<TBloque>(bloque, updFilasYContadoresDeBloque);
            }
        }
        if (cambiosAplicadosEnArchivo)
        {
            SumarizarContadoresDeBloquesDeArchivo(idArchivoCarga, out contadoresArchivo);
        }

        return cambiosAplicadosEnArchivo;
    }

    protected bool AplicarCambioAFila(TFila fila, FilaArchivoCargaUpdate changes)
    {
        var cambioAplicado = false;

        if (changes.Registrado.HasValue)
        {
            fila.Registrado = changes.Registrado.Value;
            cambioAplicado = true;
        }
        if (changes.EsValido.HasValue)
        {
            fila.EsValido = changes.EsValido.Value;
            cambioAplicado = true;
        }

        if (changes.Observaciones != null && changes.Observaciones.Any())
        {
            if (fila.Observaciones != null && fila.Observaciones.Any())
            {
                fila.Observaciones.AddRange(changes.Observaciones);
                cambioAplicado = true;
            }
            else
            {
                fila.Observaciones = changes.Observaciones;
                cambioAplicado = true;
            }
        }
        return cambioAplicado;
    }

    public void RehabilitarArchivo(Guid idArchivoCarga)
    {
        var objArchivoCarga = _bloqueGenericRepository.GetById<ArchivoCarga>(idArchivoCarga);
        if (objArchivoCarga == null || objArchivoCarga.EsEliminado)
        {
            throw new ProcesoMasivoException("ArchivoCarga no válido");
        }

        //Reactivando archivoCarga y bloques
        var lstBloquesDeArchivoCarga = _bloqueGenericRepository.GetAll<TBloque>(x =>
                                                  x.IdCarga == idArchivoCarga &&
                                                  x.EsActivo == true &&
                                                  x.EsEliminado == false);

        foreach (var bloque in lstBloquesDeArchivoCarga)
        {
            var updBloqueRevertDatosProcesamiento = Builders<TBloque>.Update
           .Set(b => b.Estado, EstadoCarga.PENDIENTE)
           .Set(b => b.FechaEvaluacionInicio, null)
           .Set(b => b.FechaEvaluacionFin, null)
           .Set(b => b.FechaRegistroInicio, null)
           .Set(b => b.FechaRegistroFin, null)
           .Set(b => b.UsuarioModificacion, _usuarioAutor)
           .Set(b => b.FechaModificacion, DateTime.Now);

            _bloqueGenericRepository.UpdateOne(bloque, updBloqueRevertDatosProcesamiento);
        }

        #region 2) Marcando el registro como "pendiente"

        var updArchivoPendiente = Builders<ArchivoCarga>.Update
           .Set(b => b.Estado, EstadoCarga.PENDIENTE)
           .Set(b => b.FechaEvaluacionInicio, null)
           .Set(b => b.FechaEvaluacionFin, null)
           .Set(b => b.FechaRegistroInicio, null)
           .Set(b => b.FechaRegistroFin, null)
           .Set(b => b.UsuarioModificacion, _usuarioAutor)
           .Set(b => b.FechaModificacion, DateTime.Now);


        _bloqueGenericRepository.UpdateOne(objArchivoCarga, updArchivoPendiente);
        #endregion
    }

    protected void SumarizarContadoresDeBloquesDeArchivo(Guid idArchivoCarga, out ContadoresProceso contadorArchivo)
    {
        _logger.LogDebug($"Sumarizando contadores de bloques de archivo de carga id = {idArchivoCarga}");

        contadorArchivo = new ContadoresProceso();
        var objArchivoCarga = _bloqueGenericRepository.GetById<ArchivoCarga>(idArchivoCarga);

        //Obteniendo los contadores de todos los bloques del archivo
        var lstContadoresBloquesArchivo = _bloqueGenericRepository.GetCursor<TBloque>(x =>
                                                    x.IdCarga == idArchivoCarga &&
                                                    x.EsActivo == true &&
                                                    x.EsEliminado == false)
                                                    .Project(x => new ContadoresProceso()
                                                    {
                                                        TotalElementos = x.CantidadTotalElementos,
                                                        Evaluados = x.CantidadEvaluados,
                                                        EvaluadosObservados = x.CantidadEvaluadosObservados,
                                                        EvaluadosValidos = x.CantidadEvaluadosValidos,
                                                        RegistradosValidos = x.CantidadRegistradosValidos,
                                                        RegistradosFallidos = x.CantidadRegistradosFallidos,
                                                    })
                                                    .ToList();

        foreach (var contBloque in lstContadoresBloquesArchivo)
        {
            contadorArchivo.TotalElementos += contBloque.TotalElementos;
            contadorArchivo.Evaluados += contBloque.Evaluados;
            contadorArchivo.EvaluadosObservados += contBloque.EvaluadosObservados;
            contadorArchivo.EvaluadosValidos += contBloque.EvaluadosValidos;
            contadorArchivo.RegistradosValidos += contBloque.RegistradosValidos;
            contadorArchivo.RegistradosFallidos += contBloque.RegistradosFallidos;
        }

        var estadoArchivoCalculado = objArchivoCarga.Estado;
        #region Reevaluando estado en base a contadores
        //Para conteo post Evaluación
        if (contadorArchivo.Evaluados > 0)
        {
            estadoArchivoCalculado = contadorArchivo.EvaluadosObservados == 0 ?
                                        EstadoCarga.EVALUADO_CORRECTO :
                                        EstadoCarga.EVALUADO_CON_OBSERVACIONES;
        }
        //Para conteo post Registro
        if (contadorArchivo.RegistradosValidos > 0)
        {
            estadoArchivoCalculado = contadorArchivo.RegistradosFallidos == 0 ?
                                        EstadoCarga.REGISTRADO_CORRECTO :
                                        EstadoCarga.REGISTRADO_CON_ERRORES;
        }
        #endregion

        var updSumarizarContadores = Builders<ArchivoCarga>.Update
          .Set(b => b.CantidadTotalElementos, contadorArchivo.TotalElementos)
          .Set(b => b.CantidadEvaluados, contadorArchivo.Evaluados)
          .Set(b => b.CantidadEvaluadosObservados, contadorArchivo.EvaluadosObservados)
          .Set(b => b.CantidadEvaluadosValidos, contadorArchivo.EvaluadosValidos)
          .Set(b => b.CantidadRegistradosValidos, contadorArchivo.RegistradosValidos)
          .Set(b => b.CantidadRegistradosFallidos, contadorArchivo.RegistradosFallidos)
          .Set(b => b.Estado, estadoArchivoCalculado)
          .Set(b => b.FechaModificacion, DateTime.Now)
          .Set(b => b.IpModificacion, _ipOrigen)
          .Set(b => b.UsuarioModificacion, _usuarioAutor);

        _bloqueGenericRepository.UpdateOne(objArchivoCarga, updSumarizarContadores);
    }
}
