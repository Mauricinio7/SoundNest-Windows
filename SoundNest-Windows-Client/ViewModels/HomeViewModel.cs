using Services.Navigation;
using SoundNest_Windows_Client.Models;
using Services.Communication.RESTful.Http;
using SoundNest_Windows_Client.Utilities;
using System.Windows;
using Services.Communication.RESTful.Services;
using System.Collections.ObjectModel;
using Services.Communication.RESTful.Models.Songs;
using Services.Infrestructure;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using System.Net.Http;
using Services.Communication.RESTful.Constants;
using System.Windows.Input;
using SoundNest_Windows_Client.Notifications;
using SoundNest_Windows_Client.Resources.Controls;
using Services.Communication.RESTful.Models;
using System.Net;

namespace SoundNest_Windows_Client.ViewModels
{
    class HomeViewModel : Services.Navigation.ViewModel
    {
        private INavigationService navigation;
        private readonly IAccountService currentUser;
        private readonly IApiClient apiClient;
        private readonly ISongService songService;

        public INavigationService Navigation
        {
            get => navigation;
            set
            {
                navigation = value;
                OnPropertyChanged();
            }
        }
        public RelayCommand ScrollLeftCommand { get; set; }
        public RelayCommand ScrollRightCommand { get; set; }
        public RelayCommand PlaySongCommand { get; set; }

        private ScrollViewer? _scrollViewer;
        private bool canScrollLeft;
        public bool CanScrollLeft
        {
            get => canScrollLeft;
            set { canScrollLeft = value; OnPropertyChanged(); }
        }

