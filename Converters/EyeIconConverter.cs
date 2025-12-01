using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Lottery.Converters
{
    public class EyeIconConverter : IValueConverter
    {
        private const string EyeOpenIcon = "pack://application:,,,/Images/Icons/eye_open.png";
        private const string EyeClosedIcon = "pack://application:,,,/Images/Icons/eye_closed.png";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isVisible)
            {     
                return isVisible ? EyeOpenIcon : EyeClosedIcon;
            }
            return EyeClosedIcon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {            
            throw new NotImplementedException();
        }
    }
}
