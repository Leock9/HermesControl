using System.Text.Json;

namespace HermesControl.Api.Infrastructure.SoulMenuGateway;

public interface ISoulMenuGateway 
{
    Task<GetByIdResponse> GetByIdAsync(Guid id);
}

public class SoulMenuGateway
(
    ILogger<SoulMenuGateway> logger, 
    HttpClient httpClient, 
    ISoulMenuConfiguration configuration
) : ISoulMenuGateway
{
    ILogger<SoulMenuGateway> logger = logger;
    private readonly HttpClient _httpClient = httpClient;
    private readonly ISoulMenuConfiguration _configuration = configuration;

    public async Task<GetByIdResponse> GetByIdAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"{_configuration.ServiceUrl}{id}");

        logger.LogInformation("Received response with status code {StatusCode}", response.StatusCode);
        if (!response.IsSuccessStatusCode) throw new Exception("Error");

        var content = await response.Content.ReadAsStringAsync();
        logger.LogInformation("Response body: {ResponseBody}", content);
        var product = JsonSerializer.Deserialize<GetByIdResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
     
        return product;
    }
}
