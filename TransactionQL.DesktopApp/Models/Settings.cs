using System;
using System.IO;
using System.Text.Json;
using TransactionQL.Application;

namespace TransactionQL.DesktopApp.Models
{
    public class Settings
    {
        public const string ConfigFileName = "desktop.tqlconfig.json";

        private Uri? _origin;

        public bool ShouldSaveProgressOnExit { get; set; }

        public required string DefaultCurrency { get; set; }
        public required string DefaultCheckingAccount { get; set; }
        public required string Locale { get; set; }

        public required string TqlBinaryLocation { get; set; }
        public required string PluginDirectory { get; set; }

        // Use getter so that the default is not changed when properties are mutated
        public static Settings Default => new()
        {
            ShouldSaveProgressOnExit = true,
            DefaultCurrency = "€",
            DefaultCheckingAccount = "Assets:Checking",
            Locale = "en-US",
            TqlBinaryLocation = Configuration.createAndGetAppDir,
            PluginDirectory = Configuration.createAndGetPluginDir,
        };

        // TODO: also read from directory where accounts file exists
        public static Settings FromConfig(Uri filePath)
        {
            using var reader = new FileStream(filePath.AbsolutePath, FileMode.OpenOrCreate, FileAccess.Read);
            var settings = FromConfig(reader);
            settings._origin = filePath;

            return settings;
        }

        public static Settings FromConfig(Stream configFile)
        {
            Settings? settings;
            try
            {
                settings = JsonSerializer.Deserialize<Settings>(configFile);
            }
            catch
            {
                // TODO: show error / warning?
                settings = null;
            }

            if (settings == null)
                return Default;

            return settings;
        }

        public void Persist(Uri fallbackPath)
        {
            using var reader = new FileStream((_origin ?? fallbackPath).AbsolutePath, FileMode.Create, FileAccess.ReadWrite);
            Persist(reader);
        }

        public void Persist(Stream configFile)
        {
            JsonSerializer.Serialize(configFile, this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
