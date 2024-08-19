using System.Reflection;
using TransactionQL.Application;

namespace TransactionQL.DesktopApp.Models;

public class About
{
    public string TqlBinaryLocation { get; } = Configuration.createAndGetAppDir;
    public string PluginDirectory { get; } = Configuration.createAndGetPluginDir;
    public string Version { get; } =
        Assembly.GetExecutingAssembly().GetName().Version?.ToString()
        ?? "<Unable to get version info>";

    public static readonly About Default = new();
}
