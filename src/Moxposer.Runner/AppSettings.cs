using System.Text.Json;

namespace Moxposer.Runner;

public static class AppSettings
{
    public static List<string> GlobalWhitelist
    {
        get
        {
            var jsonContent = File.ReadAllText("appsettings.json");
            var config = JsonSerializer.Deserialize<WhitelistConfig>(jsonContent);
            return config.GlobalWhitelists;
        }
    }

    private class WhitelistConfig
    {
        public List<string> GlobalWhitelists { get; set; }
    }
}
