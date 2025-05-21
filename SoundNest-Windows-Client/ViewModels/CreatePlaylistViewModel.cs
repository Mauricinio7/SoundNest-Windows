using Services.Infrestructure;
using Services.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using SoundNest_Windows_Client.Utilities;
using Services.Communication.RESTful.Services;

namespace SoundNest_Windows_Client.ViewModels
{
    class CreatePlaylistViewModel : Services.Navigation.ViewModel
    {
        private INavigationService navigation;
        public INavigationService Navigation
        {
            get => navigation;
            set
            {
                navigation = value;
                OnPropertyChanged();
            }
        }

        private readonly IPlaylistService _playlistService;

        private BitmapImage? previewImage;
        public BitmapImage? PreviewImage
        {
            get => previewImage;
            set
            {
                previewImage = value;
                OnPropertyChanged();
            }
        }

        private string playlistName;
        public string PlaylistName
        {
            get => playlistName;
            set { playlistName = value; OnPropertyChanged(); }
        }

        public RelayCommand CreatePlaylistCommand { get; set; }
        public RelayCommand UploadPhotoCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }

        public CreatePlaylistViewModel(INavigationService navigationService, IPlaylistService playlistService)
        {
            Navigation = navigationService;
            _playlistService = playlistService;

            UploadPhotoCommand = new RelayCommand(UploadPlaylistPhoto);
            CreatePlaylistCommand = new RelayCommand(async _ => await ExecuteCreatePlaylistAsync());
            CancelCommand = new RelayCommand(ExecuteCancelCommand);


        }

        private async Task ExecuteCreatePlaylistAsync()
        {
            if (string.IsNullOrWhiteSpace(PlaylistName) || PreviewImage == null)
            {
                MessageBox.Show("Por favor, completa todos los campos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            byte[] imageBytes;
            string fileName = "cover.png";
            string contentType = "image/png";

            using (var ms = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(PreviewImage));
                encoder.Save(ms);
                imageBytes = ms.ToArray();
            }

            var response = await _playlistService.CreatePlaylistAsync(PlaylistName,"",imageBytes,fileName,contentType);

            if (response.IsSuccess)
            {
                Mediator.Notify(MediatorKeys.ADD_PLAYLIST, new Playlist
                {
                    //Image = response.NewId,
                    //Name = PlaylistName
                });

                Mediator.Notify(MediatorKeys.REFRESH_PLAYLISTS, null);

                Navigation.NavigateTo<HomeViewModel>();
            }
            else
            {
                MessageBox.Show($"Error: {response.ErrorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancelCommand(object parameter)
        {
            Mediator.Notify(MediatorKeys.SHOW_SEARCH_BAR, null);
            Navigation.NavigateTo<HomeViewModel>();
        }


        public void UploadPlaylistPhoto()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png"
            };

            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;
                FileInfo fileInfo = new FileInfo(filePath);
                const long MaxSizeBytes = 30 * 1024 * 1024;

                if (fileInfo.Length > MaxSizeBytes)
                {
                    MessageBox.Show("La imagen supera los 30MB permitidos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                try
                {
                    byte[] imageBytes = File.ReadAllBytes(filePath);

                    using var stream = new MemoryStream(imageBytes);
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();

                    PreviewImage = image;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar la imagen: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}
