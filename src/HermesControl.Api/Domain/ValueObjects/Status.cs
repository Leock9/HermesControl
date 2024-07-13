namespace HermesControl.Api.Domain.ValueObjects;

public enum Status
{
    PaymentPending = 0,
    Received = 1,
    Preparation = 2,
    Ready = 3,
    Finished = 4,
    Canceled = 5
}
