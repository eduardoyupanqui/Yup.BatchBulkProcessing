using MediatR;
using Yup.Core;
using Yup.Enumerados;

namespace Yup.Soporte.Api.Application.Commands;

public class CrearArchivoCommand : AuditoriaCommand, IRequest<GenericResult>
{
    public int IdArchivo { get; private set; }
    public string CodigoEntidad { get; private set; }
    public int IdEntidad { get; private set; }
    public int? TipoGestion { get; private set; }
    public string NombreEntidad { get; private set; }
    public string Ruta { get; private set; }
    public string Nombre { get; private set; }
    public string Extension { get; private set; }
    public long Tamanio { get; private set; }
    public bool? TieneFirmaDigital { get; private set; }
    public int IdTblTipoCarga { get; private set; }
    public int IdTblEstadoCarga { get; private set; }
    public DateTime? FechaProcesamientoInicio { get; private set; }
    public DateTime? FechaValidacionInicio { get; private set; }
    public DateTime? FechaValidacionFin { get; private set; }
    public DateTime? FechaProcesamientoFin { get; private set; }
    public int CantidadRegistrosTotal { get; private set; }
    public int? CantidadRegistrosValidos { get; private set; }
    public int? CantidadRegistrosObservados { get; private set; }
    public int? CantidadRegistrosProcesados { get; private set; }
    public bool EsActivo { get; private set; }
    public bool EsPlantilla { get; private set; }
    public string DatosAdicionales { get; set; }

    public bool EstadoMatrizEntidadEstudiante { get; set; }

    public CrearArchivoCommand(
        string codigoEntidad
        , int idEntidad
        , int? tipoGestion
        , string nombreEntidad
        , string ruta
        , string nombre
        , string extension
        , long tamanio
        , bool? tieneFirmaDigital
        , int idTblTipoCarga
        , int cantidadRegistrosTotal
        , string datosAdicionales
        , bool esPlantilla

        , bool estadoMatrizEntidadEstudiante
    )
    {
        CodigoEntidad = codigoEntidad;
        IdEntidad = idEntidad;
        TipoGestion = tipoGestion;
        NombreEntidad = nombreEntidad;
        Ruta = ruta;
        Nombre = nombre;
        Extension = extension;
        Tamanio = tamanio;
        TieneFirmaDigital = tieneFirmaDigital;
        IdTblTipoCarga = idTblTipoCarga;
        IdTblEstadoCarga = 1;
        CantidadRegistrosTotal = cantidadRegistrosTotal;
        DatosAdicionales = datosAdicionales;
        EsActivo = true;
        EsPlantilla = esPlantilla;
        if (!string.IsNullOrEmpty(Ruta)) Ruta = Ruta.Trim();
        if (!string.IsNullOrEmpty(Nombre)) Nombre = Nombre.Trim();
        if (!string.IsNullOrEmpty(Extension)) Extension = Extension.Trim();

        EstadoMatrizEntidadEstudiante = estadoMatrizEntidadEstudiante;
    }
    public class CrearArchivoCommandHandler : IRequestHandler<CrearArchivoCommand, GenericResult>
    {
        private readonly IMediator _mediator;
        public CrearArchivoCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<GenericResult> Handle(CrearArchivoCommand message, CancellationToken cancellationToken)
        {
            var cargaArchivoExcelCommand = new CrearCargaArchivoExcelCommand()
            {
                CodigoEntidad = message.CodigoEntidad,
                IdEntidad = message.IdEntidad,
                TipoGestion = message.TipoGestion,
                EntidadDescripcion = message.NombreEntidad,
                ArchivoRuta = message.Ruta,
                ArchivoNombre = message.Nombre,
                ArchivoExtension = message.Extension,
                ArchivoTamanio = message.Tamanio,
                ArchivoTieneFirmaDigital = message.TieneFirmaDigital ?? false,
                IdTblTipoCarga = (ID_TBL_FORMATOS_CARGA)message.IdTblTipoCarga,
                CantidadRegistrosTotal = message.CantidadRegistrosTotal,
                DatosAdicionales = message.DatosAdicionales,
                ArchivoBasadoEnPlantilla = message.EsPlantilla,
                UsuarioRegistro = message.UsuarioRegistro,
                IpRegistro = message.IpRegistro,
                FechaRegistro = message.FechaRegistro
            };
            #region Adaptación para incluir "flags" de permisos 
            cargaArchivoExcelCommand.FlagsPermisos.EstadoMatrizEntidadEstudiante = message.EstadoMatrizEntidadEstudiante;
            #endregion
            var cargaArchivoExcelResponse = await _mediator.Send(cargaArchivoExcelCommand);
            return cargaArchivoExcelResponse;
        }
    }
}
