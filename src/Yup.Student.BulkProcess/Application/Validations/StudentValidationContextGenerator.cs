using Yup.Student.Domain.AggregatesModel;
using Yup.Student.Domain.Validations;
using Yup.Student.BulkProcess.Application.Queries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yup.Student.BulkProcess.Application.Validations;

public class StudentValidationContextGenerator
{
    private IEnumerable<Yup.Student.Domain.AggregatesModel.StudentAggregate.Student> _studentRepository;
    private ITransversalQueries _transversalQueries;
    public StudentValidationContextGenerator(ITransversalQueries transversalQueries)
    {
        _studentRepository = new List<Yup.Student.Domain.AggregatesModel.StudentAggregate.Student> { 
            new Domain.AggregatesModel.StudentAggregate.Student("1","4783926", "1", "1", "","","", Guid.NewGuid(), DateTime.Now, "::"),
            new Domain.AggregatesModel.StudentAggregate.Student("1","4723922", "1", "1", "","","", Guid.NewGuid(), DateTime.Now, "::"),
        };
        _transversalQueries = transversalQueries;
    }

    public async Task<StudentValidationContext> GenerarModelo(int idEntidad, string codigoEntidad, List<string> llaveUnicas)
    {
        var context = new StudentValidationContext();
        context.operacion = OperacionCurso.Registrar;
        context.dLenguasNativas = await _transversalQueries.ListarLenguasNativas();
        context.dIdiomasExtranjero = await _transversalQueries.ListarIdiomasExtranjeros();
        llaveUnicas.AddRange(_studentRepository.Select(x => x.TipoDocumento + ":" + x.NroDocumento).ToList());
        context.ListaDeLlaves = llaveUnicas;
        return context;
    }

}
