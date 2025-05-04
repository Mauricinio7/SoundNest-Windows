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
using SoundNest_Windows_Client.ViewModels;
using SoundNest_Windows_Client.Utilities;

namespace SoundNest_Windows_Client.Views
{
    /// <summary>
    /// Lógica de interacción para NotificationsView.xaml
    /// </summary>
    public partial class NotificationsView : UserControl
    {
        public NotificationsView()
        {
            InitializeComponent();
        }

        private void Notification_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is Notification notif)
            {
                var vm = (NotificationViewModel)DataContext;
                vm.OpenNotificationCommand.Execute(notif);
            }
        }
    }
}
