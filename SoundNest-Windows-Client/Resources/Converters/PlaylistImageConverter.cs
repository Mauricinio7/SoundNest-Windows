using Services.Communication.RESTful.Constants;
using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SoundNest_Windows_Client.Resources.Converters
{
    public class PlaylistImageConverter : IValueConverter
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var filename = value as string;
            if (string.IsNullOrWhiteSpace(filename))
                return null;

            try
            {
                var baseUrl = ApiRoutes.BaseUrl.TrimEnd('/');
                var rawFile = filename.TrimStart('/');
                var url = $"{baseUrl}/{rawFile}";
                var imageData = httpClient.GetByteArrayAsync(url).GetAwaiter().GetResult();
                using var ms = new MemoryStream(imageData);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                // Devuelve null si la imagen no pudo cargarse
                MessageBox.Show("Error al cargar la imagen de la playlist", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
