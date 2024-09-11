using HermesControl.Consumer.Domain.UseCases.Requests;

namespace HermesControl.Consumer.Domain.UseCases;

public interface IOrderUseCase
{
    public Task UpdateStatusOrderAsync(UpdateOrderStatusRequest orderRequest);
}
