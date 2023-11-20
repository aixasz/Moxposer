namespace Moxposer.Experiments.Services;

public interface IExperimentHttpClientFactory
{
    Task SendAsync();
}

public class ExperimentHttpClientFactory(IHttpClientFactory httpClientFactory) : IExperimentHttpClientFactory
{

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task SendAsync()
    {
        using var client = _httpClientFactory.CreateClient("ExperimentClient");

        await client.SendAsync(new HttpRequestMessage
        {
            Content = new StringContent(""),
            Method = HttpMethod.Post,
        });
    }
}
