using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using TransactionQL.Application;
using TransactionQL.DesktopApp.Models;

namespace TransactionQL.DesktopApp.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public required string Version { get; init; } 

        public bool ShouldSaveProgressOnExit { get; set; }

        public string? DefaultCurrency  { get; set; } 
        public string? DefaultCheckingAccount{ get; set; }
        public string? Locale { get; set; }

        public string? TqlBinaryLocation { get; set; }
        public string? PluginDirectory { get; set; }

        public static SettingsViewModel From(Settings settings)
        {
            return new()
            {
                Version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version
                    ?? "<error>",
                ShouldSaveProgressOnExit = settings.ShouldSaveProgressOnExit,
                DefaultCurrency = settings.DefaultCurrency,
                DefaultCheckingAccount = settings.DefaultCheckingAccount,
                Locale = settings.Locale,
                TqlBinaryLocation = settings.TqlBinaryLocation,
                PluginDirectory = settings.PluginDirectory,
            };
        }

        public void Persist()
        {
            var uri = Configuration.createAndGetAppDir;
            var path = Path.Join(uri, Settings.ConfigFileName);
            var def = Settings.Default;

            new Settings
            {
                ShouldSaveProgressOnExit = ShouldSaveProgressOnExit,
                DefaultCurrency = DefaultCurrency ?? def.DefaultCurrency,
                DefaultCheckingAccount = DefaultCheckingAccount ?? def.DefaultCheckingAccount,
                Locale = Locale ?? def.Locale,
                TqlBinaryLocation = TqlBinaryLocation ?? def.TqlBinaryLocation,
                PluginDirectory = PluginDirectory ?? def.PluginDirectory,
            }.Persist(new Uri(path));
        }
    }
}
