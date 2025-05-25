using Services.Communication.RESTful.Models.Playlist;
using Services.Communication.RESTful.Models.Songs;
using Services.Communication.RESTful.Services;
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
        private readonly IPlaylistService _playlistService;

        private PlaylistResponse _currentPlaylist;

        public PlaylistDetailViewModel(INavigationService navigation, IPlaylistService playlistService)
        {
            _navigation = navigation;
            _playlistService = playlistService;

            Songs = new ObservableCollection<SongResponse>();
            BackCommand = new RelayCommand(_ => _navigation.NavigateTo<HomeViewModel>());
            PlaySongCommand = new RelayCommand(ExecutePlaySong);
            EditPlaylistCommand = new RelayCommand(_ => ExecuteEditPlaylist());
            DeletePlaylistCommand = new RelayCommand(async _ => await ExecuteDeletePlaylistAsync());
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
        public RelayCommand EditPlaylistCommand { get; }
        public RelayCommand DeletePlaylistCommand { get; }

        public void ReceiveParameter(object parameter)
        {
            if (parameter is PlaylistResponse playlist)
            {
                _currentPlaylist = playlist;
                LoadPlaylist(playlist);
            }
            else
            {
                MessageBox.Show("Error al cargar la playlist seleccionada.");
            }
        }

        private void LoadPlaylist(PlaylistResponse playlist)
        {
            PlaylistName = playlist.PlaylistName;
            Songs.Clear();
            foreach (var song in playlist.Songs)
                Songs.Add(song);
        }

        private void ExecutePlaySong(object parameter)
        {
            if (parameter is SongResponse song)
            {
                Mediator.Notify(MediatorKeys.PLAY_SONG, song);
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
    }
}