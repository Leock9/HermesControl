using FastEndpoints;

namespace Endpoints.Order.GetList;

public class Request
{
}

public class Validator : Validator<Request>
{
    public Validator()
    {
    }
}
public class Response
{
    public IEnumerable<HermesControl.Api.Domain.Order> Orders { get; init; } = new List<HermesControl.Api.Domain.Order>();
}
