using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TransactionQL.DesktopApp.Application;

public class AdditionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return null;
        return System.Convert.ToDouble(value) + System.Convert.ToDouble(parameter);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}