using HermesControl.Consumer.Domain;
using HermesControl.Consumer.Domain.UseCases;
using HermesControl.Consumer.Domain.UseCases.Requests;
using HermesControl.Consumer.Domain.ValueObjects;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;

namespace HermesControl.Consumer.Consumer;

public class FinishedWorker : BackgroundService
{
    private readonly ILogger<FinishedWorker> _logger;
    private readonly IOrderQueue _orderQueue;
    private readonly IPaymentGateway _paymentService;
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
    private readonly IOrderUseCase _orderUseCase;

    public FinishedWorker(ILogger<FinishedWorker> logger, IOrderQueue orderQueue, IPaymentGateway paymentService, IOrderUseCase orderUseCase)
    {
        _logger = logger;
        _orderQueue = orderQueue;
        _paymentService = paymentService;
        _orderUseCase = orderUseCase;

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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Aguardando mensagens...");

        while (!stoppingToken.IsCancellationRequested)
        {
            var order = await _orderQueue.ConsumeAsync(Status.PaymentPending.ToString());

            if (order is not null)
            {
                _logger.LogInformation($"Pedido recebido: {order.Id}");

                try
                {
                    // Usa o Circuit Breaker para envolver o pagamento
                    await _circuitBreakerPolicy.ExecuteAsync(async () =>
                    {
                        // Processa o pagamento
                        var payment = new Payment(order.TotalOrder);
                        payment = _paymentService.PayAsync(payment);
                        _logger.LogInformation($"Pagamento realizado: {payment.Id}");

                        // Atualiza o status do pedido
                        await _orderUseCase.UpdateStatusOrderAsync(new UpdateOrderStatusRequest(order.Id, (int)Status.Finished));
                        _logger.LogInformation($"Pedido atualizado: {order.Id}");

                        // Publica o pedido na fila
                        _orderQueue.Publish(order);
                        _logger.LogInformation($"Pedido enviado para a fila: {order.Id}");
                    });
                }
                catch (BrokenCircuitException ex)
                {
                    _logger.LogError($"Circuito aberto, rejeitando processamento do pedido {order.Id}. Erro: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro ao processar o pedido {order.Id}: {ex.Message}");
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}

