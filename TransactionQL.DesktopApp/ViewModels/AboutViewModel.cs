using TransactionQL.DesktopApp.Models;

namespace TransactionQL.DesktopApp.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        public required string Version { get; init; }
        public string? TqlBinaryLocation { get; set; }
        public string? PluginDirectory { get; set; }

        public static AboutViewModel From(About about)
        {
            return new()
            {
                Version = about.Version,
                TqlBinaryLocation = about.TqlBinaryLocation,
                PluginDirectory = about.PluginDirectory,
            };
        }
    }
}
