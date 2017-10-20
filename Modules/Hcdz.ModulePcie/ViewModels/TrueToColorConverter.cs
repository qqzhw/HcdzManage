using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Hcdz.ModulePcie.ViewModels
{
    public class TrueToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value==null)
            {
                return new SolidColorBrush(Colors.OrangeRed);
            }
            bool flag = false;
            bool.TryParse(value.ToString(), out flag);
            if (flag)
            {
                return new SolidColorBrush(Color.FromRgb(50,205,50));
            }
            return new SolidColorBrush(Colors.OrangeRed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
