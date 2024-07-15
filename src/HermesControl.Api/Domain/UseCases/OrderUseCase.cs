using HermesControl.Api.Domain.Gateways;
using HermesControl.Api.Domain.UseCases.Requests;
using HermesControl.Api.Infrastructure.CerberusGateway;
using HermesControl.Api.Infrastructure.SoulMenuGateway;

namespace HermesControl.Api.Domain.UseCases;

public class OrderUseCase
(
    ILogger<OrderUseCase> logger,
    IOrderGateway orderGateway,
    IPaymentGateway paymentService,
    IOrderQueue queue,
    ICerberusGateway cerberusGateway,
    ISoulMenuGateway soulMenuGateway
) : IOrderUseCase
{
    private readonly ILogger<OrderUseCase> _logger = logger;
    private readonly IOrderGateway _orderGateway = orderGateway;
    private readonly IPaymentGateway _paymentService = paymentService;
    private readonly IOrderQueue _queue = queue;
    private readonly ICerberusGateway _cerberusGateway = cerberusGateway;
    private readonly ISoulMenuGateway _soulMenuGateway = soulMenuGateway;

    public Guid CreateOrder(BaseOrderRequest orderRequest)
    {
        try
        {
            var client = _cerberusGateway.GetByDocumentAsync(orderRequest.Document).Result ?? 
                                                           throw new NullReferenceException("Client not found");

            foreach (var itemId in orderRequest.ItemMenuIds)
            {
                var product = _soulMenuGateway.GetByIdAsync(new Guid(itemId)).Result ??
                    throw new NullReferenceException("Product not found");

                if (!product.IsActive)
                    throw new Exception("Product not active");
            }

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
