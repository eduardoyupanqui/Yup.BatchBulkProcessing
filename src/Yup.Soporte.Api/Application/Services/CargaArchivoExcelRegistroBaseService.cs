using OfficeOpenXml;
using Yup.Soporte.Api.Application.Commands;
using Yup.Soporte.Api.Application.Queries;
using Yup.Soporte.Api.Application.Services;
using Yup.Soporte.Api.Settings;
using Yup.Soporte.Domain.AggregatesModel.ArchivoCargaAggregate;
using Yup.Soporte.Domain.AggregatesModel.Bloques;
using Yup.Core;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace Yup.Soporte.Api.Application.Services;

public abstract class CargaArchivoExcelRegistroBaseService<TBloqueCarga, TFilaArchivoCarga> :
                                                     CargaRegistroBaseService<TBloqueCarga, TFilaArchivoCarga>
                                                                                           where TBloqueCarga : BloqueCarga<TFilaArchivoCarga>, new()
                                                                                           where TFilaArchivoCarga : FilaArchivoCarga
{


    protected readonly CargaMasivaSettings _cargaMasivaSettings;

    public CargaArchivoExcelRegistroBaseService(
        IArchivoCargaRepository archivoCargaRepository,
         IBloqueCargaGenericRepository bloquesGenericRepository,
         IGenericCargaQueries genericCargaQueries,
         CargaMasivaSettings cargaMasivaSettings
    ) : base(archivoCargaRepository, bloquesGenericRepository, genericCargaQueries)
    {
        _cargaMasivaSettings = cargaMasivaSettings ?? throw new ArgumentNullException(nameof(cargaMasivaSettings));
    }

    protected GenericResult<ExcelPackage> ObtenerArchivoOrigen(CrearCargaArchivoExcelCommand command)
    {
        var result = new GenericResult<ExcelPackage>();
        ExcelPackage excelPackage = null;
        Stream excelStream;

        IFileProvider provider = new PhysicalFileProvider(Path.Combine(_cargaMasivaSettings.RutaBaseArchivos));
        IFileInfo fileInfo = provider.GetFileInfo(command.ArchivoRuta);
        excelStream = fileInfo.CreateReadStream();


        if (excelStream == null)
        {
            return new GenericResult<ExcelPackage>(MessageType.Error, "No se pudo tener acceso al archivo físico. Por favor notifíquelo al administrador del sistema.");
        }
        excelPackage = new ExcelPackage(excelStream);
        if (excelPackage.Workbook.Worksheets.Count < 2)
        {
            excelStream.Close();
            return new GenericResult<ExcelPackage>(MessageType.Error, "El archivo especificado no cuenta con hojas suficientes. Por favor vuelva a cargar el archivo respetando el formato de la plantilla.");
        }
        result.DataObject = excelPackage;
        return result;
    }

    protected async Task<GenericResult<Guid>> RegistrarArchivoCarga(CrearCargaArchivoExcelCommand command)
    {
        GenericResult<Guid> result = await base.RegistrarCarga(command,
            new CrearCargaCommandArchivoData()
            {
                Ruta = command.ArchivoRuta,
                Nombre = command.ArchivoNombre,
                Extension = command.ArchivoExtension,
                Tamanio = command.ArchivoTamanio,
                TieneFirmaDigital = command.ArchivoTieneFirmaDigital
            });

        return result;
    }
}
