using Services.Infrestructure;
using Services.Navigation;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SoundNest_Windows_Client.Models;
using SoundNest_Windows_Client.Utilities;

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

        private ImageSource profilePhoto;
        public ImageSource ProfilePhoto
        {
            get => profilePhoto;
            set { profilePhoto = value; OnPropertyChanged(); }
        }

        public RelayCommand ViewProfileCommand { get; set; }
        public RelayCommand ViewNotificationsCommand { get; set; }
        public RelayCommand GoHomeCommand { get; set; }
        public RelayCommand CreatePlaylistCommand { get; set; }
        public RelayCommand OpenPlaylistCommand { get; set; }
        public RelayCommand UploadSongCommand { get; set; }

        public ObservableCollection<Playlist> Playlists { get; set; } = new();

        public SideBarViewModel(INavigationService navigationService, IAccountService user)
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

            LoadProfileImage(user.CurrentUser.ProfileImagePath);
        }

        private void LoadProfileImage(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(imagePath, UriKind.Absolute);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                ProfilePhoto = image;
            }
            else
            {
                //TODO Opcional: asignar imagen por defecto si no hay archivo
                MessageBox.Show("Error, no se pudo cargar la foto");
                ProfilePhoto = null;
            }
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
            Mediator.Notify(MediatorKeys.HIDE_SEARCH_BAR, null);
            Navigation.NavigateTo<PlaylistDetailViewModel>();
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
