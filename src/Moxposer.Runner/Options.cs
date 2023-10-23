using CommandLine;

public class Options
{
    [Option('p', "path", Required = false, HelpText = "Path to the target project directory.")]
    public string Path { get; set; } = Directory.GetCurrentDirectory();
}
