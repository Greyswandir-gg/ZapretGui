using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ZapretGui.Converters;

public class RunningToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var running = value is bool b && b;
        var mediaColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(
            running ? "#3BA55C" : "#ED4245");
        return new SolidColorBrush(mediaColor);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class RunningToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var running = value is bool b && b;
        return running ? "Zapret активен" : "Zapret выключен";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class SuccessToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var ok = value is bool b && b;
        var mediaColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(
            ok ? "#3BA55C" : "#ED4245");
        return new SolidColorBrush(mediaColor);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class SuccessToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var ok = value is bool b && b;
        return ok ? "OK" : "Ошибка";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
