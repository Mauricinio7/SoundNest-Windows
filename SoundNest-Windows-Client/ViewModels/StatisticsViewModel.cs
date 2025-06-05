using LiveCharts;
using LiveCharts.Wpf;
using Services.Navigation;
using Services.Communication.RESTful.Services;
using Services.Communication.RESTful.Models.Songs;
using Services.Communication.RESTful.Models;
using SoundNest_Windows_Client.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using Services.Infrestructure;
using System.Reflection;

namespace SoundNest_Windows_Client.ViewModels
{
    public class StatisticsViewModel : Services.Navigation.ViewModel
    {
        private readonly IVisualizationsService _visualizationsService;
        private readonly IAccountService _accountService;
        public INavigationService Navigation { get; set; }

        public SeriesCollection TopSongsByUserCollection { get; set; } = new();
        public List<string> TopSongsByUserLabels { get; set; } = new();
        public Func<double, string> TopSongsByUserFormatter { get; set; }
        public SeriesCollection GenresVisitCollection { get; set; } = new();
        public Func<ChartPoint, string> PointLabel { get; set; } = chartPoint => $"{chartPoint.SeriesView.Title}: {chartPoint.Y}";
        public SeriesCollection GlobalTopSongsCollection { get; set; } = new();
        public List<string> GlobalTopSongsLabels { get; set; } = new();
        public Func<double, string> GlobalTopSongsFormatter { get; set; } = val => val.ToString("N0");



        public StatisticsViewModel(INavigationService navigationService, IVisualizationsService visualizationsService, IAccountService accountService)
        {
            Navigation = navigationService;
            _visualizationsService = visualizationsService;
            _accountService = accountService;

            TopSongsByUserFormatter = value => value.ToString("N0");
            _ = LoadAllStatisticsAsync();
        }

        public async Task LoadAllStatisticsAsync()
        {
            try
            {
                Mediator.Notify(MediatorKeys.SHOW_LOADING_SCREEN, null);

                await Task.WhenAll(
                    LoadTopSongsByUserGraphAsync(),
                    LoadGenresVisitChartAsync(),
                    LoadGlobalTopSongsAsync()
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error al cargar las estadísticas.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mediator.Notify(MediatorKeys.HIDE_LOADING_SCREEN, null);
            }
        }


        private async Task LoadTopSongsByUserGraphAsync()
        {
            var userId = _accountService.CurrentUser.Id;
            var result = await _visualizationsService.GetTopSongsByUserAsync(userId);

            if (result.IsSuccess && result.Data != null)
            {
                var values = new ChartValues<double>();
                var labels = new List<string>();

                foreach (TopSongsResponse? song in result.Data)
                {
                    labels.Add(song.SongName);
                    values.Add(double.Parse(song.totalPlayCount));
                }

                TopSongsByUserLabels = labels;

                TopSongsByUserCollection = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Reproducciones",
                        Values = values,
                        Fill = new SolidColorBrush(Color.FromRgb(255, 166, 77)), 
                        Stroke = Brushes.White,
                        StrokeThickness = 1,
                        MaxColumnWidth = 45,
                        ColumnPadding = 10,
                        DataLabels = true,
                        LabelPoint = point => point.Y.ToString("N0"),
                        Foreground = Brushes.White
                    }
                };

                OnPropertyChanged(nameof(TopSongsByUserLabels));
                OnPropertyChanged(nameof(TopSongsByUserCollection));
            }
            else
            {
                MessageBox.Show("No se pudieron cargar las estadísticas del usuario.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadGenresVisitChartAsync()
        {
            var result = await _visualizationsService.GetTopGenresGlobalAsync();

            if (result.IsSuccess && result.Data is not null)
            {
                GenresVisitCollection.Clear();

                var genreData = result.Data
                    .Select(g => new
                    {
                        Name = g.GenreName,
                        Count = double.TryParse(g.totalPlayCount, out double val) ? val : 0
                    })
                    .Where(g => g.Count > 0)
                    .ToList();

                double total = genreData.Sum(g => g.Count);

                SolidColorBrush[] customPalette = new[]
                {
                    new SolidColorBrush(Color.FromRgb(178, 85, 247)),   
                    new SolidColorBrush(Color.FromRgb(255, 111, 73)),   
                    new SolidColorBrush(Color.FromRgb(146, 41, 179)),   
                    new SolidColorBrush(Color.FromRgb(216, 80, 42)),    
                    new SolidColorBrush(Color.FromRgb(255, 136, 0)),    
                    new SolidColorBrush(Color.FromRgb(109, 48, 123)),  
                    new SolidColorBrush(Color.FromRgb(234, 93, 120)),   
                    new SolidColorBrush(Color.FromRgb(187, 85, 59)),    
                };

                int index = 0;

                foreach (var genre in genreData)
                {
                    GenresVisitCollection.Add(new PieSeries
                    {
                        Title = genre.Name,
                        Values = new ChartValues<double> { genre.Count },
                        DataLabels = true,
                        LabelPoint = chartPoint =>
                        {
                            double percentage = (chartPoint.Y / total) * 100;
                            return $"{genre.Count}: ({percentage:N1}%)";
                        },
                        Fill = customPalette[index % customPalette.Length]
                    });
                    index++;
                }

                OnPropertyChanged(nameof(GenresVisitCollection));
            }
            else
            {
                MessageBox.Show("No se pudieron cargar las visitas por género.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private async Task LoadGlobalTopSongsAsync()
        {
            var result = await _visualizationsService.GetTopSongsGlobalAsync();

            if (result.IsSuccess && result.Data != null)
            {
                var values = new ChartValues<double>();
                var labels = new List<string>();

                foreach (var song in result.Data)
                {
                    labels.Add(song.SongName);
                    values.Add(double.Parse(song.totalPlayCount));
                }

                GlobalTopSongsLabels = labels;

                GlobalTopSongsCollection = new SeriesCollection
        {
            new LineSeries
            {
                Title = "Reproducciones globales",
                Values = values,
                PointGeometry = DefaultGeometries.Circle,
                Stroke = new SolidColorBrush(Color.FromRgb(255, 166, 77)), 
                Fill = new SolidColorBrush(Color.FromArgb(50, 255, 166, 77)), 
                PointForeground = Brushes.White,
                LineSmoothness = 0.8
            }
        };

                OnPropertyChanged(nameof(GlobalTopSongsLabels));
                OnPropertyChanged(nameof(GlobalTopSongsCollection));
            }
            else
            {
                MessageBox.Show("No se pudieron cargar las canciones globales más escuchadas.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




    }
}
