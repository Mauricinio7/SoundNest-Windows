using Services.Infrestructure;
using Services.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundNest_Windows_Client.Utilities;
using System.Windows;

namespace SoundNest_Windows_Client.ViewModels
{
    class SideBarViewModel : Services.Navigation.ViewModel
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

        public RelayCommand ViewProfileCommand { get; set; }
        public RelayCommand ViewNotificationsCommand { get; set; }
        public RelayCommand GoHomeCommand { get; set; }
        public RelayCommand CreatePlaylistCommand { get; set; }
        public RelayCommand OpenPlaylistCommand { get; set; }
        public RelayCommand UploadSongCommand { get; set; }

        public ObservableCollection<Playlist> Playlists { get; set; } = new();

        public SideBarViewModel(INavigationService navigationService)
        {
            Navigation = navigationService;

            ViewProfileCommand = new RelayCommand(ExecuteViewProfileCommand);
            GoHomeCommand = new RelayCommand(ExecuteGoHomeCommand);
            CreatePlaylistCommand = new RelayCommand(ExecuteCreatePlaylistCommand);
            OpenPlaylistCommand = new RelayCommand(ExecuteOpenPlaylistCommand);
            ViewNotificationsCommand = new RelayCommand(ExecuteViewNotificationsCommand);
            UploadSongCommand = new RelayCommand(ExecuteUploadSongCommand);

            Mediator.Register(MediatorKeys.ADD_PLAYLIST, (param) =>
            {
                if (param is Playlist newPlaylist)
                    App.Current.Dispatcher.Invoke(() => Playlists.Add(newPlaylist));
            });

        }

        private void ExecuteUploadSongCommand(object parameter)
        {
            Mediator.Notify(MediatorKeys.HIDE_SEARCH_BAR, null);
            Mediator.Notify(MediatorKeys.HIDE_MUSIC_PLAYER, null);
            Navigation.NavigateTo<UploadSongViewModel>();
        }

        private void ExecuteViewNotificationsCommand(object parameter)
        {
            Mediator.Notify(MediatorKeys.HIDE_SEARCH_BAR, null);
            Navigation.NavigateTo<NotificationViewModel>();
        }

        private void ExecuteOpenPlaylistCommand(object parameter)
        {
            if (parameter is Playlist playlist)
            {
                MessageBox.Show($"Abriendo la playlist: {playlist.Name}", "Abrir Playlist", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExecuteViewProfileCommand(object parameter)
        {
            Mediator.Notify(MediatorKeys.HIDE_SEARCH_BAR, null);
            Navigation.NavigateTo<ProfileViewModel>();
        }

        private void ExecuteGoHomeCommand(object parameter)
        {
            Mediator.Notify(MediatorKeys.SHOW_SEARCH_BAR, null);
            Mediator.Notify(MediatorKeys.SHOW_MUSIC_PLAYER, null);
            Navigation.NavigateTo<HomeViewModel>();
        }

        private void ExecuteCreatePlaylistCommand(object parameter)
        {
            Mediator.Notify(MediatorKeys.HIDE_SEARCH_BAR, null);
            Navigation.NavigateTo<CreatePlaylistViewModel>();
        }


    }
}
