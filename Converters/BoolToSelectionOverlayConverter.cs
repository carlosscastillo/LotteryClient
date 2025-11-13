using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Lottery.Converters
{
    public class BoolToSelectionOverlayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isSelected = value is bool b && b;
            return isSelected
                ? new SolidColorBrush(Color.FromArgb(200, 0, 0, 0))
                : Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}