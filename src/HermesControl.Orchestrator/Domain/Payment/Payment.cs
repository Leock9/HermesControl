namespace HermesControl.Orchestrator.Domain;

public record Payment(decimal TotalOrder)
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public decimal TotalOrder { get; init; } = TotalOrder;

    public bool IsAproved { get; set; } = false;
}
