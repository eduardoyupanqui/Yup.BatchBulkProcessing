using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Yup.BulkProcess;
using Yup.Core;
using Yup.Soporte.Domain.AggregatesModel.ArchivoCargaAggregate;
using Yup.Soporte.Domain.AggregatesModel.Bloques;
using Yup.Soporte.Domain.SeedworkMongoDB;
using Yup.Student.BulkProcess.Application.IntegrationEvents.Events;
using Yup.Student.BulkProcess.Application.Queries;
using Yup.Student.BulkProcess.Application.Validations;
using Yup.Student.BulkProcess.Infrastructure.Services;
using Yup.Student.Domain.Validations;

namespace Yup.Student.BulkProcess.Application.Commands;

public class CrearStudentBlockBulkCommand : IRequest<GenericResult>
{
    public Guid GuidArchivo { get; private set; }
    public Guid GuidBloque { get; private set; }
    public CrearStudentBlockBulkCommand(Guid guidArchivo, Guid guidBloque)
    {
        GuidArchivo = guidArchivo;
        GuidBloque = guidBloque;
    }

    public class CrearStudentBlockBulkCommandHandler : IRequestHandler<CrearStudentBlockBulkCommand, GenericResult>
    {
        private readonly ILogger _logger;
        private readonly IEventBus _eventBus;
        private readonly IArchivoCargaRepository _archivoCargaRepository;
        private readonly ITransversalQueries _transversalQueries;

        private readonly IConsultaBloqueService<BloquePersonas, FilaArchivoPersona, Yup.Student.Domain.AggregatesModel.StudentAggregate.Student> _bloqueStudentConsultaService;
        private IProcesoBloqueService<BloquePersonas, FilaArchivoPersona, Student.Domain.AggregatesModel.StudentAggregate.Student> _procesoBloqueService;

        private readonly StudentValidationContextGenerator _studentValidationContextGenerator;
        public CrearStudentBlockBulkCommandHandler(
            ILogger<CrearStudentBlockBulkCommandHandler> logger,
            IEventBus eventBus,
            IArchivoCargaRepository archivoCargaRepository,
            ITransversalQueries transversalQueries,

            IConsultaBloqueService<BloquePersonas, FilaArchivoPersona, Student.Domain.AggregatesModel.StudentAggregate.Student> bloqueStudentConsultaService,
            IProcesoBloqueService<BloquePersonas, FilaArchivoPersona, Student.Domain.AggregatesModel.StudentAggregate.Student> procesoBloqueService,
            StudentValidationContextGenerator studentValidationContextGenerator)
        {
            _logger = logger;
            _eventBus = eventBus;
            _archivoCargaRepository = archivoCargaRepository;
            _transversalQueries = transversalQueries;
            _bloqueStudentConsultaService = bloqueStudentConsultaService;
            _procesoBloqueService = procesoBloqueService;
            _studentValidationContextGenerator = studentValidationContextGenerator;
        }

        public async Task<GenericResult> Handle(CrearStudentBlockBulkCommand request, CancellationToken cancellationToken)
        {
            ArchivoCarga archivoCarga = await _archivoCargaRepository.FindByIdAsync(request.GuidArchivo);
            if (archivoCarga == null) return new GenericResult(MessageType.Error, "No se encontro el archivo carga.");

            var result = await ProcesarBloqueCarga(archivoCarga, request.GuidBloque);
            return result;
        }

