using System.Text.Json;

namespace Moxposer.Runner;

/// <summary>
/// Provides access to application settings, including a global whitelist for DLL analysis.
/// </summary>
public static class AppSettings
{
    /// <summary>
    /// Gets the global whitelist of DLL names from the appsettings.json file.
    /// This is used to determine which DLLs are whitelisted during analysis.
    /// </summary>
    public static List<string> GlobalWhitelist
    {
        get
        {
            // Read and deserialize the appsettings.json file to retrieve the global whitelist
            var jsonContent = File.ReadAllText("appsettings.json");
            var config = JsonSerializer.Deserialize<WhitelistConfig>(jsonContent);
            return config.GlobalWhitelists;
        }
    }

    /// <summary>
    /// Represents the configuration structure for the whitelist settings in the appsettings.json file.
    /// </summary>
    private class WhitelistConfig
    {
        /// <summary>
        /// Gets or sets the list of global whitelisted DLL names.
        /// </summary>
        public List<string> GlobalWhitelists { get; set; }
    }
}
