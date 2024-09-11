namespace HermesControl.Orchestrator.Domain.ValueObjects;

public enum Status
{
    NewOrder = 0,
    SimulateOrder = 1,
    PaymentPending = 2,
    Preparation = 3,
    Ready = 4,
    Finished = 5,
    Canceled = 6
}
