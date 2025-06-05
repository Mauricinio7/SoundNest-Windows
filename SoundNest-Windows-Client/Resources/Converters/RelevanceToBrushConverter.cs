using Services.Communication.RESTful.Models.Notification;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace SoundNest_Windows_Client.Resources.Converters
{
    public class RelevanceToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Relevance relevance)
            {
                return relevance switch
                {
                    Relevance.low => new SolidColorBrush(Color.FromRgb(60, 60, 60)),     
                    Relevance.medium => new SolidColorBrush(Color.FromRgb(70, 100, 160)),
                    Relevance.high => new SolidColorBrush(Color.FromRgb(180, 50, 50)),   
                    _ => Brushes.DarkGray
                };
            }

            return Brushes.DarkGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
