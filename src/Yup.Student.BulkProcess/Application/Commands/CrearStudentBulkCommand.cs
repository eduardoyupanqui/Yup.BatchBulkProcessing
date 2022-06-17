using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Yup.BulkProcess;
using Yup.BulkProcess.Abstractions;
using Yup.Core;
using Yup.Soporte.Domain.AggregatesModel.ArchivoCargaAggregate;
using Yup.Soporte.Domain.AggregatesModel.Bloques;
using Yup.Student.BulkProcess.Application.Validations;
using Yup.Student.Domain.Validations;

namespace Yup.BulkProcess.Application.Commands;

public class CrearStudentBulkCommand : IRequest<GenericResult>
{
    public Guid GuidArchivo { get; private set; }
    public CrearStudentBulkCommand(Guid guidArchivo)
    {
        GuidArchivo = guidArchivo;
    }

    public class CrearStudentBulkCommandHandler : IRequestHandler<CrearStudentBulkCommand, GenericResult>
    {
        private readonly IEventBus _eventBus;
        private readonly ISeguimientoProcesoBloqueService _seguimientoBloqueService;
        private readonly IArchivoCargaRepository _archivoCargaRepository;

        private readonly IConsultaBloqueService<BloquePersonas, FilaArchivoPersona, Yup.Student.Domain.AggregatesModel.StudentAggregate.Student> _bloqueStudentConsultaService;
        private IProcesoBloqueService<BloquePersonas, FilaArchivoPersona, Student.Domain.AggregatesModel.StudentAggregate.Student> _procesoBloqueService;

        private readonly StudentValidationContextGenerator _studentValidationContextGenerator;
        public CrearStudentBulkCommandHandler(
            IEventBus eventBus,
            ISeguimientoProcesoBloqueService seguimientoBloqueService,
            IArchivoCargaRepository archivoCargaRepository,

            IConsultaBloqueService<BloquePersonas, FilaArchivoPersona, Student.Domain.AggregatesModel.StudentAggregate.Student> bloqueStudentConsultaService,
            IProcesoBloqueService<BloquePersonas, FilaArchivoPersona, Student.Domain.AggregatesModel.StudentAggregate.Student> procesoBloqueService,
            StudentValidationContextGenerator studentValidationContextGenerator)
        {
            _eventBus = eventBus;
            _seguimientoBloqueService = seguimientoBloqueService;
            _archivoCargaRepository = archivoCargaRepository;
            EnlazarEventosDeServicioDeSeguimiento();
            _seguimientoBloqueService = seguimientoBloqueService;
            _bloqueStudentConsultaService = bloqueStudentConsultaService;
            _procesoBloqueService = procesoBloqueService;
            _studentValidationContextGenerator = studentValidationContextGenerator;
        }

        private void EnlazarEventosDeServicioDeSeguimiento()
        {
            _seguimientoBloqueService.StatusUpdateAsync = OnSeguimientoProcesoArchivoStatusUpdate;
            _seguimientoBloqueService.ProcessCompletedAsync = OnSeguimientoProcesoArchivoCompletado;
        }

        public async Task<GenericResult> Handle(CrearStudentBulkCommand request, CancellationToken cancellationToken)
        {
            ArchivoCarga archivoCarga = await _archivoCargaRepository.FindByIdAsync(request.GuidArchivo);
            if (archivoCarga == null) return new GenericResult(MessageType.Error, "No se encontro el archivo carga.");

            var result = await ProcesarArchivoCarga(archivoCarga);
            return result;
        }

        private async Task<GenericResult> ProcesarArchivoCarga(ArchivoCarga archivoCarga)
        {
            GenericResult result = new GenericResult();
            string hostName = System.Net.Dns.GetHostName();

            _procesoBloqueService.SetAuditoriaInfo(usuarioAutor: archivoCarga.UsuarioCreacion, ipOrigen: archivoCarga.IpCreacion, hostNameOrigen: hostName);
            _procesoBloqueService.SetValidator(modelValidator: await GenerarValidadorAprovisionado(archivoCarga));



            return result;
        }

        private async Task<StudentValidator> GenerarValidadorAprovisionado(ArchivoCarga archivoCarga)
        {
            //1) Obtención de claves repetidas
            var lstUniqueKeysRepetidas = _bloqueStudentConsultaService.GetListUniqueKeysRepetidasDeIdArchivoCarga(archivoCarga.Id);
            StudentValidationContext validationContext = await _studentValidationContextGenerator.GenerarModelo(archivoCarga.IdEntidad, archivoCarga.CodigoEntidad, lstUniqueKeysRepetidas);
            StudentValidator validator = new StudentValidator(validationContext);
            return validator;
        }

        Task OnSeguimientoProcesoArchivoStatusUpdate(SeguimientoProcesoArchivoEventArgs args)
        {
            return Task.CompletedTask;
        }
        Task OnSeguimientoProcesoArchivoCompletado(SeguimientoProcesoArchivoEventArgs args)
        {
            return Task.CompletedTask;
        }

    }
}
