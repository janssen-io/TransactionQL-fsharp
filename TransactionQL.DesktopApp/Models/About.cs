using System.Reflection;
using TransactionQL.Application;

namespace TransactionQL.DesktopApp.Models;

public class About
{
    public string TqlBinaryLocation { get; } = Configuration.createAndGetAppDir;
    public string PluginDirectory { get; } = Configuration.createAndGetPluginDir;
    public string Version { get; } =
        Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version
        ?? "<Unable to get version info>";

    public static readonly About Default = new();
}
