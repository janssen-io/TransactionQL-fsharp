using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace TransactionQL.DesktopApp.Controls;

/// <summary>
/// Converts any whitespace or null string to null; otherwise the string
/// </summary>
public class EmptyToNullConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is string text && !string.IsNullOrWhiteSpace(text)
            ? text
            : null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => BindingOperations.DoNothing;
}