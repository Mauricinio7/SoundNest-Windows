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
using Services.Communication.RESTful.Models;
using SoundNest_Windows_Client.Notifications;
using System.Net;
using SoundNest_Windows_Client.Resources.Controls;

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
        public bool HasNotifications => Notifications.Count > 0;

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
                    RefreshNotificationState();
                }
                else
                {
                ShowNotificationLoadError(result);

            }
        }

        private void ShowNotificationLoadError(ApiResult<List<NotificationResponse>> result)
        {
            string title = "Error al cargar notificaciones";

            string message = result.StatusCode switch
            {
                HttpStatusCode.BadRequest => "Ocurrió un error al procesar la solicitud de tus notificaciones. Inténtalo más tarde",
                HttpStatusCode.Unauthorized => "Tu sesión ha expirado. Inicia sesión nuevamente.",
                HttpStatusCode.Forbidden => "Tu sesión ha expirado. Inicia sesión nuevamente.",
                HttpStatusCode.InternalServerError => "Ocurrió un error inesperado al obtener tus notificaciones. Inténtalo más tarde.",
                _ => "Parece que no hay conexión a internet, inténtalo más tarde"
            };

            ToastHelper.ShowToast(message, NotificationType.Warning, title);
        }


        private void RefreshNotificationState()
        {
            OnPropertyChanged(nameof(HasNotifications));
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
                bool resultMessage = DialogHelper.ShowConfirmation("Eliminar Notificación", $"¿Estás seguro de que deseas eliminar la notificación: {notification.Title}?");


                if (!resultMessage)
                    return;

                ApiResult<bool> resultDelete = await ExecuteRESTfulApiCall(() => notificationService.DeleteNotificationAsync(notification.Id));

                    if (resultDelete.IsSuccess)
                    {
                    ToastHelper.ShowToast("Notificación eliminada correctamente", NotificationType.Success, "Éxito");
                    Notifications.Remove(notification);
                        RefreshNotificationState();
                    }
                    else
                    {
                        ShowNotificationDeleteError(resultDelete);
                    }
            }
        }

        private void ShowNotificationDeleteError(ApiResult<bool> result)
        {
            string title = "Error al eliminar notificación";

            string message = result.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Tu sesión ha expirado. Inicia sesión nuevamente.",
                HttpStatusCode.Forbidden => "Tu sesión ha expirado. Inicia sesión nuevamente.",
                HttpStatusCode.NotFound => "La notificación que intentas eliminar no fue encontrada. Inetente con otra",
                HttpStatusCode.InternalServerError => "Ocurrió un error inesperado al eliminar la notificación. Inténtalo más tarde.",
                _ => "Parece que no hay conexión a internet, inténtalo más tarde"
            };

            DialogHelper.ShowAcceptDialog(title, message, AcceptDialogType.Warning);
        }

    }
}