        private bool canScrollRight;
        public bool CanScrollRight
        {
            get => canScrollRight;
            set { canScrollRight = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Models.Song> RecentSongsList { get; set; } = new();
        public ObservableCollection<Models.Song> PopulartSongsList { get; set; } = new();

        private ScrollViewer? _popularScrollViewer;

        private bool canScrollLeftPopular;
        public bool CanScrollLeftPopular
        {
            get => canScrollLeftPopular;
            set { canScrollLeftPopular = value; OnPropertyChanged(); }
        }

        private bool canScrollRightPopular;
        public bool CanScrollRightPopular
        {
            get => canScrollRightPopular;
            set { canScrollRightPopular = value; OnPropertyChanged(); }
        }

        public RelayCommand ScrollLeftPopularCommand { get; set; }
        public RelayCommand ScrollRightPopularCommand { get; set; }

        private bool _noRecentSongs;
        public bool NoRecentSongs
        {
            get => _noRecentSongs;
            set { _noRecentSongs = value; OnPropertyChanged(); }
        }

        private bool _noPopularSongs;
        public bool NoPopularSongs
        {
            get => _noPopularSongs;
            set { _noPopularSongs = value; OnPropertyChanged(); }
        }






        public ObservableCollection<Models.Song> RecentSongs { get; set; } = new();
        public ObservableCollection<Models.Song> PopularSongs { get; set; } = new ();

        public HomeViewModel(INavigationService navigationService, IAccountService user, IApiClient apiClient, ISongService songService)
        {
            Navigation = navigationService;
            currentUser = user;
            this.apiClient = apiClient;
            this.songService = songService;

            EnsureTokenIsConfigured();

            _ = LoadRecentSongsAsync();
            _ = LoadPopularSongsAsync();
            ScrollLeftCommand = new RelayCommand(_ => _scrollViewer?.ScrollToHorizontalOffset(_scrollViewer.HorizontalOffset - 300));
            ScrollRightCommand = new RelayCommand(_ => _scrollViewer?.ScrollToHorizontalOffset(_scrollViewer.HorizontalOffset + 300));
            ScrollLeftPopularCommand = new RelayCommand(_ => _popularScrollViewer?.ScrollToHorizontalOffset(_popularScrollViewer.HorizontalOffset - 300));
            ScrollRightPopularCommand = new RelayCommand(_ => _popularScrollViewer?.ScrollToHorizontalOffset(_popularScrollViewer.HorizontalOffset + 300));

            PlaySongCommand = new RelayCommand(PlaySong);
            Mediator.Notify(MediatorKeys.REFRESH_PLAYLISTS, null);

        }

        private void EnsureTokenIsConfigured()
        {
            var token = TokenStorageHelper.LoadToken();
            if (!string.IsNullOrWhiteSpace(token))
            {
                this.apiClient.SetAuthorizationToken(token);
            }
        }

        private async Task LoadRecentSongsAsync()
        {
            var result = await songService.GetRecentSongsAsync(10);

            if (result.IsSuccess && result.Data is not null)
            {
                RecentSongs.Clear();

                foreach (var song in result.Data)
                {
                    var realSong = CreateSongModel(song);
                    RecentSongs.Add(realSong);

                    if (!string.IsNullOrEmpty(song.PathImageUrl) && song.PathImageUrl.Length > 1)
                    {
                        _ = Task.Run(async () =>
                        {
                            var image = await ImagesHelper.LoadImageFromUrlAsync($"{ApiRoutes.BaseUrl}{song.PathImageUrl[1..]}");
                            Application.Current.Dispatcher.Invoke(() => realSong.Image = image);
                        });
                    }
                }
            }
            else
            {
                ShowSongLoadError(result, "recientes");
            }
            NoRecentSongs = RecentSongs.Count == 0;
        }

        private async Task LoadPopularSongsAsync()
        {
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            var result = await songService.GetPopularSongsByMonthAsync(50, currentYear, currentMonth);

            if (result.IsSuccess && result.Data is not null)
            {
                PopularSongs.Clear();

                foreach (var song in result.Data)
                {
                    var realSong = CreateSongModel(song);
                    PopularSongs.Add(realSong);

                    if (!string.IsNullOrEmpty(song.PathImageUrl) && song.PathImageUrl.Length > 1)
                    {
                        _ = Task.Run(async () =>
                        {
                            var image = await ImagesHelper.LoadImageFromUrlAsync($"{ApiRoutes.BaseUrl}{song.PathImageUrl[1..]}");
                            Application.Current.Dispatcher.Invoke(() => realSong.Image = image);
                        });
                    }
                }
            }
            else
            {
                ShowSongLoadError(result, "populares");
            }
            NoPopularSongs = PopularSongs.Count == 0;
        }

        private void ShowSongLoadError(ApiResult<List<SongResponse>> result, string tipo)
        {
            string title = "Error al cargar canciones";

            string message = result.StatusCode switch
            {
                HttpStatusCode.BadRequest => "La solicitud no es válida. Intenta recargar las canciones.",
                HttpStatusCode.NotFound => $"No se encontraron canciones {tipo}.",
                HttpStatusCode.InternalServerError => "Ocurrió un error interno en el servidor. Intenta más tarde.",
                _ => result.Message ?? "No se pudieron obtener las canciones. Intenta de nuevo más tarde."
            };

            ToastHelper.ShowToast(message, NotificationType.Error, title);
        }

        private Models.Song CreateSongModel(SongResponse song)
        {
            return new Models.Song
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
                Image = ImagesHelper.LoadDefaultImage("pack://application:,,,/Resources/Images/Icons/Default_Song_Icon.png")
            };
        }







        private void PlaySong(object parameter)
        {
            if (parameter is Models.Song song)
            {
                Mediator.Notify(MediatorKeys.HIDE_MUSIC_PLAYER, null);
                Mediator.Notify(MediatorKeys.SHOW_MUSIC_PLAYER, song);
            }
            else
            {
                ToastHelper.ShowToast("Error al reproducir la canción, intentelo de nuevo más tarde", NotificationType.Error, "Error");
            }   
        }

        public void RegisterScrollViewer(ScrollViewer scrollViewer)
        {
            _scrollViewer = scrollViewer;
            _scrollViewer.ScrollChanged += (s, e) => UpdateScrollButtonVisibility();
            UpdateScrollButtonVisibility();
        }
        private void UpdateScrollButtonVisibility()
        {
            if (_scrollViewer == null) return;

            CanScrollLeft = _scrollViewer.HorizontalOffset > 0;
            CanScrollRight = _scrollViewer.HorizontalOffset < _scrollViewer.ScrollableWidth;
        }

        public void RegisterPopularScrollViewer(ScrollViewer scrollViewer)
        {
            _popularScrollViewer = scrollViewer;
            _popularScrollViewer.ScrollChanged += (_, __) => UpdatePopularScrollButtonVisibility();
            UpdatePopularScrollButtonVisibility();
        }

        private void UpdatePopularScrollButtonVisibility()
        {
            if (_popularScrollViewer == null) return;

            CanScrollLeftPopular = _popularScrollViewer.HorizontalOffset > 0;
            CanScrollRightPopular = _popularScrollViewer.HorizontalOffset + _popularScrollViewer.ViewportWidth < _popularScrollViewer.ExtentWidth;
        }

        private void CarouselPopularSongsScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                double offsetChange = e.Delta > 0 ? -100 : 100;
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + offsetChange);
                e.Handled = true;
            }
        }

        private void CarouselRecentSongsScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                double offsetChange = e.Delta > 0 ? -100 : 100;
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + offsetChange);
                e.Handled = true;
            }
        }

    }
}
