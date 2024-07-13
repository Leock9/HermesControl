using FastEndpoints;
using HermesControl.Api.Domain.UseCases.Requests;

namespace Endpoints.Order.UpdateStatus;

public class Mapper : Mapper<Request, Response, object>
{
    public UpdateOrderStatusRequest ToRequest(Request r) => new
        (
            r.OrderId,
            r.Status!.Value
        );
}