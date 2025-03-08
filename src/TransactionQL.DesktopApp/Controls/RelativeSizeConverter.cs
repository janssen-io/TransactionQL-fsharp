using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace TransactionQL.DesktopApp.Controls
{
    public class RelativeSizeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (IsValidConversion(value, parameter, out double parentFontSize, out double factor))
            {
                return parentFontSize * factor;
            }

            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (IsValidConversion(value, parameter, out double parentFontSize, out double factor))
            {
                return parentFontSize / factor;
            }

            return value;
        }

        private static bool IsValidConversion(object? value, object? parameter, out double parentFontSize, out double factor)
        {
            factor = 0;
            parentFontSize = 0;

            if (value is double fontSize
                && parameter is string parameterString
                && double.TryParse(parameterString, out factor)
                && factor > 0)
            {
                parentFontSize = fontSize;
                return true;
            }

            return false;
        }
    }

}