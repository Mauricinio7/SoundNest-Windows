using LiveCharts;
using LiveCharts.Wpf;
using Services.Navigation;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace SoundNest_Windows_Client.ViewModels
{
    public class StatisticsViewModel : Services.Navigation.ViewModel
    {
        public INavigationService Navigation { get; set; }

        public SeriesCollection SeriesCollection { get; set; }
        public List<string> Labels { get; set; }
        public Func<double, string> Formatter { get; set; }

        public StatisticsViewModel(INavigationService navigationService)
        {
            Navigation = navigationService;

            // Datos de prueba (cámbialos por los reales de la API)
            Labels = new List<string> { "Ene", "Feb", "Mar", "Abr", "May", "Jun" };
            var values = new ChartValues<double> { 120, 85, 140, 200, 180, 210 };

            SeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Visitas",
                    Values = values,
                    Fill = new SolidColorBrush(Color.FromRgb(50, 150, 250))
                }
            };

            Formatter = value => value.ToString("N0");
        }
    }
}
