using Event;
using Microsoft.Extensions.Logging;
using Services.Communication.gRPC.Managers;
using SoundNest_Windows_Client.Notifications;
using SoundNest_Windows_Client.Notifications.ViewModels;
using SoundNest_Windows_Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SoundNest_Windows_Client.Utilities
{
    public interface INotificationsGrpc
    {
        public void init();
    }
    public class NotificationsGrpc : INotificationsGrpc
    {
        public static CancellationToken CancellationToken { get; set; } = new CancellationToken();

        IEventStreamManager _eventStreamManager;
        INotificationManager _notificationManager;
        ILogger _logger;
        private bool isInitialized = false;
        public NotificationsGrpc(IEventStreamManager eventStreamManager, INotificationManager notificationManager, ILogger<NotificationsGrpc> logger)
        {
            _eventStreamManager = eventStreamManager;
            _notificationManager = notificationManager;
            _logger = logger;

        }
        public void init()
        {
            if (isInitialized)
                return;
            isInitialized = true;
            _ = _eventStreamManager.StartAsync(CancellationToken);
            _eventStreamManager.OnEventReceived += HandleGrpcEvent;
        }

        private void HandleGrpcEvent(EventMessageReturn msg)
        {
            var content = new WelcomeViewModel(_notificationManager)
            {
                Title = "Custom notification.",
                Message = "Click on buttons!"
            };
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _notificationManager.Show(
                    content,
                    expirationTime: TimeSpan.FromSeconds(30));

                    _notificationManager.Show(new NotificationContent { Title = "Message", Message = "Message in window" }, areaName: "WindowArea");
                });
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mostrando notificación");
            }
        }


    }
}
