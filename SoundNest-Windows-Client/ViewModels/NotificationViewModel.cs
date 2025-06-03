using Services.Infrestructure;
using Services.Navigation;
using SoundNest_Windows_Client.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SoundNest_Windows_Client.Views;
using Services.Communication.RESTful.Services;
using SoundNest_Windows_Client.Models;
using Services.Communication.RESTful.Models.Notification;

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

        private readonly INotificationService notificationService;
        private readonly Account currentUser;

        public ObservableCollection<Notification> Notifications { get; set; } = new();

        public RelayCommand OpenNotificationCommand { get; set; }
        public AsyncRelayCommand DeleteNotificationCommand { get; set; }

        public NotificationViewModel(INavigationService navigationService, INotificationService notificationService, IAccountService accountService)
        {
            Navigation = navigationService;
            this.notificationService = notificationService;
            currentUser = accountService.CurrentUser;

            OpenNotificationCommand = new RelayCommand(OnNotificationClick);
            DeleteNotificationCommand = new AsyncRelayCommand(async (param) => await OnDeleteNotificationClick(param));

            _ = LoadNotifications(); 
        }

        private async Task LoadNotifications()
        {
                var result = await ExecuteRESTfulApiCall(() => notificationService.GetNotificationsByUserIdAsync(currentUser.Id.ToString()));


                if (result.IsSuccess && result.Data is not null)
                {
                    Notifications.Clear();

                    foreach (NotificationResponse? notification in result.Data)
                    {
                        Notifications.Add(new Notification(notification.Title, notification.Sender, notification.Notification, notification.Relevance.Value, notification.Id));
                    }
                }
                else
                {
                    MessageBox.Show(result.Message ?? "Error al cargar las notificaciones", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
        }

        private void OnNotificationClick(object parameter)
        {
            if (parameter is Notification notification)
            {
                NotificationWindowViewModel viewModel = new NotificationWindowViewModel(
                    App.ServiceProvider.GetRequiredService<INavigationService>(), notification
                );

                var window = new NotificationWindow(viewModel);
                window.ShowDialog();
            }
        }

        private async Task OnDeleteNotificationClick(object parameter)
        {
            if (parameter is Notification notification)
            {
                MessageBoxResult resultMessage = MessageBox.Show(
                    $"¿Estás seguro de que deseas eliminar la notificación: {notification.Title}?",
                    "Eliminar Notificación", MessageBoxButton.YesNo, MessageBoxImage.Warning
                );

                if (resultMessage != MessageBoxResult.Yes)
                    return;

                    var resultDelete = await ExecuteRESTfulApiCall(() => notificationService.DeleteNotificationAsync(notification.Id));

                    if (resultDelete.IsSuccess)
                    {
                        MessageBox.Show("Notificación eliminada exitosamente", "Eliminar Notificación", MessageBoxButton.OK, MessageBoxImage.Information);
                        Notifications.Remove(notification);
                    }
                    else
                    {
                        MessageBox.Show(resultDelete.Message ?? "Error al eliminar la notificación", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
            }
        }
    }
}
