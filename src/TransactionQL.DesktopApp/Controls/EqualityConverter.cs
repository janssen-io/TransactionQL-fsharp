using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace TransactionQL.DesktopApp.Controls;

/// <summary>
/// Converts a list of items to a boolean.
/// <list type="bullet">
/// <item>True, if the collection is empty;</item>
/// <item>True, all items are equal (==);</item>
/// <item>False, otherwise.</item>
/// </list>
/// </summary>
public class EqualityConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count <= 1)
        {
            return true;
        }

        for (int i = 1; i < values.Count; i++)
        {
            if (values[i] != values[i - 1])
            {
                return false;
            }
        }

        return true;
    }
}