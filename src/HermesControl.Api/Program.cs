using FastEndpoints;
using FastEndpoints.Swagger;
using HermesControl.Api;
using HermesControl.Api.Domain.Gateways;
using HermesControl.Api.Domain.UseCases;
using HermesControl.Api.Infrastructure.OrderGateway.PostgreDb;
using HermesControl.Api.Infrastructure.RabbitMq;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(options =>
{
    options.TimestampFormat = "hh:mm:ss ";
});

builder.Services.AddFastEndpoints();
builder.Services.AddHealthChecks();

builder.Services.SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.DocumentName = "swagger";
        s.Title = "Hermes Control API";
        s.Version = "v1";
        s.Description = "Documentation about endpoints";
    };

    o.EnableJWTBearerAuth = false;
    o.ShortSchemaNames = false;
    o.RemoveEmptyRequestSchema = true;
});

builder.Services.AddHttpClient();

// ** CORS **
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// ** CONTEXT POSTGRE**
var postgreDbSettings = builder.Configuration.GetSection("PostgreDbSettings").Get<PostgreDbSettings>();

builder.Services.AddSingleton<Context>
    (
    sp => new Context
                       (
                        postgreDbSettings!.PostgresConnection
                       )
    );

// ** USECASES **
builder.Services.AddScoped<IOrderUseCase, OrderUseCase>();

// ** GATEWAYS **
builder.Services.AddScoped<IOrderQueue, OrderQueue>();
builder.Services.AddScoped<IOrderGateway, OrderGateway>();

// ** CONFIGURATIONS **
builder.Services.AddSingleton<IRabbitMqSettings>(_ => builder.Configuration.GetSection("RabbitMqSettings").Get<RabbitMqSettings>());

var app = builder.Build();

// Use CORS
app.UseCors("AllowAll");

app.MapHealthChecks("/health");

app.UseFastEndpoints(c =>
{
    c.Endpoints.ShortNames = false;

    c.Endpoints.Configurator = ep =>
    {
        ep.Summary(s =>
        {
            s.Response<ErrorResponse>(400);
            s.Response(401);
            s.Response(403);
            s.Responses[200] = "OK";
        });

        ep.PostProcessors(FastEndpoints.Order.After, new GlobalLoggerPostProcces
        (
            LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            }).CreateLogger<GlobalLoggerPostProcces>()
        ));
    };
}).UseSwaggerGen();
app.Run();
