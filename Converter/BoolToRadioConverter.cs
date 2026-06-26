using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace D8_Demo.Converter;

public class BoolToRadioConverter : IValueConverter
{
    public object Convert(object? value, Type targetType,
        object? parameter, CultureInfo culture)
    {
        bool flag = (bool)value!;
        bool target = bool.Parse(parameter!.ToString()!);

        return flag == target;
    }

    public object ConvertBack(object? value, Type targetType,
        object? parameter, CultureInfo culture)
    {
        if ((bool)value!)
            return bool.Parse(parameter!.ToString()!);

        return Avalonia.Data.BindingOperations.DoNothing;
    }
}