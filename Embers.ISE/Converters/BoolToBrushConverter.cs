using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Embers.ISE.Converters;

public sealed class BoolToBrushConverter : IValueConverter
{
    public IBrush TrueBrush { get; set; } = Brushes.Transparent;
    public IBrush FalseBrush { get; set; } = Brushes.Transparent;
    public bool ReturnUnsetWhenTrue { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool flag && flag)
            return ReturnUnsetWhenTrue ? Avalonia.AvaloniaProperty.UnsetValue : TrueBrush;

        return FalseBrush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Avalonia.AvaloniaProperty.UnsetValue;
    }
}
