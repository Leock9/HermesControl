namespace HermesControl.Api.Domain.UseCases.Requests;
public record UpdateOrderStatusRequest(Guid OrderId, int Status);
