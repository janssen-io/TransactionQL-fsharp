using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace TransactionQL.DesktopApp.Controls;

public class HasValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string text)
            return !string.IsNullOrEmpty(text);

        return value != null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
