using HermesControl.Consumer.Domain.Gateways;
using HermesControl.Consumer.Domain.UseCases.Requests;
using Microsoft.Extensions.Logging;

namespace HermesControl.Consumer.Domain.UseCases;

public class OrderUseCase
(
    ILogger<OrderUseCase> logger,
    IOrderGateway orderGateway,
    IPaymentGateway paymentService
) : IOrderUseCase
{
    private readonly ILogger<OrderUseCase> _logger = logger;
    private readonly IOrderGateway _orderGateway = orderGateway;
    private readonly IPaymentGateway _paymentService = paymentService;

    public async Task UpdateStatusOrderAsync(UpdateOrderStatusRequest orderRequest)
    {
        try
        {
            var order = await _orderGateway.GetById(orderRequest.OrderId) ??
                throw new NullReferenceException("Order not found");

            order = order.ChangeStatus((ValueObjects.Status)orderRequest.Status);
            await _orderGateway.UpdateAsync(order);
            _logger.LogInformation($"Order {order.Id} status changed to {order.Status}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }
}
