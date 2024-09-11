namespace HermesControl.Api.Infrastructure.RabbitMq;

public interface IRabbitMqSettings
{
    string HostName { get; init; }
    string UserName { get; init; }
    string Password { get; init; }
    int Port { get; init; }
}

public record RabbitMqSettings : IRabbitMqSettings
{
    public string HostName { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public int Port { get; init; } = 0;
}
