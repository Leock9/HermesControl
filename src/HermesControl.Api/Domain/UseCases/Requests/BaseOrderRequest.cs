namespace HermesControl.Api.Domain.UseCases.Requests;

public record BaseOrderRequest
(
    decimal TotalOrder,
    string Document,
    IList<string> ItemMenuIds
);
