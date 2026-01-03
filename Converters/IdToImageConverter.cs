using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Lottery.Converters
{
    public class IdToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int cardId)
            {
                try
                {
                    string fileName = $"card{cardId:00}.png";

                    string path = $"pack://application:,,,/Images/Cards/{fileName}";

                    return new BitmapImage(new Uri(path));
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}