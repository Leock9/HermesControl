using HermesControl.Orchestrator.Domain.ValueObjects;

namespace HermesControl.Orchestrator.Domain;

public record Order
{
    public Guid Id { get; set; }

    public Guid EventId { get; init; } = Guid.NewGuid();

    public decimal TotalOrder { get; set; }

    public Status Status { get; set; }

    public string Document { get; set; } = string.Empty;

    public IEnumerable<string> ItemMenuIds { get; init; } = new List<string>();

    public DateTime CreatedAt { get; set; } = DateTime.Now;    

    public DateTime UpdatedAt { get; init; } = DateTime.Now;

    public Payment Payment { get; set; } = null;
}