using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Lottery.Converters
{
    public class BooleanAndToVisibilityConverter : IMultiValueConverter
    {
        /// <summary>
        /// Convierte múltiples valores booleanos a Visibility.Visible si TODOS son true.
        /// </summary>
        /// 
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.OfType<bool>().Count() == values.Length && values.OfType<bool>().All(b => b))
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("La conversión inversa no está soportada.");
        }
    }
}