using Services.Communication.RESTful.Constants;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SoundNest_Windows_Client.Resources.Converters
{
    public class PlaylistImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var filename = value as string;
            if (string.IsNullOrWhiteSpace(filename))
                return null;
            var baseUrl = ApiRoutes.BaseUrl.TrimEnd('/');
            var rawFile = filename.TrimStart('/');
            var uri = new Uri($"{baseUrl}/{rawFile}");
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = uri;
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            return bmp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
