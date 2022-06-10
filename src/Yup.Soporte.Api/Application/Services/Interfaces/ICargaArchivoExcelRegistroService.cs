using OfficeOpenXml;
using Yup.BulkProcess.Contracts.Request;
using Yup.Soporte.Domain.AggregatesModel.Bloques;
using System.Collections.Generic;
using Yup.Soporte.Api.Application.Commands;

namespace Yup.Soporte.Api.Application.Services.Interfaces;

/// <summary>
/// Interfaz genérica de RegistroCargaArchivoExcelService para comandos de tipo "CrearCargaArchivoExcelCommand"
/// </summary>
/// <typeparam name="TCargaCommand"></typeparam>
public interface ICargaArchivoExcelRegistroService<TCargaCommand> : ICargaRegistroService<TCargaCommand>
    where TCargaCommand : CrearCargaArchivoExcelCommand
{

}

/// <summary>
/// Interfaz especializada de RegistroCargaArchivoExcelService. Se declara para cada tipo de carga (ej: postulantes, ingresantes)
/// </summary>
/// <typeparam name="TCargaCommand"></typeparam>
/// <typeparam name="TFilaOrigen"></typeparam>
/// <typeparam name="TBloqueCarga"></typeparam>
/// <typeparam name="TFilaCarga"></typeparam>
public interface ICargaArchivoExcelRegistroService<TCargaCommand, TFilaOrigen, TBloqueCarga, TFilaCarga> : ICargaArchivoExcelRegistroService<TCargaCommand>
                                                                                                             where TCargaCommand : CrearCargaArchivoExcelCommand
                                                                                                             where TFilaOrigen : ProcesoMasivoRequestBase, new()
                                                                                                             where TBloqueCarga : BloqueCarga<TFilaCarga>, new()
                                                                                                             where TFilaCarga : FilaArchivoCarga
{
    TFilaCarga ConvertirAFilaPersistencia(TFilaOrigen fila, TCargaCommand command);
    IEnumerable<TFilaOrigen> LeerFilasOrigen(ExcelPackage excelPackage, TCargaCommand command);
}

