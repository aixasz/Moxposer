using Moxposer.Runner;
using Moxposer.Runner.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static ServiceProvider GetServiceProvider()
    {
        var serviceProvider = new ServiceCollection()
           .AddSingleton<IDecompilerService, DecompilerService>()
           .AddSingleton<IDllScanner, DllScanner>()
           .AddTransient<IDllAnalyzer, DllAnalyzer>()
           .BuildServiceProvider();

        return serviceProvider;
    }
}
