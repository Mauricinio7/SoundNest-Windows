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

        public ObservableCollection<SongResponse> PopularSongsList { get; set; } = new();

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





        public ObservableCollection<SongResponse> PopularSongs { get; set; } = new();

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

            //PlaySongCommand = new RelayCommand();

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
                PopularSongs.Clear();

                foreach (var song in result.Data)
                {
                    PopularSongs.Add(song);
                }
            }
            else
            {
                MessageBox.Show(result.Message ?? "Error al obtener canciones recientes", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadPopularSongsAsync()
        {
            var result = await songService.GetRecentSongsAsync(10); //TODO Change to popularSongs

            if (result.IsSuccess && result.Data is not null)
            {
                PopularSongs.Clear();

                foreach (var song in result.Data)
                {
                    PopularSongs.Add(song);
                }
            }
            else
            {
                MessageBox.Show(result.Message ?? "Error al obtener canciones recientes", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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



    }
}
