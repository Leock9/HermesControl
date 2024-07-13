using FastEndpoints;
using FluentValidation;

namespace Endpoints.Order.UpdateStatus;
public class Request
{
    public Guid OrderId { get; init; }
    public int? Status { get; init; } = null;
}

public class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(x => x.OrderId).NotEmpty().NotNull();
        RuleFor(x => x.Status)
                            .NotEmpty()
                            .NotNull()
                            .GreaterThanOrEqualTo(0)
                            .LessThanOrEqualTo(5);
    }
}

public class Response
{
    public string Message { get; init; } = string.Empty;
}
