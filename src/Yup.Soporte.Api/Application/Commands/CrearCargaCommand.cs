using Yup.Enumerados;
using MediatR;
using Yup.Core;
using System;
using System.Text.Json.Serialization;

namespace Yup.Soporte.Api.Application.Commands;

public abstract class CrearCargaCommand : AuditoriaCommand, IRequest<GenericResult<Guid>>
{
    public int IdEntidad { get; set; }
    public int? TipoGestion { get; set; }
    [JsonIgnore]
    public ID_TBL_FORMATOS_CARGA IdTblTipoCarga { get; set; }
    public string CodigoEntidad { get; set; }
    [JsonIgnore]
    public virtual int CantidadRegistrosTotal { get; set; }
    [JsonIgnore]
    public int IdOrigenCarga { get; protected set; } //(0=archivoExcel,1=servicio externo)
    public string EntidadDescripcion { get; set; }
    [JsonIgnore]
    public string UsuarioCreacionDescripcion { get; set; }
}
/// <summary>
/// Clase auxiliar para registro de data específica de archivos cargas de tipo archivo
/// </summary>
public class CrearCargaCommandArchivoData
{
    ////Solo archivos
    public string Ruta { get; set; }
    public string Nombre { get; set; }
    public string Extension { get; set; }
    public long Tamanio { get; set; }
    public bool TieneFirmaDigital { get; set; }
}

