using Services.Communication.RESTful.Services;
using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SoundNest_Windows_Client.ViewModels
{
    class CreatePlaylistViewModel : ViewModel
    {
        private readonly INavigationService _navigation;
        private readonly IPlaylistService _playlistService;

        private string _selectedImagePath = "";

        public CreatePlaylistViewModel(
            INavigationService navigationService,
            IPlaylistService playlistService)
        {
            _navigation = navigationService;
            _playlistService = playlistService;

            UploadPhotoCommand = new RelayCommand(_ => UploadPlaylistPhoto());
            CreatePlaylistCommand = new RelayCommand(async _ => await ExecuteCreatePlaylistAsync());
            CancelCommand = new RelayCommand(_ => ExecuteCancelCommand());
        }

        public string PlaylistName { get; set; } = "";
        public BitmapImage? PreviewImage { get; private set; }

        public RelayCommand UploadPhotoCommand { get; }
        public RelayCommand CreatePlaylistCommand { get; }
        public RelayCommand CancelCommand { get; }

        private void UploadPlaylistPhoto()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Imagen (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
            };

            if (dlg.ShowDialog() != true)
                return;

            _selectedImagePath = dlg.FileName;
            var fi = new FileInfo(_selectedImagePath);

            const long MaxBytes = 20 * 1024 * 1024; // 20 MB servidor
            if (fi.Length > MaxBytes)
            {
                MessageBox.Show(
                    "La imagen supera los 20 MB permitidos por el servidor.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                _selectedImagePath = "";
                return;
            }

            try
            {
                // Sólo para previsualizar en la UI
                var img = new BitmapImage();
                using var fs = File.OpenRead(_selectedImagePath);
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.StreamSource = fs;
                img.EndInit();
                img.Freeze();
                PreviewImage = img;
                OnPropertyChanged(nameof(PreviewImage));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar la imagen: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                _selectedImagePath = "";
            }
        }

        private async Task ExecuteCreatePlaylistAsync()
        {
            if (string.IsNullOrWhiteSpace(PlaylistName) || PreviewImage == null || string.IsNullOrEmpty(_selectedImagePath))
            {
                MessageBox.Show(
                    "Por favor, completa todos los campos.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            byte[] imageBytes = File.ReadAllBytes(_selectedImagePath);
            string fileName = Path.GetFileName(_selectedImagePath);

            string contentType = Path.GetExtension(fileName).ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

            var result = await _playlistService.CreatePlaylistAsync(
                playlistName: PlaylistName,
                description: "",          // si tienes descripción, pásala aquí
                imageBytes: imageBytes,
                imageFileName: fileName,
                contentType: contentType);

            if (result.IsSuccess)
            {
                // Fuerza recarga completa en el sidebar
                Mediator.Notify(MediatorKeys.REFRESH_PLAYLISTS, null);

                _navigation.NavigateTo<HomeViewModel>();
            }
            else
            {
                MessageBox.Show(
                    $"Error al crear la playlist:\n{result.ErrorMessage}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ExecuteCancelCommand()
        {
            Mediator.Notify(MediatorKeys.SHOW_SEARCH_BAR, null);
            _navigation.NavigateTo<HomeViewModel>();
        }
    }
}
