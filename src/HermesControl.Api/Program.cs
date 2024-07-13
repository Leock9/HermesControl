using Amazon.SQS;
using Amazon.SQS.Model;
using FastEndpoints;
using FastEndpoints.Swagger;
using HermesControl.Api;
using HermesControl.Api.Domain;
using HermesControl.Api.Domain.Gateways;
using HermesControl.Api.Domain.UseCases;
using HermesControl.Api.Infrastructure.AwsSqs;
using HermesControl.Api.Infrastructure.AwsSqs.Configuration;
using HermesControl.Api.Infrastructure.OrderGateway.PostgreDb;
using HermesControl.Api.Infrastructure.PaymentGateway;
using HermesControl.Api.Infrastructure.PaymentGateway.Weebhook;
using LocalStack.Client.Extensions;

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
builder.Services.AddScoped<IPaymentGateway, PaymentGateway>();
builder.Services.AddScoped<IOrderQueue, AwsSqsGateway>();
builder.Services.AddScoped<IPaymentWebHook, PaymentWebHook>();
builder.Services.AddScoped<IOrderGateway, OrderGateway>();

// ** AWS **
builder.Services.AddLocalStack(builder.Configuration);
builder.Services.AddAWSServiceLocalStack<IAmazonSQS>();

// ** CONFIGURATIONS **
builder.Services.AddSingleton<IAwsSqsConfiguration>(_ => builder.Configuration.GetSection("AwsSqsConfiguration").Get<AwsSqsConfiguration>());

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

if (builder.Configuration.GetSection("LocalStack").GetValue<bool>("UseLocalStack"))
{
    var sqsClient = app.Services.GetRequiredService<IAmazonSQS>();

    var queueNames = new List<string>
    {
        "PaymentPending",
        "Received",
        "Preparation",
        "Ready",
        "Finished",
        "Canceled"
    };

    foreach (var queueName in queueNames)
    {
        var createQueueRequest = new CreateQueueRequest
        {
            QueueName = queueName
        };

        var createQueueResponse = await sqsClient.CreateQueueAsync(createQueueRequest);
        Console.WriteLine($"Queue '{queueName}' created with URL: {createQueueResponse.QueueUrl}");
    }
}

app.Run();
