using Services.Communication.RESTful.Services;
using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Models;
using SoundNest_Windows_Client.Utilities;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SoundNest_Windows_Client.ViewModels
{
    class CreatePlaylistViewModel : ViewModel
    {
        private readonly IAccountService _accountService;
        private readonly INavigationService _navigation;
        private readonly IPlaylistService _playlistService;

        private string _selectedImagePath = "";

        public CreatePlaylistViewModel(
            INavigationService navigationService,
            IPlaylistService playlistService,
            IAccountService accountService)
        {
            _navigation = navigationService;
            _playlistService = playlistService;
            _accountService = accountService;

            UploadPhotoCommand = new RelayCommand(_ => UploadPlaylistPhoto());
            CreatePlaylistCommand = new RelayCommand(async _ => await ExecuteCreatePlaylistAsync());
            CancelCommand = new RelayCommand(_ => ExecuteCancelCommand());
        }

        public string PlaylistName { get; set; } = "";
        public string Description { get; set; } = "";
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

            if (fi.Length > 20 * 1024 * 1024)
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
                    ex.Message,
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
                MessageBox.Show("Por favor, completa todos los campos.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var bytes = File.ReadAllBytes(_selectedImagePath);
                var extension = Path.GetExtension(_selectedImagePath).ToLower();
                var mime = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    _ => throw new InvalidOperationException("Tipo de imagen no soportado")
                };
                var base64 = $"data:{mime};base64,{Convert.ToBase64String(bytes)}";

                var result = await _playlistService.CreatePlaylistAsync(
                    PlaylistName,
                    Description,
                    base64);

                if (result.IsSuccess)
                {
                    Mediator.Notify(MediatorKeys.REFRESH_PLAYLISTS, null);
                    _navigation.NavigateTo<HomeViewModel>();
                }
                else
                {
                    MessageBox.Show(result.ErrorMessage,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar la imagen: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ExecuteCancelCommand()
        {
            Mediator.Notify(MediatorKeys.SHOW_SEARCH_BAR, null);
            _navigation.NavigateTo<HomeViewModel>();
        }
    }
}
