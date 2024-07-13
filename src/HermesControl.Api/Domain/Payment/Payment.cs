using HermesControl.Api.Domain.Base;

namespace HermesControl.Api.Domain;

public record Payment(decimal TotalOrder)
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public decimal TotalOrder { get; init; } = TotalOrder < 0 ?
                                               throw new DomainException("Total order is required") : TotalOrder;

    public bool IsAproved { get; set; } = false;
}
