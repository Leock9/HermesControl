namespace HermesControl.Api.Infrastructure.AwsSqs.Configuration;

public interface IAwsSqsConfiguration
{
    string Base { get; }
    string Region { get; }
}

public record class AwsSqsConfiguration : IAwsSqsConfiguration
{
    public string Base { get; init; } = null!;

    public string Region { get; init; } = null!;
}
