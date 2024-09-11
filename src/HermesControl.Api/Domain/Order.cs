using HermesControl.Api.Domain.Base;
using HermesControl.Api.Domain.ValueObjects;

namespace HermesControl.Api.Domain;

public record Order
(
    decimal TotalOrder,
    string Document,
    IEnumerable<string> ItemMenuIds,
    Payment Payment
)
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid EventId { get; init; } = Guid.NewGuid();

    public decimal TotalOrder { get; init; } = TotalOrder <= 0 ?
                                               throw new DomainException("Total order is required") : TotalOrder;

    public Status Status { get; set; } = Status.NewOrder;

    public string Document { get; init; } = Document;

    public IEnumerable<string> ItemMenuIds { get; init; } = ItemMenuIds.Count() is not 0 ? ItemMenuIds :
                                                      throw new DomainException("Item Menu is required");

    public DateTime CreatedAt { get; init; } = DateTime.Now;

    public DateTime UpdatedAt { get; init; } = DateTime.Now;

    public Payment Payment { get; init; } = Payment;
}