using Services.Communication.RESTful.Constants;
using Services.Communication.RESTful.Models.Playlist;
using Services.Communication.RESTful.Models.Songs;
using Services.Communication.RESTful.Services;
using Services.Infrestructure;
using Services.Navigation;
using Song;
using SoundNest_Windows_Client.Models;
using SoundNest_Windows_Client.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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
                MessageBox.Show("Error al cargar la playlist seleccionada.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadPlaylist(Playlist playlist)
        {
            PlaylistName = playlist.PlaylistName;
            Songs.Clear();
            int index = 1;

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
                    Visualizations = song.Visualizations,
                    DurationFormatted = TimeSpan.FromSeconds(song.DurationSeconds).ToString(@"m\:ss"),
                    Index = index++
                };


                if (!string.IsNullOrEmpty(song.PathImageUrl) && song.PathImageUrl.Length > 1)
                {
                    parsedSong.Image = await ImagesHelper.LoadImageFromUrlAsync($"{ApiRoutes.BaseUrl}{song.PathImageUrl[1..]}");
                }
                else
                {
                    parsedSong.Image = ImagesHelper.LoadDefaultImage("pack://application:,,,/Resources/Images/Icons/Default_Song_Icon.png");
                }

                Songs.Add(parsedSong);
            }
        }

        private void ExecutePlayPlaylistCommand()
        {
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
            if (_currentPlaylist == null) return;

            var confirm = MessageBox.Show(
                $"¿Seguro que quieres eliminar “{_currentPlaylist.PlaylistName}”?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            var result = await _playlistService.DeletePlaylistAsync(_currentPlaylist.Id);
            if (result.IsSuccess)
            {
                MessageBox.Show("Playlist eliminada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                _navigation.NavigateTo<HomeViewModel>();
            }
            else
            {
                MessageBox.Show($"No se pudo eliminar la playlist:\n{result.ErrorMessage}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExecuteRemoveSongAsync(Models.Song song)
        {
            if (_currentPlaylist == null || song == null)
                return;

            var result = await _playlistService.RemoveSongFromPlaylistAsync(song.IdSong.ToString(), _currentPlaylist.Id);

            if (result.IsSuccess)
            {
                Mediator.Notify(MediatorKeys.REFRESH_PLAYLISTS, null);
                Songs.Remove(song);
            }
            else
            {
                MessageBox.Show($"Error al eliminar la canción: {result.ErrorMessage}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
