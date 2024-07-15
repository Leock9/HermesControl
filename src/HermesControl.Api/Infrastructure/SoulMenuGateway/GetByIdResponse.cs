namespace HermesControl.Api.Infrastructure.SoulMenuGateway;

public record GetByIdResponse
(
    Guid Id,
    bool IsActive
);