        private async Task<GenericResult> ProcesarBloqueCarga(ArchivoCarga archivoCarga, Guid guidBloque)
        {
            GenericResult result = new GenericResult();
            string hostName = System.Net.Dns.GetHostName();

            _procesoBloqueService.SetAuditoriaInfo(usuarioAutor: archivoCarga.UsuarioCreacion, ipOrigen: archivoCarga.IpCreacion, hostNameOrigen: hostName);
            _procesoBloqueService.SetValidator(modelValidator: await GenerarValidadorAprovisionado(archivoCarga));

            //3) Enlace de eventos
            _procesoBloqueService.InicioProcesoAsync = OnProcesoBloqueServiceEventoInicioProceso;
            _procesoBloqueService.ProgresoProcesoAsync = OnProcesoBloqueServiceEventoProgresoProceso;
            _procesoBloqueService.FinProcesoAsync = OnProcesoBloqueServiceEventoFinProceso;
            //4) Invocación del proceso
            await _procesoBloqueService.ProcesarBloqueDeArchivoAsync(archivoCarga.Id, guidBloque);


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
        private Task OnProcesoBloqueServiceEventoInicioProceso(ProcesoArchivoCargaEventArgs args)
        {
            return Task.CompletedTask;
        }
        private async Task OnProcesoBloqueServiceEventoProgresoProceso(ProcesoArchivoCargaEventArgs args)
        {
            await _eventBus.Publish(new ProcesoCargaStatusIntegrationEvent(idEntidad: args.CodigoEntidad,
                                                          procesoId: args.IdArchivoCarga,
                                                          total: args.ContadoresProceso.TotalElementos,
                                                          evaluados: args.ContadoresProceso.Evaluados,
                                                          evaluadosValidos: args.ContadoresProceso.EvaluadosValidos,
                                                          evaluadosObservados: args.ContadoresProceso.EvaluadosObservados
                                                          ));
        }
        private async Task OnProcesoBloqueServiceEventoFinProceso(ProcesoArchivoCargaEventArgs args)
        {
            ArchivoCarga objArchivoCarga = await _archivoCargaRepository.FindByIdAsync(args.IdArchivoCarga);
            if (objArchivoCarga == null) { return; }

            try
            {
                await _eventBus.Publish(new ProcesoCargaStatusIntegrationEvent(idEntidad: args.CodigoEntidad,
                                                               procesoId: args.IdArchivoCarga,
                                                               total: args.ContadoresProceso.TotalElementos,
                                                               evaluados: args.ContadoresProceso.Evaluados,
                                                               evaluadosValidos: args.ContadoresProceso.EvaluadosValidos,
                                                               evaluadosObservados: args.ContadoresProceso.EvaluadosObservados
                                                               ));


                var lstRegistrosParaInsercion = _bloqueStudentConsultaService.ObtenerFilasDeArchivoModelValidas(args.IdArchivoCarga).ToList();



                //Cambiando estado de archivoCarga a "40-Finalizado"
                _archivoCargaRepository.UpdateStatus(objArchivoCarga, EstadoCarga.FINALIZADO, actualizarFechaAsociada: true);

                await NotificarProcesoMasiva(args.IdArchivoCarga);

                await _eventBus.Publish(new ProcesoCargaStatusIntegrationEvent(idEntidad: args.CodigoEntidad,
                                                   procesoId: args.IdArchivoCarga,
                                                   total: args.ContadoresProceso.TotalElementos,
                                                   evaluados: args.ContadoresProceso.Evaluados,
                                                   evaluadosValidos: args.ContadoresProceso.EvaluadosValidos,
                                                   evaluadosObservados: args.ContadoresProceso.EvaluadosObservados
                                                   ));

            }
            catch (Exception ex)
            {
                _archivoCargaRepository.UpdateStatus(objArchivoCarga, EstadoCarga.REGISTRADO_CON_ERRORES, actualizarFechaAsociada: false);
                _logger.LogError(ex, $"Ocurrió un error al migrar el registro. Por favor reintente.");
            }
            _logger.LogInformation($"El proceso terminó: - {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}");
        }

        private async Task<bool> NotificarProcesoMasiva(Guid guidArchivoCarga)
        {
            var archivoCarga = await _archivoCargaRepository.FindByIdAsync(guidArchivoCarga);
            var entidad = await _transversalQueries.ObtenerEntidadAsync(archivoCarga.IdEntidad);
            Dictionary<string, string> dParametroPlantilla = new Dictionary<string, string>();
            dParametroPlantilla.Add("entidad", entidad.Nombre);
            dParametroPlantilla.Add("fechaCarga", archivoCarga.FechaCreacion.Value.ToString("dd/MM/yyyy"));
            dParametroPlantilla.Add("horaCarga", archivoCarga.FechaCreacion.Value.ToString("HH:mm:ss"));
            dParametroPlantilla.Add("tipoArchivoCarga", "cursos");
            dParametroPlantilla.Add("archivoCarga", archivoCarga.Nombre);

            dParametroPlantilla.Add("totalValidos", archivoCarga.CantidadEvaluadosValidos.ToString());
            dParametroPlantilla.Add("totalObservados", archivoCarga.CantidadEvaluadosObservados.ToString());
            dParametroPlantilla.Add("totalRegistros", archivoCarga.CantidadTotalElementos.ToString());

            dParametroPlantilla.Add("guidArchivo", guidArchivoCarga.ToString());
            var @enviarCorreoEvent = new EnviarNotificacionIntegrationEvent(entidad.CodigoEntidad, "00022", dParametroPlantilla);
            await _eventBus.Publish(@enviarCorreoEvent);
            return true;
        }
    }
}
