using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Embers.ISE.Converters;

public sealed class EmptyToNullConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var text = value as string;
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value?.ToString() ?? string.Empty;
}
