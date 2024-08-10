using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace TransactionQL.DesktopApp.Controls;

public class EqualityConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count <= 1)
            return true;

        for(int i = 1; i < values.Count; i++)
        {
            if (values[i] != values[i - 1])
                return false;
        }

        return true;
    }
}
