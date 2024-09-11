using HermesControl.Consumer.Consumer;
using HermesControl.Consumer.Domain;
using HermesControl.Consumer.Domain.Gateways;
using HermesControl.Consumer.Domain.UseCases;
using HermesControl.Consumer.Infrastructure.OrderGateway.PostgreDb;
using HermesControl.Consumer.Infrastructure.PaymentGateway;
using HermesControl.Consumer.Infrastructure.PaymentGateway.Weebhook;
using HermesControl.Consumer.Infrastructure.RabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Console.WriteLine("Aguardando 2 minutos para iniciar a execução dos workers...");
await Task.Delay(120000);

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // Configuração do logger
        services.AddLogging(config =>
        {
            config.AddConsole();  // Adiciona o log no console
            config.AddDebug();    // Adiciona o log no debug (útil para debugging local)
        });

        // Registro da classe PaymentWorker como um serviço em segundo plano
        services.AddHostedService<PaymentWorker>();
        services.AddHostedService<PreparationWorker>();
        services.AddHostedService<ReadyWorker>();
        services.AddHostedService<FinishedWorker>();


        // Registro das dependências do PaymentWorker
        services.AddScoped<IOrderQueue, OrderQueue>();            // Implementação do IOrderQueue
        services.AddScoped<IPaymentGateway, PaymentGateway>();    // Implementação do IPaymentGateway
        services.AddScoped<IOrderGateway, OrderGateway>();         // Implementação do OrderGateway
        services.AddScoped<IOrderUseCase, OrderUseCase>();        // Implementação do IOrderUseCase
        services.AddScoped<IPaymentWebHook, PaymentWebHook>();    // Implementação do IPaymentWebHook

        // Configuração do PostgreSQL
        var postgreDbSettings = hostContext.Configuration.GetSection("PostgreDbSettings").Get<PostgreDbSettings>();

        services.AddSingleton<Context>(sp =>
            new Context(postgreDbSettings.PostgresConnection));

        services.AddSingleton<IRabbitMqSettings>(_ => hostContext.Configuration.GetSection("RabbitMqSettings").Get<RabbitMqSettings>());
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();  // Log no console para monitorar o serviço em execução
    });

var host = builder.Build();

await host.RunAsync();
