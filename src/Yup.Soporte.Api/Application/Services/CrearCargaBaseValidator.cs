using Yup.Enumerados;
using Yup.Soporte.Api.Application.Commands;
using Yup.Soporte.Api.Application.Queries;
using Yup.Soporte.Api.Settings;
using Yup.Soporte.Domain.AggregatesModel.ArchivoCargaAggregate;
using Yup.Core;
using System;
using System.Threading.Tasks;

namespace Yup.Soporte.Api.Application.Services
{
    public abstract class CrearCargaBaseValidator
    {
        protected readonly IGenericCargaQueries _genericCargaQueries;
        protected readonly CargaMasivaSettings _cargaMasivaSettings;
        private readonly IArchivoCargaRepository _archivoCargaRepository;

        public CrearCargaBaseValidator(CargaMasivaSettings cargaMasivaSettings,
             IArchivoCargaRepository archivoCargaRepository,
                                                                        IGenericCargaQueries genericCargaQueries)
        {
            _genericCargaQueries = genericCargaQueries ?? throw new ArgumentNullException(nameof(genericCargaQueries));
            _cargaMasivaSettings = cargaMasivaSettings ?? throw new ArgumentNullException(nameof(cargaMasivaSettings));
            _archivoCargaRepository = archivoCargaRepository ?? throw new ArgumentNullException(nameof(archivoCargaRepository));
        }

        public async Task<GenericResult<Guid>> ValidarGenerico(CrearCargaCommand message, bool completarDatosDescriptivos = false)
        {
            var response = new GenericResult<Guid>();
            //UsuarioResponseDto objUsuarioCreacion = null;
            //EntidadResponseDto objEntidad = null;

            if (message == null)
            { return new GenericResult<Guid>(MessageType.Error, "Solicitud no válida"); }

            if (message.IdTblTipoCarga == 0 || Enum.IsDefined(typeof(ID_TBL_FORMATOS_CARGA), message.IdTblTipoCarga) == false)
            { return new GenericResult<Guid>(MessageType.Error, "El nombre del archivo no corresponda a un Tipo de Archivo de Carga válido"); }

            #region Validando usuario de creación especificado
            if (message.UsuarioRegistro == Guid.Empty)
            { return new GenericResult<Guid>(MessageType.Error, "Solicitud no válida. Se requiere un identificador de usuario autor"); }
            #endregion

            #region Validando entidad
            if (string.IsNullOrWhiteSpace(message.CodigoEntidad))
            { return new GenericResult<Guid>(MessageType.Error, "Solicitud no válida. Se requiere especificar un código de entidad"); }
            #endregion


            if (_archivoCargaRepository.ExisteArchivoEnProcesoParaEntidad(message.IdEntidad, (int)message.IdTblTipoCarga))
            { return new GenericResult<Guid>(MessageType.Error, "No es posible realizar la carga mientras se tengan otra carga en proceso"); }

            return response;
        }
    }
}
