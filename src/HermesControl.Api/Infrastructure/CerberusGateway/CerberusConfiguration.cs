namespace HermesControl.Api.Infrastructure.CerberusGateway;

public interface ICerberusConfiguration
{
    string ServiceUrl { get; }
}

public record class CerberusConfiguration : ICerberusConfiguration
{
    public string ServiceUrl { get; init; } = null!;
}
