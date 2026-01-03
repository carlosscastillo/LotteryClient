using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Lottery.Converters
{
    public class TokenToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string tokenKey)
            {
                try
                {
                    string fileName = "";

                    switch (tokenKey)
                    {
                        case "beans": 
                            fileName = "token00.png"; 
                            break;

                        case "bottle_caps": 
                            fileName = "token01.png"; 
                            break;

                        case "pou": 
                            fileName = "token02.png"; 
                            break;

                        case "corn": 
                            fileName = "token03.png"; 
                            break;

                        case "coins": 
                            fileName = "token04.png"; 
                            break;

                        default: 
                            fileName = "token00.png"; 
                            break;
                    }

                    string path = $"pack://application:,,,/Images/Tokens/{fileName}";
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