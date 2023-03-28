using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TransactionQL.DesktopApp.Views;

public class AdditionConverter : IValueConverter
{
    public static readonly AdditionConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return ((double?)value + (double?)parameter).ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (double?)value - (double?)parameter;
    }
}