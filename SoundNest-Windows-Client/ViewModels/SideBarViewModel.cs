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
using Services.Communication.RESTful.Services;
using Services.Communication.RESTful.Models.Playlist;
using UserImage;
using Services.Communication.gRPC.Http;
using Services.Communication.gRPC.Services;

namespace SoundNest_Windows_Client.ViewModels
{
    class SideBarViewModel : Services.Navigation.ViewModel
    {
        private readonly IPlaylistService _playlistService;
        private readonly IAccountService _accountService;
        private readonly string _userId;

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
        public RelayCommand RefreshPlaylistsCommand { get; set; }
        public RelayCommand OpenPlaylistCommand { get; set; }
        public RelayCommand UploadSongCommand { get; set; }

        public ObservableCollection<PlaylistResponse> Playlists { get; set; } = new();

        private readonly IGrpcClientManager clientManager;
        private readonly IUserImageServiceClient userImageService;

        public SideBarViewModel(INavigationService navigationService, IAccountService user, IPlaylistService playlistService, IGrpcClientManager clientService, IUserImageServiceClient userImageService)
        {
            Navigation = navigationService;
            _playlistService = playlistService;
            _accountService = user; 
            this.userImageService = userImageService;
            clientManager = clientService;

            _userId = _accountService.CurrentUser.Id.ToString();

            ViewProfileCommand = new RelayCommand(ExecuteViewProfileCommand);
            GoHomeCommand = new RelayCommand(ExecuteGoHomeCommand);
            CreatePlaylistCommand = new RelayCommand(ExecuteCreatePlaylistCommand);
            OpenPlaylistCommand = new RelayCommand(ExecuteOpenPlaylistCommand);
            RefreshPlaylistsCommand = new RelayCommand(ExecuteRefreshPlaylistsCommand);
            ViewNotificationsCommand = new RelayCommand(ExecuteViewNotificationsCommand);
            UploadSongCommand = new RelayCommand(ExecuteUploadSongCommand);

            Mediator.Register(MediatorKeys.ADD_PLAYLIST, param =>
            {
                if (param is PlaylistResponse newPlaylist)
                    App.Current.Dispatcher.Invoke(() => Playlists.Add(newPlaylist));
            });

            EnsureTokenIsConfigured();
            _ = LoadPlaylistsAsync();
            _ = LoadProfileImage();
        }

        private void EnsureTokenIsConfigured()
        {
            var token = TokenStorageHelper.LoadToken();
            if (!string.IsNullOrWhiteSpace(token))
            {
                clientManager.SetAuthorizationToken(token);
            }
        }

        private async Task LoadProfileImage()
        {
            try
            {
                var response = await userImageService.DownloadImageAsync(_accountService.CurrentUser.Id);

                byte[] imageBytes = response.ImageData.ToByteArray();

                using var stream = new MemoryStream(imageBytes);

                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze(); 

                ProfilePhoto = image;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo cargar la imagen de perfil: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            Navigation.NavigateTo<HomeViewModel>();
        }

        private void ExecuteCreatePlaylistCommand(object parameter)
        {
            Mediator.Notify(MediatorKeys.HIDE_SEARCH_BAR, null);
            Navigation.NavigateTo<CreatePlaylistViewModel>();
        }

        private void ExecuteRefreshPlaylistsCommand(object parameter)
        {
            _ = LoadPlaylistsAsync();
        }

        private async Task LoadPlaylistsAsync()
        {
            var result = await _playlistService.GetPlaylistsByUserIdAsync(_userId);
            if (!result.IsSuccess || result.Data is null)
                MessageBox.Show(result.ErrorMessage);
                return;

            App.Current.Dispatcher.Invoke(() =>
            {
                Playlists.Clear();
                foreach (var dto in result.Data)
                    Playlists.Add(dto);
            });
        }

    }
}
