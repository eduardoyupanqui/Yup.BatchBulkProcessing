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
        private readonly IBloqueCargaGenericRepository _bloquesGenericRepository;

        private readonly IConsultaBloqueService<BloquePersonas, FilaArchivoPersona, Yup.Student.Domain.AggregatesModel.StudentAggregate.Student> _bloqueStudentConsultaService;
        public CrearStudentBulkCommandHandler(
            IEventBus eventBus, 
            ISeguimientoProcesoBloqueService seguimientoBloqueService,
            IArchivoCargaRepository archivoCargaRepository,
            IConsultaBloqueService<BloquePersonas, FilaArchivoPersona, Student.Domain.AggregatesModel.StudentAggregate.Student> bloqueStudentConsultaService)
        {
            _eventBus = eventBus;
            _seguimientoBloqueService = seguimientoBloqueService;
            _archivoCargaRepository = archivoCargaRepository;
            EnlazarEventosDeServicioDeSeguimiento();
            _seguimientoBloqueService = seguimientoBloqueService;
            _bloqueStudentConsultaService = bloqueStudentConsultaService;
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

            //1) Obtención de claves repetidas
            var lstUniqueKeysRepetidas = _bloqueStudentConsultaService.GetListUniqueKeysRepetidasDeIdArchivoCarga(archivoCarga.Id);



            return result;
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
