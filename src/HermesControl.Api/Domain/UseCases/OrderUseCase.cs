using HermesControl.Api.Domain.Gateways;
using HermesControl.Api.Domain.UseCases.Requests;

namespace HermesControl.Api.Domain.UseCases;

public class OrderUseCase
(
    ILogger<OrderUseCase> logger,
    IOrderGateway orderGateway,
    IOrderQueue queue
) : IOrderUseCase
{
    private readonly ILogger<OrderUseCase> _logger = logger;
    private readonly IOrderGateway _orderGateway = orderGateway;
    private readonly IOrderQueue _queue = queue;

    public Guid CreateOrder(BaseOrderRequest orderRequest)
    {
        try
        {
            var payment = new Payment(orderRequest.TotalOrder);
            var order = new Order
            (
                orderRequest.TotalOrder,
                orderRequest.Document,
                orderRequest.ItemMenuIds,
                payment
            );

            _orderGateway.Create(order);

            if (order.Status == ValueObjects.Status.NewOrder)
                _queue.Publish(order);

            _logger.LogInformation($"OrderId:{order.Id} created with success. Event send to broker{order.EventId}");

            return order.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<Order>> GetAll()
    {
        try
        {
            var orders = await _orderGateway.GetAll();

            return orders
                  .OrderBy(x => x.CreatedAt)
                  .Where(x => x.Status != ValueObjects.Status.Finished);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }
}
