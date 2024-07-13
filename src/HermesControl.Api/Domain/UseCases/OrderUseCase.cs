using HermesControl.Api.Domain.Gateways;
using HermesControl.Api.Domain.UseCases.Requests;

namespace HermesControl.Api.Domain.UseCases;

public class OrderUseCase : IOrderUseCase
{
    private readonly ILogger<OrderUseCase> _logger;
    private readonly IOrderGateway _orderGateway;
    private readonly IPaymentGateway _paymentService;
    private readonly IOrderQueue _queue;

    public OrderUseCase
    (
        ILogger<OrderUseCase> logger,
        IOrderGateway orderGateway,
        IPaymentGateway paymentService,
        IOrderQueue queue
    )
    {
        _logger = logger;
        _orderGateway = orderGateway;
        _paymentService = paymentService;
        _queue = queue;
    }

    public Guid CreateOrder(BaseOrderRequest orderRequest)
    {
        try
        {
            var payment = new Payment(orderRequest.TotalOrder);
            payment = _paymentService.PayAsync(payment);

            var order = new Order
            (
                orderRequest.TotalOrder,
                orderRequest.Document,
                orderRequest.ItemMenuIds,
                payment
            );

            _orderGateway.Create(order);

            if (order.Status == ValueObjects.Status.Received && payment.IsAproved)
                _queue.Publish(order);

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

    public async Task UpdateStatusOrderAsync(UpdateOrderStatusRequest orderRequest)
    {
        try
        {
            var order = await _orderGateway.GetById(orderRequest.OrderId) ??
                throw new NullReferenceException("Order not found");

            order = order.ChangeStatus((ValueObjects.Status)orderRequest.Status);
            await _orderGateway.UpdateAsync(order);
            _logger.LogInformation($"Order {order.Id} status changed to {order.Status}");

            _queue.Publish(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }
}
