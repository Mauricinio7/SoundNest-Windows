using Services.Communication.RESTful.Models.Playlist;
using Services.Communication.RESTful.Models.Songs;
using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SoundNest_Windows_Client.ViewModels
{
    class PlaylistDetailViewModel : Services.Navigation.ViewModel, IParameterReceiver
    {
        private readonly INavigationService _navigation;

        public PlaylistDetailViewModel(INavigationService navigation)
        {
            _navigation = navigation;
            Songs = new ObservableCollection<SongResponse>();
            BackCommand = new RelayCommand(_ => _navigation.NavigateTo<HomeViewModel>());
            PlaySongCommand = new RelayCommand(ExecutePlaySong);
            //TODO quitar
            var testPlaylist = new PlaylistResponse
            {
                PlaylistName = "Playlist de prueba",
                Songs = new List<SongResponse>
                {
                    new SongResponse { IdSong = 1, SongName = "Canción 1", UserName = "Artista A", DurationSeconds = 180 },
                    new SongResponse { IdSong = 2, SongName = "Canción 2", UserName = "Artista B", DurationSeconds = 200 },
                    new SongResponse { IdSong = 3, SongName = "Canción 3", UserName = "Artista C", DurationSeconds = 220 },
                }
            };
            LoadPlaylist(testPlaylist);
        }

        private string _playlistName = string.Empty;
        public string PlaylistName
        {
            get => _playlistName;
            set { _playlistName = value; OnPropertyChanged(); }
        }

        public ObservableCollection<SongResponse> Songs { get; }

        public RelayCommand BackCommand { get; }

        public RelayCommand PlaySongCommand { get; }

        public void ReceiveParameter(object parameter)
        {
            if (parameter is PlaylistResponse playlist)
            {
                LoadPlaylist(playlist);
            }
            else
            {
                MessageBox.Show(
                    "Error al cargar la playlist seleccionada.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void LoadPlaylist(PlaylistResponse playlist)
        {
            PlaylistName = playlist.PlaylistName;
            Songs.Clear();
            foreach (var song in playlist.Songs)
            {
                Songs.Add(song);
            }
        }

        private void ExecutePlaySong(object parameter)
        {
            if (parameter is SongResponse song)
            {
                Mediator.Notify(MediatorKeys.PLAY_SONG, song);
            }
        }
    }
}