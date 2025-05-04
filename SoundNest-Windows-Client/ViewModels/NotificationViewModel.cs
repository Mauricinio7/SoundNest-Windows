using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SoundNest_Windows_Client.Views;

namespace SoundNest_Windows_Client.ViewModels
{
    class NotificationViewModel : Services.Navigation.ViewModel
    {
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

        public ObservableCollection<Notification> Notifications { get; set; } = new();
        public RelayCommand OpenNotificationCommand { get; set; }
        public RelayCommand DeleteNotificationCommand { get; set; }

        public NotificationViewModel(INavigationService navigationService)
        {
            Navigation = navigationService;
            Notifications.Add(new Notification { Title = "Nuevo mensaje", Sender = "Mauricito" });
            Notifications.Add(new Notification { Title = "Playlist compartida", Sender = "Echaparo" });

            OpenNotificationCommand = new RelayCommand(OnNotificationClick);
            DeleteNotificationCommand = new RelayCommand(OnDeleteNotificationClick);

        }

        private void OnNotificationClick(object parameter)
        {
            if (parameter is Notification notification)
            {
                MessageBox.Show($"Abriendo la notificación: {notification.Title}", "Abrir Notificación", MessageBoxButton.OK, MessageBoxImage.Information);

                NotificationWindowViewModel viewModel = new NotificationWindowViewModel(
                    App.ServiceProvider.GetRequiredService<INavigationService>(),
                    notification);

                var window = new NotificationWindow(viewModel);
                window.ShowDialog();
            }
        }

        private void OnDeleteNotificationClick(object parameter)
        {
            if (parameter is Notification notification)
            {
                MessageBoxResult result = MessageBox.Show($"¿Estás seguro de que deseas eliminar la notificación: {notification.Title}?", "Eliminar Notificación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    MessageBox.Show($"Notificación eliminada: {notification.Title}", "Eliminar Notificación", MessageBoxButton.OK, MessageBoxImage.Information);
                    Notifications.Remove(notification);
                }
            }
        }


    }
}
