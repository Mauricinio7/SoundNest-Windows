using Services.Infrestructure;
using Services.Navegation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;

namespace SoundNest_Windows_Client.ViewModels
{
    class SearchBarViewModel : Services.Navegation.ViewModel
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

        public ObservableCollection<string> RecentSearches { get; set; } = new();

        public RelayCommand SearchCommand { get; set; }

        public SearchBarViewModel(INavigationService navigationService)
        {
            Navigation = navigationService;

            SearchCommand = new RelayCommand(ExecuteSearchCommand);

            LoadHistoryFromFile();
        }

        private void ExecuteSearchCommand(object parameter)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return;

            if (RecentSearches == null || RecentSearches.Count == 0)
                LoadHistoryFromFile();

            if (RecentSearches.Contains(SearchText))
                RecentSearches.Remove(SearchText);

            if (RecentSearches.Count >= 10)
                RecentSearches.RemoveAt(RecentSearches.Count - 1);

            RecentSearches.Insert(0, SearchText);

            SaveHistoryToFile();

            Console.WriteLine($"Buscando: {SearchText}");

            IsRecentVisible = false;
        }

        private void LoadHistoryFromFile()
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar historial: {ex.Message}");
            }
        }

        private void SaveHistoryToFile()
        {
            try
            {
                var dir = Path.GetDirectoryName(historyFilePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonSerializer.Serialize(RecentSearches.ToList());
                File.WriteAllText(historyFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar historial: {ex.Message}");
            }
        }
    }
}