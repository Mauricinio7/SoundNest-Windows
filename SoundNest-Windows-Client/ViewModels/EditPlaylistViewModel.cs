using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Models.Playlist;
using Services.Communication.RESTful.Services;
using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Utilities;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SoundNest_Windows_Client.ViewModels
{
    class EditPlaylistViewModel : ViewModel, IParameterReceiver
    {
        private readonly INavigationService _navigation;
        private readonly IPlaylistService _playlistService;
        private PlaylistResponse _playlist;
        private string _selectedImagePath = "";

        public string PlaylistName { get; set; } = "";
        public string Description { get; set; } = "";
        public BitmapImage? PreviewImage { get; private set; }

        public RelayCommand SavePlaylistCommand { get; }
        public RelayCommand CancelCommand { get; }

        public EditPlaylistViewModel(
            INavigationService navigationService,
            IPlaylistService playlistService)
        {
            _navigation = navigationService;
            _playlistService = playlistService;

            SavePlaylistCommand = new RelayCommand(async _ => await ExecuteSavePlaylistAsync());
            CancelCommand = new RelayCommand(_ => ExecuteCancel());
        }

        public void ReceiveParameter(object parameter)
        {
            if (parameter is PlaylistResponse pr)
            {
                _playlist = pr;
                PlaylistName = pr.PlaylistName;
                Description = pr.Description;
                OnPropertyChanged(nameof(PlaylistName));
                OnPropertyChanged(nameof(Description));

                try
                {
                    var uri = new Uri(Path.Combine(ApiRoutes.BaseUrl, pr.ImagePath.TrimStart('/')));
                    PreviewImage = new BitmapImage(uri);
                    OnPropertyChanged(nameof(PreviewImage));
                }
                catch { /* ignora si falla */ }
            }
        }


        private async Task ExecuteSavePlaylistAsync()
        {
            if (string.IsNullOrWhiteSpace(PlaylistName))
            {
                MessageBox.Show("El nombre no puede quedar vacío", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                string? base64 = null;
                if (!string.IsNullOrEmpty(_selectedImagePath))
                {
                    var bytes = File.ReadAllBytes(_selectedImagePath);
                    var ext = Path.GetExtension(_selectedImagePath).ToLower();
                    var mime = ext switch
                    {
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        _ => throw new InvalidOperationException("Tipo no soportado")
                    };
                    base64 = $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
                }

                var result = await _playlistService.EditPlaylistAsync(
                    _playlist.Id,
                    PlaylistName,
                    Description);

                if (result.IsSuccess)
                {
                    var playlistUpdated = result.Data;
                    Mediator.Notify(MediatorKeys.REFRESH_PLAYLISTS, null);
                    _navigation.NavigateTo<PlaylistDetailViewModel>(playlistUpdated);
                }
                else
                {
                    MessageBox.Show(result.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar playlist: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancel()
        {
            _navigation.NavigateTo<PlaylistDetailViewModel>(_playlist);
        }
    }
}
