using Amazon.SQS.Model;
using Amazon.SQS;
using HermesControl.Api.Infrastructure.AwsSqs.Configuration;
using System.Text.Json;
using HermesControl.Api.Domain.UseCases;
using HermesControl.Api.Domain;

namespace HermesControl.Api.Infrastructure.AwsSqs;

public class AwsSqsGateway
(
    ILogger<AwsSqsGateway> logger,
    IAmazonSQS amazonSQS,
    IAwsSqsConfiguration awsSqsConfiguration
) : IOrderQueue
{
    public readonly ILogger<AwsSqsGateway> _logger = logger;
    public readonly IAmazonSQS _amazonSQS = amazonSQS;
    public readonly IAwsSqsConfiguration _awsSqsConfiguration = awsSqsConfiguration;

    public void Publish(Order order)
    {
        try
        {
            var queueUrl = _awsSqsConfiguration.QueueUrl ??
                throw new ArgumentNullException(nameof(_awsSqsConfiguration.QueueUrl));

            var request = new SendMessageRequest
                (
                    queueUrl, JsonSerializer.Serialize(order)
                );

            _amazonSQS.SendMessageAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to SQS");
            throw;
        }
    }
}
