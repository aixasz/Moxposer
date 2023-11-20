using Moxposer.Experiments.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("ExperimentClient", client =>
{
    client.BaseAddress = new Uri("www.experiment.net");
});

builder.Services.AddTransient<IExperimentHttpClientInstance, ExperimentHttpClientInstance>();
builder.Services.AddTransient<IExperimentHttpClientFactory, ExperimentHttpClientFactory>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/Send", () =>
{

});

app.Run();
