namespace HermesControl.Api.Infrastructure.PaymentGateway.Weebhook;

public record PaymentHook
{
    public Guid TransactionId { get; init; }
    public bool IsAproved { get; init; }
}
