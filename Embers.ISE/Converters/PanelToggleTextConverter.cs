using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Embers.ISE.Converters;

public sealed class PanelToggleTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isVisible)
            return isVisible ? "❮" : "❯";

        return "❯";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value is "❮";
}
