using System.Text.Json;

namespace HermesControl.Api.Infrastructure.CerberusGateway;

public interface ICerberusGateway
{
    public Task<GetByDocumentResponse> GetByDocumentAsync(string document);
}

public class CerberusGateway
(
    ILogger<CerberusGateway> logger, 
    IHttpClientFactory httpClientFactory,
    ICerberusConfiguration configuration
) : ICerberusGateway
{
    private readonly ILogger<CerberusGateway> _logger = logger;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ICerberusConfiguration _configuration = configuration;

    public async Task<GetByDocumentResponse> GetByDocumentAsync(string document)
    {
        var client = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"http://localhost:8080/client/GetByDocument?Document={document}"),
        };

        _logger.LogInformation("Sending request to {RequestUri}", request.RequestUri);

        using var response = await client.SendAsync(request);
        _logger.LogInformation("Received response with status code {StatusCode}", response.StatusCode);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Response body: {ResponseBody}", body);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var result = JsonSerializer.Deserialize<GetByDocumentResponse>(body, options);

        return result!;
    }
}
