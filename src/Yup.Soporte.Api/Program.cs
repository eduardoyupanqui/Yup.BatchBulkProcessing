using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Yup.Soporte.Api.Application.IntegrationEvents;
using Yup.Soporte.Api.Infrastructure.AutofacModules;
using Yup.Soporte.Api.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Container
builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
{
    builder.RegisterModule(new ApplicationModule());
    builder.RegisterModule(new MediatorModule());
    builder.RegisterAssemblyModules<CargaMasivaInitModule>(Assembly.GetExecutingAssembly());
    builder.RegisterAssemblyModules<CargaSpecModule>(Assembly.GetExecutingAssembly());
});

#region CargaMasivaSettings
builder.Services.Configure<CargaMasivaSettings>(builder.Configuration.GetSection(nameof(CargaMasivaSettings)));
builder.Services.AddSingleton(provider =>
{
    var cargaMasivaSettings = provider.GetService<IOptions<CargaMasivaSettings>>()?.Value;
    #region Lectura de PathServerFile
    cargaMasivaSettings.RutaBaseArchivos = builder.Configuration.GetValue<string>("PathServerFile:Carga:Archivo");
    #endregion
    return cargaMasivaSettings;
});
builder.Services.AddTransient<ISoporteIntegrationEventService, SoporteIntegrationEventService>();
#endregion

#region MongoDb
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection(nameof(MongoDBSettings)));
//BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;
//BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

builder.Services.AddSingleton<IMongoClient>(sp => {
    var mongoDbSettings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    return new MongoClient(mongoDbSettings.ConnectionString);
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var mongoDbSettings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    var _url = MongoUrl.Create(mongoDbSettings.ConnectionString);

    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(_url.DatabaseName);
});
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


public partial class Program
{
    public static string AppName { get; } = "Yup.Soporte";
}