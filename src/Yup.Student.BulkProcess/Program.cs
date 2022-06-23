using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Utf8Json;
using Yup.Student.BulkProcess.Infrastructure.AutofacModules;
using Yup.Student.BulkProcess.Settings;

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
});

#region MongoDb
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection(nameof(MongoDBSettings)));

BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

builder.Services.AddSingleton<IMongoClient>(sp => {
    var mongoDbSettings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    var mongoClientSettings = MongoClientSettings.FromUrl(new MongoUrl(mongoDbSettings.ConnectionString));
    mongoClientSettings.GuidRepresentation = GuidRepresentation.Standard;
#if DEBUG
    mongoClientSettings.ClusterConfigurator = cb =>
    {
        cb.Subscribe<MongoDB.Driver.Core.Events.CommandStartedEvent>(e => {
            Console.WriteLine($"{e.CommandName} -> {e.Command.ToJson()}");
        });
    };
#endif
    return new MongoClient(mongoClientSettings);
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var mongoDbSettings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    var _url = MongoUrl.Create(mongoDbSettings.ConnectionString);

    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(_url.DatabaseName);
});
#endregion

#region Redis
builder.Services.AddSingleton(builder.Configuration.GetSection("Redis").Get<RedisConfiguration>());
builder.Services.AddSingleton<IRedisCacheClient, RedisCacheClient>();
builder.Services.AddSingleton<IRedisCacheConnectionPoolManager, RedisCacheConnectionPoolManager>();
//builder.Services.AddSingleton<IRedisDefaultCacheClient, RedisDefaultCacheClient>();
builder.Services.AddSingleton((provider) =>
{
    return provider.GetRequiredService<IRedisCacheClient>().GetDbFromConfiguration();
});
builder.Services.AddSingleton<ISerializer, Utf8JsonSerializer>();
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
