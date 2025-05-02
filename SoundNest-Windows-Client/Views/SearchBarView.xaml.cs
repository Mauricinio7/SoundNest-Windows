using Microsoft.Extensions.DependencyInjection;
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
using System.Windows.Threading;

namespace SoundNest_Windows_Client.Views
{
    /// <summary>
    /// Lógica de interacción para SearchBarView.xaml
    /// </summary>
    public partial class SearchBarView : UserControl
    {
        public SearchBarView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<SearchBarViewModel>();
            Application.Current.Deactivated += OnAppLostFocus;
            Application.Current.MainWindow.PreviewMouseDown += OnWindowClick;

        }
        private void TextBoxSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is SearchBarViewModel vm)
            {
                vm.IsRecentVisible = true;
            }
        }

        private void TextBoxSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (DataContext is SearchBarViewModel vm)
                {
                    vm.IsRecentVisible = false;
                    SearchButton.Focus();
                }
            }), DispatcherPriority.Background);
        }

        private void RecentSearch_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && DataContext is SearchBarViewModel vm)
            {
                vm.SearchText = tb.Text;
                vm.IsRecentVisible = false;
                SearchButton.Focus();
            }
        }
        private void OnAppLostFocus(object? sender, EventArgs e)
        {
            if (DataContext is SearchBarViewModel vm)
                vm.IsRecentVisible = false;
            SearchButton.Focus();
        }
        private void OnWindowClick(object sender, MouseButtonEventArgs e)
        {
            if (RecentPopup == null || !RecentPopup.IsOpen)
                return;

            if (!IsClickInsidePopup(e))
            {
                if (DataContext is SearchBarViewModel vm)
                    vm.IsRecentVisible = false;
                SearchButton.Focus();
            }
        }
        private bool IsClickInsidePopup(MouseButtonEventArgs e)
        {
            if (RecentPopup.Child is FrameworkElement popupChild)
            {
                var mousePos = e.GetPosition(popupChild);
                return mousePos.X >= 0 && mousePos.X <= popupChild.ActualWidth &&
                       mousePos.Y >= 0 && mousePos.Y <= popupChild.ActualHeight;
            }

            return false;
        }
        private void TextBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is SearchBarViewModel vm)
            {
                if (vm.SearchCommand.CanExecute(null))
                {
                    vm.SearchCommand.Execute(null);
                    SearchButton.Focus(); 
                    e.Handled = true;
                }
            }
        }
    }
}
