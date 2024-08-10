using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.IO;

namespace TransactionQL.DesktopApp.Controls
{
    public class FilePathConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return string.Empty;
            }

            if (value is string path && targetType == typeof(string))
            {
                return Path.GetFileName(path);
            }

            if (value is Uri uri && targetType == typeof(string))
            {
                return Path.GetFileName(uri.AbsolutePath);
            }

            throw new InvalidCastException($"{value} ({value.GetType()}) is not a file path.");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}
