namespace HermesControl.Orchestrator.Domain.UseCases;

public interface IOrderQueue
{
    public void Publish(Order order);
    Task<Order> ConsumeAsync(string status);
}
