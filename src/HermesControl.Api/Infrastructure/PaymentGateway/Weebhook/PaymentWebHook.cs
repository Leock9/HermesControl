namespace HermesControl.Api.Infrastructure.PaymentGateway.Weebhook;

public class PaymentWebHook : IPaymentWebHook
{
    public Task<PaymentHook> PaymentHookAsync(Guid transactionId)
    {
        return Task.FromResult(new PaymentHook
        {
            TransactionId = transactionId,
            IsAproved = new Random().Next(2) == 0
        });
    }
}