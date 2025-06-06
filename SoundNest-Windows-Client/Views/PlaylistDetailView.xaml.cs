using Services.Communication.RESTful.Models.Songs;
using SoundNest_Windows_Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SoundNest_Windows_Client.Views
{
    /// <summary>
    /// Lógica de interacción para PlaylistDetailView.xaml
    /// </summary>
    public partial class PlaylistDetailView : UserControl
    {
        public PlaylistDetailView()
        {
            InitializeComponent();
        }
        private void OnDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Models.Song song)
            {
                var vm = DataContext as PlaylistDetailViewModel;
                vm.SelectedSong = song;
                DeleteSongPopup.PlacementTarget = btn;
                vm.IsDeletePopupVisible = true;
            }
        }

        private void OnDeletePopupClosed(object sender, System.EventArgs e)
        {
            var vm = DataContext as PlaylistDetailViewModel;
            vm.IsDeletePopupVisible = false;
        }
    }
}
