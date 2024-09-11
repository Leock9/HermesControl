using HermesControl.Api.Domain.UseCases.Requests;

namespace HermesControl.Api.Domain.UseCases;

public interface IOrderUseCase
{
    public Guid CreateOrder(BaseOrderRequest orderRequest);
    public Task<IEnumerable<Order>> GetAll();
}
