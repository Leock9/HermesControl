namespace HermesControl.Orchestrator.Domain.Gateways;

public interface IOrderGateway
{
    public void Create(Order order);
    public Task<IEnumerable<Order>> GetAll();
    public Task UpdateAsync(Order order);
    public Task<Order> GetById(Guid id);
}
