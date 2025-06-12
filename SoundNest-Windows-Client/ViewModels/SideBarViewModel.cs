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
using Services.Communication.RESTful.Constants;
using Song;
using SoundNest_Windows_Client.Notifications;
using SoundNest_Windows_Client.Resources.Controls;
using System.Net;

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
        public RelayCommand OpenPlaylistCommand { get; }
        public RelayCommand UploadSongCommand { get; set; }

        public ObservableCollection<Models.Playlist> Playlists { get; set; } = new();

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

            Mediator.Register(MediatorKeys.REFRESH_PLAYLISTS, _ =>
            {
                ExecuteRefreshPlaylistsCommand(null);
            });

            Mediator.Register(MediatorKeys.UPLOAD_USER_IMAGE, _ =>
            {
                LoadProfileImage();
            });

            EnsureTokenIsConfigured();
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
            ProfilePhoto = (BitmapImage)ImagesHelper.LoadDefaultImage("pack://application:,,,/Resources/Images/Icons/Default_ProfileImage_Icon.png");

            try
            {
                var response = await userImageService.DownloadImageAsync(_accountService.CurrentUser.Id);

                if (response.ImageData != null && response.ImageData.Length > 0)
                {
                    byte[] imageBytes = response.ImageData.ToByteArray();

                    using var stream = new MemoryStream(imageBytes);

                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                    image.Freeze();

                    if (image.PixelWidth > 0 && image.PixelHeight > 0)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ProfilePhoto = image;
                        });
                    }
                }
            }
            catch
            {}
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

        private async void ExecuteOpenPlaylistCommand(object parameter)
        {
            if (parameter is not Playlist playlist)
            {
                ToastHelper.ShowToast("Ocurrió un error inesperado al abrir la playlist, inténtelo con otra.", NotificationType.Error, "Error");
                return;
            }

                var songIds = playlist.PlaylistSongs.Select(s => s.SongId).ToList();

            Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);
            var songDetailsResult = await _playlistService.GetSongsDetailsAsync(songIds);
            Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);

            if (songDetailsResult.IsSuccess || songDetailsResult.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var detailedPlaylist = new Playlist
                {
                    Id = playlist.Id,
                    CreatorId = playlist.CreatorId,
                    PlaylistName = playlist.PlaylistName,
                    Description = playlist.Description,
                    ImagePath = playlist.ImagePath,
                    CreatedAt = playlist.CreatedAt,
                    Version = playlist.Version,
                    Songs = songDetailsResult.Data
                };

                Mediator.Notify(MediatorKeys.HIDE_SEARCH_BAR, null);
                Navigation.NavigateTo<PlaylistDetailViewModel>(detailedPlaylist);
            }
            else
            {
                ShowPlaylistDetailsLoadError(songDetailsResult.StatusCode);
            }
        }

        private void ShowPlaylistDetailsLoadError(HttpStatusCode? statusCode)
        {
            string title = "Error al cargar la Playlist";

            string message = statusCode switch
            {
                HttpStatusCode.BadRequest => "Ocurrió un error al intentar cargar las canciones de la playlist. Inténtalo nuevamente más tarde.",
                HttpStatusCode.InternalServerError => "Ocurrio un error inesperado al cargar la playlist. Por favor, intenta más tarde.",
                _ => "No se pudo conectar con el servidor. Revisa tu conexión a internet."
            };

            DialogHelper.ShowAcceptDialog(title, message, AcceptDialogType.Error);
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
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Playlists.Clear();
                    ShowUserPlaylistsLoadError(result.StatusCode);
                });
                return;
            }

            var playlists = new List<Playlist>();

            foreach (var playlistResponse in result.Data)
            {
                var playlist = new Playlist
                {
                    Id = playlistResponse.Id,
                    CreatorId = playlistResponse.CreatorId,
                    PlaylistName = playlistResponse.PlaylistName,
                    Description = playlistResponse.Description,
                    ImagePath = playlistResponse.ImagePath,
                    CreatedAt = playlistResponse.CreatedAt,
                    Version = playlistResponse.Version,
                    PlaylistSongs = playlistResponse.Songs,
                    Image = ImagesHelper.LoadDefaultImage("pack://application:,,,/Resources/Images/Icons/Default_Song_Icon.png")
                };

                playlists.Add(playlist);

                if (!string.IsNullOrEmpty(playlist.ImagePath) && playlist.ImagePath.Length > 1)
                {
                    var imageUrl = $"{ApiRoutes.BaseUrl}{playlist.ImagePath[1..]}";
                    _ = Task.Run(async () =>
                    {
                        var image = await ImagesHelper.LoadImageFromUrlAsync(imageUrl);
                        if (image != null)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                playlist.Image = image;
                            });
                        }
                    });
                }
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                Playlists.Clear();
                foreach (var playlist in playlists)
                {
                    Playlists.Add(playlist);
                }
            });
        }

        private void ShowUserPlaylistsLoadError(HttpStatusCode? statusCode)
        {
            string title = "Error al cargar playlists";

            string message = statusCode switch
            {
                HttpStatusCode.NotFound => "No se encontraron playlists para este usuario.",
                HttpStatusCode.InternalServerError => "Ocurrió un error inesperado al obtener las playlists. Intenta más tarde.",
                _ => "No se pudo conectar con el servidor. Revisa tu conexión a internet."
            };

            ToastHelper.ShowToast(message, NotificationType.Warning, title);
        }


    }

}

