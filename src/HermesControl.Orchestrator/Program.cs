using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HermesControl.Orchestrator.Infrastructure.RabbitMq;
using HermesControl.Orchestrator.Domain.UseCases;
using HermesControl.Orchestrator;
using HermesControl.Orchestrator.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Aguardando 2 minutos para iniciar a execução da Saga...");
await Task.Delay(120000);

var builder = Host.CreateDefaultBuilder()
    .ConfigureServices((hostContext, services) =>
    {
        services.AddScoped<IOrderQueue, OrderQueue>();
        services.AddScoped<Orchestrator>();
        services.AddSingleton<IRabbitMqSettings>(_ => hostContext.Configuration.GetSection("RabbitMqSettings").Get<RabbitMqSettings>());
    });

var host = builder.Build();

// Resolvendo o orquestrador e executando a Saga
var orchestrator = host.Services.GetRequiredService<Orchestrator> ();

// Criando uma ordem fictícia
await orchestrator.ExecuteAsync(Status.NewOrder, new CancellationToken());

await host.RunAsync();
