using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace TransactionQL.DesktopApp.Controls;

/// <summary>
/// Converts any nullable object to a boolean.
/// <list type="bullet">
/// <item>True, if the item is not null and not empty (string);</item>
/// <item>False, otherwise.</item>
/// </list>
/// </summary>
public class HasValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is string text
            ? !string.IsNullOrEmpty(text)
            : value != null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => BindingOperations.DoNothing;
}