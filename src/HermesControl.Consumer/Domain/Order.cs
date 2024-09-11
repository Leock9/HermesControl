using HermesControl.Consumer.Domain.Base;
using HermesControl.Consumer.Domain.ValueObjects;

namespace HermesControl.Consumer.Domain;

public record Order
(
    decimal TotalOrder,
    string Document,
    IEnumerable<string> ItemMenuIds,
    Payment Payment
)
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public decimal TotalOrder { get; init; } = TotalOrder <= 0 ?
                                               throw new DomainException("Total order is required") : TotalOrder;

    public Status Status { get; set; } = Payment.IsAproved ? Status.Preparation : Status.PaymentPending;

    public string Document { get; init; } = Document;

    public IEnumerable<string> ItemMenuIds { get; init; } = ItemMenuIds.Count() is not 0 ? ItemMenuIds :
                                                      throw new DomainException("Item Menu is required");

    public DateTime CreatedAt { get; init; } = DateTime.Now;

    public DateTime UpdatedAt { get; init; } = DateTime.Now;

    public Payment Payment { get; init; } = Payment;

    public Order ChangeStatus(Status newStatus)
    {
        if (!Payment.IsAproved)
            return this with { Status = Status.PaymentPending, UpdatedAt = DateTime.Now };

        if (newStatus == Status.Canceled)
            return this with { Status = newStatus, UpdatedAt = DateTime.Now };

        if ((int)newStatus < (int)Status)
            throw new DomainException("Status cannot be changed to a previous state");

        if (newStatus == Status.Preparation && Status != Status.NewOrder)
            throw new DomainException("Status cannot be changed to preparing");

        if (newStatus == Status.Ready && Status != Status.Preparation)
            throw new DomainException("Status cannot be changed to ready");

        if (newStatus == Status.Finished && Status != Status.Ready)
            throw new DomainException("Status cannot be changed to finished");

        return this with { Status = newStatus, UpdatedAt = DateTime.Now };
    }
}