// Version 1.0
using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace KittyOfAngels.Converters
{
    public class CountToBoolInverterConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                int count => count == 0,
                ICollection<object> collection => collection.Count == 0,
                _ => false
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }
}