using CommandLine;

/// <summary>
/// Represents command-line options for specifying the path to a target project directory.
/// </summary>
public class Options
{
    /// <summary>
    /// Gets or sets the path to the target project directory.
    /// If not specified, the current directory will be used as the default value.
    /// </summary>
    [Option('p', "path", Required = false, HelpText = "Path to the target project directory.")]
    public string Path { get; set; } = Directory.GetCurrentDirectory();
}
