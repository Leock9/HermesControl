namespace HermesControl.Api.Domain.UseCases;

public interface IOrderQueue
{
    public void Publish(Order order);
}
