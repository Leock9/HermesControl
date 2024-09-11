using HermesControl.Orchestrator.Domain.UseCases;
using HermesControl.Orchestrator.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;

namespace HermesControl.Orchestrator
{
    public class Orchestrator
    {
        private readonly ILogger<Orchestrator> _logger;
        private readonly IOrderQueue _orderQueue;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

        public Orchestrator
        (
            IOrderQueue orderQueue,
            ILogger<Orchestrator> logger
        )
        {
            _orderQueue = orderQueue;
            _logger = logger;

            // Definição do Circuit Breaker
            _circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30),
                    onBreak: (exception, timespan) =>
                    {
                        _logger.LogWarning($"Circuito aberto por {timespan.TotalSeconds} segundos devido a erro: {exception.Message}");
                    },
                    onReset: () => _logger.LogInformation("Circuito fechado, a operação foi restabelecida."),
                    onHalfOpen: () => _logger.LogInformation("Circuito em modo half-open, testando novamente...")
                );
        }

        public async Task ExecuteAsync(Status status, CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _circuitBreakerPolicy.ExecuteAsync(async () =>
                    {
                        // Step 1: Consumir pedido da fila
                        _logger.LogInformation("Step 1: Recuperando pedidos novos...");
                        var orderMessage = await _orderQueue.ConsumeAsync(status.ToString());

                        // Checa novamente o token de cancelamento após a operação assíncrona
                        if (stoppingToken.IsCancellationRequested)
                        {
                            _logger.LogWarning("Execução cancelada.");
                            return;
                        }

                        _logger.LogInformation($"Pedido {orderMessage.Id} recuperado.");

                        // Step 2: Validação de CPF (ou outra validação necessária)

                        // Step 3: Publicar na fila de simulação de pagamento
                        _logger.LogInformation($"Step 2: Simulando estoque para o pedido:{orderMessage.Id}. EventId:{orderMessage.EventId}");
                        orderMessage.Status = Status.SimulateOrder;
                        _orderQueue.Publish(orderMessage);
                        _logger.LogInformation($"Pedido {orderMessage.Id} publicado para a fila de simulação de pedido.");

                        // Step 4: Consumir status do pedido da fila de status
                        _logger.LogInformation("Step 3: Aguardando status do pedido...");
                        var orderMessageStatus = await _orderQueue.ConsumeAsync("OrderStatus");

                        if (stoppingToken.IsCancellationRequested)
                        {
                            _logger.LogWarning("Execução cancelada.");
                            return;
                        }

                        _logger.LogInformation($"Pedido:{orderMessageStatus.Id} com status:{orderMessageStatus.Status} recebido.");

                        // Step 5: Publicar nas filas de status
                        _orderQueue.Publish(orderMessageStatus);
                        _logger.LogInformation($"Status do pedido {orderMessageStatus.Id} publicado na fila.");
                    });

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Saga completed successfully.");
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Execução cancelada através do CancellationToken.");
                }
                catch (BrokenCircuitException ex)
                {
                    _logger.LogError($"Circuito aberto, rejeitando a execução da saga. Erro: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Saga falhou: {ex.Message}");
                    await CompensateAsync();  // Compensação em caso de falha
                }

                // Delay de 1 segundo entre execuções, respeitando o CancellationToken
                await Task.Delay(1000, stoppingToken);
            }

            _logger.LogInformation("Execução finalizada devido ao cancelamento.");
        }

        private async Task CompensateAsync()
        {
            _logger.LogWarning("Executando lógica de compensação...");
            await Task.CompletedTask;
        }
    }
}
