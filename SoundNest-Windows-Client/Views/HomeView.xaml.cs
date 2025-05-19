using System.Windows;
using System.Windows.Controls;

namespace SoundNest_Windows_Client.Views
{
    /// <summary>
    /// Lógica de interacción para HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
            Loaded += HomeView_Loaded;
        }

        private void HomeView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.HomeViewModel vm)
            {
                vm.RegisterScrollViewer(CarouselRecentSongsScrollViewer);
                vm.RegisterPopularScrollViewer(CarouselPopularSongsScrollViewer);
            }
        }
    }
}
