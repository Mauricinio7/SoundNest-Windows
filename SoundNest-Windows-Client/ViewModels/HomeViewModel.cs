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

            //TODO Tests You can use this
            //ToastHelper.ShowToast($"¡Bienvenido, {currentUser.CurrentUser.Name}!", NotificationType.Information, "Inicio de sesión");
            //ToastHelper.ShowToast($"¡Bienvenido, {currentUser.CurrentUser.Name}!", NotificationType.Warning, "Inicio de sesión");

            //bool confirmed = DialogHelper.ShowConfirmation("Eliminar playlist", "¿Seguro que deseas eliminar esta playlist?");
            //MessageBox.Show(confirmed.ToString());
            //DialogHelper.ShowAcceptDialog("Éxito", "La canción se subió correctamente. slkjuadlisahldoihaslodhaslkjdhaslkjjdhasldkjhasldkjashdlaksjdhlaskjdhaskdjasldkjashdlaksjhdñlkajsñlkajsñd", AcceptDialogType.Confirmation);
            //DialogHelper.ShowAcceptDialog("Error", "No se pudo guardar la playlist.", AcceptDialogType.Error);

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
                    Models.Song realSong = new Models.Song();
                    realSong.IdSong = song.IdSong;
                    realSong.IdSongExtension = song.IdSongExtension;
                    realSong.IdSongGenre = song.IdSongGenre;
                    realSong.IsDeleted = song.IsDeleted;
                    realSong.PathImageUrl = song.PathImageUrl;
                    realSong.ReleaseDate = song.ReleaseDate;
                    realSong.SongName = song.SongName;
                    realSong.UserName = song.UserName;
                    realSong.FileName = song.FileName;
                    realSong.DurationSeconds = song.DurationSeconds;
                    realSong.Description = song.Description;
                    realSong.DurationSeconds = song.DurationSeconds;

                    if (!string.IsNullOrEmpty(song.PathImageUrl) && song.PathImageUrl.Length > 1)
                    {
                        realSong.Image = await ImagesHelper.LoadImageFromUrlAsync(string.Concat(ApiRoutes.BaseUrl, song.PathImageUrl.AsSpan(1)));
                    }
                    else
                    {
                        realSong.Image = ImagesHelper.LoadDefaultImage("pack://application:,,,/Resources/Images/Icons/Default_Song_Icon.png");
                    }

                    RecentSongs.Add(realSong);
                }
            }
            else
            {
                MessageBox.Show(result.Message ?? "Error al obtener canciones recientes", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadPopularSongsAsync()
        {
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            var result = await songService.GetPopularSongsByMonthAsync(50, currentYear, currentMonth);

            if (result.IsSuccess && result.Data is not null)
            {
                PopularSongs.Clear();

                foreach (SongResponse song in result.Data)
                {
                    var realSong = new Models.Song
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
                        DurationFormatted = TimeSpan.FromSeconds(song.DurationSeconds).ToString(@"m\:ss")
                    };

                    if (!string.IsNullOrEmpty(song.PathImageUrl) && song.PathImageUrl.Length > 1)
                    {
                        realSong.Image = await ImagesHelper.LoadImageFromUrlAsync(string.Concat(ApiRoutes.BaseUrl, song.PathImageUrl.AsSpan(1)));
                    }
                    else
                    {
                        realSong.Image = ImagesHelper.LoadDefaultImage("pack://application:,,,/Resources/Images/Icons/Default_Song_Icon.png");
                    }

                    PopularSongs.Add(realSong);
                }
            }
            else
            {
                MessageBox.Show(result.Message ?? "Error al obtener canciones populares", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                MessageBox.Show("Error al reproducir la canción", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
