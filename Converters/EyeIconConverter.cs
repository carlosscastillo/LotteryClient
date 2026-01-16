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
        private const string EYE_OPEN_ICON = "pack://application:,,,/Images/Icons/eye_open.png";
        private const string EYE_CLOSED_ICON = "pack://application:,,,/Images/Icons/eye_closed.png";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isVisible)
            {     
                return isVisible ? EYE_OPEN_ICON : EYE_CLOSED_ICON;
            }
            return EYE_CLOSED_ICON;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {            
            throw new NotImplementedException();
        }
    }
}
