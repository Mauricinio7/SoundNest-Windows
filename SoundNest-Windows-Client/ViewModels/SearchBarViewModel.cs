using Services.Communication.RESTful.Models.Search;
using Services.Communication.RESTful.Models.Songs;
using Services.Communication.RESTful.Services;
using Services.Infrestructure;
using Services.Navigation;
using Song;
using SoundNest_Windows_Client.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace SoundNest_Windows_Client.ViewModels
{
    class SearchBarViewModel : Services.Navigation.ViewModel
    {
        private readonly string historyFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SoundNest", "recent_searches.json");

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

        private string searchText;
        public string SearchText
        {
            get => searchText;
            set
            {
                searchText = value;
                OnPropertyChanged();
            }
        }

        private bool isRecentVisible;
        public bool IsRecentVisible
        {
            get => isRecentVisible;
            set
            {
                isRecentVisible = value;
                OnPropertyChanged();
            }
        }

        private bool isFilterVisible;
        public bool IsFilterVisible
        {
            get => isFilterVisible;
            set { isFilterVisible = value; OnPropertyChanged(); }
        }
        public ObservableCollection<GenreResponse> GenreOptions { get; set; } = new();



        public string ArtistFilter { get; set; }

        public GenreResponse? GenreFilter { get; set; }

        public RelayCommand ApplyFilterCommand { get; set; }

        public RelayCommand FilterCommand { get; set; }
        public RelayCommand SearchCommand { get; set; }
        public RelayCommand RandomSearchCommand { get; set; }


        public ObservableCollection<string> RecentSearches { get; set; } = new();

       

        private Search searchSong = new Search();

        private ISongService songService;

        public SearchBarViewModel(INavigationService navigationService, ISongService songService)
        {
            Navigation = navigationService;
            this.songService = songService;

            SearchCommand = new RelayCommand(ExecuteSearchCommand);
            ApplyFilterCommand = new RelayCommand(ExecuteApplyFilterCommand);
            FilterCommand = new RelayCommand(ToggleFilterVisibility);
            RandomSearchCommand = new RelayCommand(ExecuteRandomSearchCommand);


            LoadHistoryFromFile();
            LoadGenresAsync();
        }

        private async void LoadGenresAsync()
        {
            var result = await songService.GetGenresAsync();

            if (result.IsSuccess && result.Data is not null)
            {
                GenreOptions.Clear();
                GenreOptions.Add(new GenreResponse
                {
                    IdSongGenre = -1,
                    GenreName = "Ninguno"
                });
                foreach (GenreResponse? genre in result.Data)
                {
                    GenreOptions.Add(genre);
                }

                OnPropertyChanged(nameof(GenreOptions));
            }
            else
            {
                MessageBox.Show("No se pudieron cargar los géneros", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void ToggleFilterVisibility()
        {
            if (!IsFilterVisible)
            {
                IsFilterVisible = true;
            }
            else
            {
                IsFilterVisible = false;
            }
                    
        }

        private void ExecuteApplyFilterCommand(object obj)
        {
            if (!string.IsNullOrWhiteSpace(ArtistFilter))
            {
                searchSong.ArtistName = ArtistFilter;
            }

            if (GenreFilter != null && GenreFilter.IdSongGenre != -1)
            {
                searchSong.IDGenre = GenreFilter.IdSongGenre;
            }


            IsFilterVisible = false;

            ExecuteSearchCommand(null);
        }

        private void ExecuteRandomSearchCommand()
        {
            searchSong.IsRandom = true;
            Navigation.NavigateTo<SearchResultsViewModel>(searchSong);

        }


        private void ExecuteSearchCommand(object parameter)
        {
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                searchSong.SongName = SearchText;
            }

            if(string.IsNullOrWhiteSpace(searchSong.SongName) && searchSong.IDGenre == null  && string.IsNullOrWhiteSpace(searchSong.ArtistName))
            {
                return;
            }   

            if (RecentSearches == null || RecentSearches.Count == 0)
                LoadHistoryFromFile();

            if (RecentSearches.Contains(SearchText))
                RecentSearches.Remove(SearchText);

            if (RecentSearches.Count >= 10)
                RecentSearches.RemoveAt(RecentSearches.Count - 1);

            RecentSearches.Insert(0, SearchText);

            SaveHistoryToFile();

            IsRecentVisible = false;

            Navigation.NavigateTo<SearchResultsViewModel>(searchSong);
            searchSong.SongName = null;
            searchSong.ArtistName = null;
            searchSong.IDGenre = null;
        }

        private void LoadHistoryFromFile()
        {
                if (File.Exists(historyFilePath))
                {
                    var json = File.ReadAllText(historyFilePath);
                    var loadedList = JsonSerializer.Deserialize<List<string>>(json);
                    if (loadedList != null)
                        RecentSearches = new ObservableCollection<string>(loadedList);
                }
                else
                {
                    RecentSearches = new ObservableCollection<string>();
                }

                OnPropertyChanged(nameof(RecentSearches));
        }

        private void SaveHistoryToFile()
        {
                var dir = Path.GetDirectoryName(historyFilePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonSerializer.Serialize(RecentSearches.ToList());
                File.WriteAllText(historyFilePath, json);  
        }
    }
}