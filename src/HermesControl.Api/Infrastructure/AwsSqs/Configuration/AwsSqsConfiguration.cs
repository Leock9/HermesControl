namespace HermesControl.Api.Infrastructure.AwsSqs.Configuration;

public interface IAwsSqsConfiguration
{
    string QueueUrl { get; }
    string Region { get; }
}

public record class AwsSqsConfiguration : IAwsSqsConfiguration
{
    public string QueueUrl { get; init; } = null!;

    public string Region { get; init; } = null!;
}
