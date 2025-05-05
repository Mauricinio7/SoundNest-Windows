using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SoundNest_Windows_Client.Resources.Converters
{
    class ProgressToWidthConverter : IMultiValueConverter
    {
        private const double TotalBarWidth = 483;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2) return 0;

            if (values[0] is double progress && values[1] is double max && max > 0)
                return (progress / max) * TotalBarWidth;

            return 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
