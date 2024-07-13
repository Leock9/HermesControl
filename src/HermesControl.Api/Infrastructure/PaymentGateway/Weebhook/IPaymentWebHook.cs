﻿namespace HermesControl.Api.Infrastructure.PaymentGateway.Weebhook;

public interface IPaymentWebHook
{
    Task<PaymentHook> PaymentHookAsync(Guid transactionId);
}