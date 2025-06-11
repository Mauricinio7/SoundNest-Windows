using SoundNest_Windows_Client.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundNest_Windows_Client.Utilities
{
    public static class ToastHelper
    {
        private static readonly NotificationManager manager = new();

        public static void ShowToast(string message, NotificationType type = NotificationType.Information, string title = null)
        {
            manager.Show(new NotificationContent
            {
                Title = title ?? (type == NotificationType.Error ? "Error" : "Aviso"),
                Message = message,
                Type = type
            }, expirationTime: TimeSpan.FromSeconds(3));
        }
    }
}
