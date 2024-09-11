namespace HermesControl.Consumer.Domain.UseCases.Requests;
public record UpdateOrderStatusRequest(Guid OrderId, int Status);
