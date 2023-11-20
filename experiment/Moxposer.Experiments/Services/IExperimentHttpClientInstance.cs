namespace Moxposer.Experiments.Services;

public interface IExperimentHttpClientInstance
{
    Task SendAsync();
}

public class ExperimentHttpClientInstance : IExperimentHttpClientInstance
{
    private static readonly HttpClient _httpClient = new();

    public async Task SendAsync()
    {
        _httpClient.BaseAddress = new Uri("www.experiment.net");
        await _httpClient.SendAsync(new HttpRequestMessage
        {
            Content = new StringContent(""),
            Method = HttpMethod.Post,
        });
    }
}
