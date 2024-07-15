namespace HermesControl.Api.Infrastructure.SoulMenuGateway;

public interface ISoulMenuConfiguration
{
    string ServiceUrl { get; }
}

public record class SoulMenuConfiguration : ISoulMenuConfiguration
{
    public string ServiceUrl { get; init; } = null!;
}