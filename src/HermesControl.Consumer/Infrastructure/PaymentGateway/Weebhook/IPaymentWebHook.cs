namespace HermesControl.Consumer.Infrastructure.PaymentGateway.Weebhook;

public interface IPaymentWebHook
{
    Task<PaymentHook> PaymentHookAsync(Guid transactionId);
}
