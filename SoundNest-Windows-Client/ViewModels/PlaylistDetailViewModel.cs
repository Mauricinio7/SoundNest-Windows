using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Models;
using Services.Communication.RESTful.Models.Playlist;
using Services.Communication.RESTful.Models.Songs;
using Services.Communication.RESTful.Services;
using Services.Infrestructure;
using Services.Navigation;
using Song;
using SoundNest_Windows_Client.Models;
using SoundNest_Windows_Client.Notifications;
using SoundNest_Windows_Client.Resources.Controls;
using SoundNest_Windows_Client.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SoundNest_Windows_Client.ViewModels
{
    class PlaylistDetailViewModel : Services.Navigation.ViewModel, IParameterReceiver
    {
        private readonly INavigationService _navigation;
        private readonly IPlaylistService _playlistService;

        private Playlist _currentPlaylist;

        private Models.Song selectedSong;
        public Models.Song SelectedSong
        {
            get => selectedSong;
            set { selectedSong = value; OnPropertyChanged(); }
        }


        private bool isDeletePopupVisible;
        public bool IsDeletePopupVisible
        {
            get => isDeletePopupVisible;
            set { isDeletePopupVisible = value; OnPropertyChanged(); }
        }

        private string _playlistName = string.Empty;
        public string PlaylistName
        {
            get => _playlistName;
            set { _playlistName = value; OnPropertyChanged(); }
        }

        private bool _isPlaylistEmpty;
        public bool IsPlaylistEmpty
        {
            get => _isPlaylistEmpty;
            set { _isPlaylistEmpty = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Models.Song> Songs { get; set; } = new();

        public RelayCommand BackCommand { get; }
        public RelayCommand PlaySongCommand { get; }
        public RelayCommand EditPlaylistCommand { get; }
        public RelayCommand DeletePlaylistCommand { get; }
        public RelayCommand PlayPlaylistCommand { get; }

        public ICommand ShowDeletePopupCommand { get; }
        public ICommand DeleteSongCommand { get; }


        public PlaylistDetailViewModel(INavigationService navigation, IPlaylistService playlistService)
        {
            _navigation = navigation;
            _playlistService = playlistService;

            Songs = new ObservableCollection<Models.Song>();

            BackCommand = new RelayCommand(_ => _navigation.NavigateTo<HomeViewModel>());
            PlaySongCommand = new RelayCommand(ExecutePlaySong);
            EditPlaylistCommand = new RelayCommand(_ => ExecuteEditPlaylist());
            DeletePlaylistCommand = new RelayCommand(async _ => await ExecuteDeletePlaylistAsync());
            PlayPlaylistCommand = new RelayCommand(ExecutePlayPlaylistCommand);

            ShowDeletePopupCommand = new RelayCommand(param =>
            {
                if (param is Models.Song song)
                {
                    SelectedSong = song;
                    IsDeletePopupVisible = true;
                }
            });

            DeleteSongCommand = new RelayCommand(async param =>
            {
                if (param is Models.Song songToRemove)
                {
                    await ExecuteRemoveSongAsync(songToRemove);
                    IsDeletePopupVisible = false;
                }
            });
        }
        public void ReceiveParameter(object parameter)
        {
            if (parameter is Playlist playlist)
            {
                _currentPlaylist = playlist;
                _ = LoadPlaylist(playlist);
            }
            else
            {
                ToastHelper.ShowToast("Hubo un error al cargar la playlist seleccionada", NotificationType.Error, "Error");
            }
        }

        private async Task LoadPlaylist(Playlist playlist)
        {
            PlaylistName = playlist.PlaylistName;
            Songs.Clear();
            int index = 1;

            if (playlist.Songs == null || playlist.Songs.Count == 0)
            {
                IsPlaylistEmpty = true;
            }

            foreach (var song in playlist.Songs)
            {
                var parsedSong = new Models.Song
                {
                    IdSong = song.IdSong,
                    IdSongExtension = song.IdSongExtension,
                    IdSongGenre = song.IdSongGenre,
                    IsDeleted = song.IsDeleted,
                    PathImageUrl = song.PathImageUrl,
                    ReleaseDate = song.ReleaseDate,
                    SongName = song.SongName,
                    UserName = song.UserName,
                    FileName = song.FileName,
                    DurationSeconds = song.DurationSeconds,
                    Description = song.Description,
                    DurationFormatted = TimeSpan.FromSeconds(song.DurationSeconds).ToString(@"m\:ss"),
                    Index = index++,
                    Image = ImagesHelper.LoadDefaultImage("pack://application:,,,/Resources/Images/Icons/Default_Song_Icon.png")
                };

                Songs.Add(parsedSong);

                if (!string.IsNullOrEmpty(song.PathImageUrl) && song.PathImageUrl.Length > 1)
                {
                    var imageUrl = $"{ApiRoutes.BaseUrl}{song.PathImageUrl[1..]}";
                    _ = Task.Run(async () =>
                    {
                        var image = await ImagesHelper.LoadImageFromUrlAsync(imageUrl);
                        if (image != null)
                        {
                            Application.Current.Dispatcher.Invoke(() => parsedSong.Image = image);
                        }
                    });
                }
            }
            IsPlaylistEmpty = Songs.Count == 0;
        }

        private void ExecutePlayPlaylistCommand()
        {
            if (IsPlaylistEmpty)
            {
                ToastHelper.ShowToast("No hay canciones para reproducir", NotificationType.Warning, "Playlist vacía");
                return;
            }
            Mediator.Notify(MediatorKeys.HIDE_MUSIC_PLAYER, null);
            Mediator.Notify(MediatorKeys.SHOW_MUSIC_PLAYER, Songs.ToList());
        }

        private void ExecutePlaySong(object parameter)
        {
            if (parameter is Models.Song song)
            {
                Mediator.Notify(MediatorKeys.HIDE_MUSIC_PLAYER, null);

                Models.SongOfPlaylist songOfPlaylist = new SongOfPlaylist();
                songOfPlaylist.Playlist = Songs.ToList();
                songOfPlaylist.Index = Songs.IndexOf(song);

                Mediator.Notify(MediatorKeys.SHOW_MUSIC_PLAYER, songOfPlaylist);
            }
        }

        private void ExecuteEditPlaylist()
        {
            _navigation.NavigateTo<EditPlaylistViewModel>(_currentPlaylist);
        }

        private async Task ExecuteDeletePlaylistAsync()
        {
            if (_currentPlaylist == null)
                return;

            bool confirm = DialogHelper.ShowConfirmation("Eliminar Playlist", $"¿Seguro que quieres eliminar la playlist: “{_currentPlaylist.PlaylistName}”?");

            if (!confirm)
                return;

            var result = await ExecuteRESTfulApiCall(() => _playlistService.DeletePlaylistAsync(_currentPlaylist.Id));

            if (result.IsSuccess)
            {
                ToastHelper.ShowToast("Playlist eliminada correctamente", NotificationType.Success, "Éxito");
                _navigation.NavigateTo<HomeViewModel>();
            }
            else
            {
                ShowPlaylistDeleteError(result);
            }
        }

        private void ShowPlaylistDeleteError(ApiResult<bool> result)
        {
            string title = "Error al eliminar playlist";

            string message = result.StatusCode switch
            {
                HttpStatusCode.BadRequest => "No se pudo procesar la solicitud para eliminar la playlist.",
                HttpStatusCode.Unauthorized => "Tu sesión ha expirado. Inicia sesión nuevamente.",
                HttpStatusCode.Forbidden => "Tu sesión ha expirado. Inicia sesión nuevamente.",
                HttpStatusCode.NotFound => "La playlist que intentas eliminar no fue encontrada o ya fue eliminada.",
                HttpStatusCode.InternalServerError => "Ocurrió un error inesperado al eliminar la playlist. Inténtalo más tarde.",
                _ => "Parece que no hay conexión a internet. Inténtalo más tarde."
            };

            DialogHelper.ShowAcceptDialog(title, message, AcceptDialogType.Warning);
        }



        private async Task ExecuteRemoveSongAsync(Models.Song song)
        {
            if (_currentPlaylist == null || song == null)
                return;

            var result = await ExecuteRESTfulApiCall(() =>
                _playlistService.RemoveSongFromPlaylistAsync(song.IdSong.ToString(), _currentPlaylist.Id)
            );

            if (result.IsSuccess)
            {
                ToastHelper.ShowToast("Canción eliminada correctamente de la playlist", NotificationType.Success, "Éxito");
                Mediator.Notify(MediatorKeys.REFRESH_PLAYLISTS, null);
                Songs.Remove(song);
            }
            else
            {
                ShowRemoveSongFromPlaylistError(result);
            }
        }

        private void ShowRemoveSongFromPlaylistError(ApiResult<bool> result)
        {
            string title = "Error al eliminar canción";

            string message = result.StatusCode switch
            {
                HttpStatusCode.BadRequest => "La canción no pudo eliminarse debido a una solicitud inválida.",
                HttpStatusCode.Unauthorized => "Tu sesión ha expirado. Inicia sesión nuevamente.",
                HttpStatusCode.Forbidden => "No tienes permiso para modificar esta playlist.",
                HttpStatusCode.NotFound => "La playlist no fue encontrada.",
                HttpStatusCode.Conflict => "La canción no se encuentra en esta playlist.",
                HttpStatusCode.InternalServerError => "Ocurrió un error inesperado al eliminar la canción. Inténtalo más tarde.",
                _ => "No se pudo conectar con el servidor. Revisa tu conexión a internet."
            };

            DialogHelper.ShowAcceptDialog(title, message, AcceptDialogType.Warning);
        }


    }
}
