using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace SoundNest_Windows_Client.Utilities
{
    public static class ImagesHelper
    {
        public static ImageSource LoadDefaultImage(string uri)
        {
            return new System.Windows.Media.Imaging.BitmapImage(
                new Uri(uri));
        }


        public static async Task<ImageSource?> LoadImageFromUrlAsync(string url)
        {
            try
            {
                using var httpClient = new HttpClient();
                var imageData = await httpClient.GetByteArrayAsync(url);

                return await Task.Run(() =>
                {
                    using var ms = new MemoryStream(imageData);
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return (ImageSource)bitmap;
                });
            }
            catch
            {
                MessageBox.Show("Error al cargar la imagen de la canción", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
