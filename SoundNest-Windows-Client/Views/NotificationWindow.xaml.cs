using SoundNest_Windows_Client.Utilities;
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
using System.Windows.Shapes;

namespace SoundNest_Windows_Client.Views
{
    /// <summary>
    /// Lógica de interacción para NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {
        public NotificationWindow(NotificationWindowViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove(); 
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); 
        }
    }
}
