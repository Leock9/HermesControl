using FastEndpoints;
using FluentValidation;
using HermesControl.Api.Domain.UseCases.Requests;

namespace Endpoints.Order.Checkout;

public sealed class Request
{
    public BaseOrderRequest BaseOrderRequest { get; set; } = null!;
}

public sealed class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(x => x.BaseOrderRequest.ItemMenuIds).NotEmpty().NotNull();
        RuleFor(x => x.BaseOrderRequest.Document).NotEmpty().NotNull();
        RuleFor(x => x.BaseOrderRequest.TotalOrder).NotEmpty().NotNull();
    }
}

public sealed class Response
{
    public Guid OrderId { get; init; }
}