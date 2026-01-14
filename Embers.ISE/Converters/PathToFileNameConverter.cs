using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;

namespace Embers.ISE.Converters;

public sealed class PathToFileNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var path = value as string;
        if (string.IsNullOrWhiteSpace(path)) return string.Empty;
        return Path.GetFileName(path);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value?.ToString() ?? string.Empty;
}
