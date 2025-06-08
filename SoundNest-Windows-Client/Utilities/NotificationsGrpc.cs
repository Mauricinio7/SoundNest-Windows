using Event;
using Microsoft.Extensions.Logging;
using Services.Communication.gRPC.Managers;
using SoundNest_Windows_Client.Notifications;
using SoundNest_Windows_Client.Notifications.ViewModels;
using SoundNest_Windows_Client.Notifications.Views;
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
        private int _hasHandledFirstDataUpdate = 0;
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
            _eventStreamManager.OnDisconnected += (args) =>
            {
                _logger.LogDebug(args.Exception, "Stream error: {Msg}", args.Context);
                Interlocked.Exchange(ref _hasHandledFirstDataUpdate, 0);
                ShowDisconnectedNotification(args.Context);
            };
        }

        private void HandleGrpcEvent(EventMessageReturn msg)
        {
            if (msg.EventTypeRespose == EventType.DataUpdate)
            {
                if (Interlocked.Exchange(ref _hasHandledFirstDataUpdate, 1) == 0)
                {
                    ShowWelcomeNotification();
                }
                return;
            }
            switch (msg.EventTypeRespose)
            {
                case EventType.CommentReplyRecive:
                    ShowCommentReplyNotification(msg);
                    break;
                case EventType.SongVisitsNotification:
                    ShowViewsThresholdNotification(msg);
                    break;
                default:
                    _logger.LogDebug("Unknown event received: {EventType}", msg.EventTypeRespose);
                    break;
            }
        }

        private void ShowWelcomeNotification()
        {
            var content = new WelcomeViewModel(_notificationManager)
            {
                Title = "¡Bienvendio!",
                Message = "Que bueno verte."
            };

            Application.Current.Dispatcher.Invoke(() =>
            {
                _notificationManager.Show(
                    content,
                    expirationTime: TimeSpan.FromSeconds(30));

                _notificationManager.Show(
                    new NotificationContent
                    {
                        Title = "¡Bienvendio!",
                        Message = "Que bueno verte",
                        Type = NotificationType.Success
                    },
                    areaName: "WindowArea");
            });
        }

        private void ShowDisconnectedNotification(string context)
        {
            var errorContent = new DisconnectedViewModel(_notificationManager)
            {
                Title = "Error de conexion",
                Message = $"¿Reintentar?"
            };
            Action reconnectHandler = null;
            reconnectHandler = () =>
            {
                _eventStreamManager.ReconnectAsync();
                _logger.LogInformation("User ask for reconnect.");
                errorContent.OnReconnectRequested -= reconnectHandler;
            };
            errorContent.OnReconnectRequested += reconnectHandler;

            Action cancelHandler = null;
            cancelHandler = () =>
            {
                _logger.LogInformation("User canceled reconnect.");
                errorContent.OnCancelRequested -= cancelHandler;
            };
            errorContent.OnCancelRequested += cancelHandler;

            Application.Current.Dispatcher.Invoke(() =>
            {
                _notificationManager.Show(
                    errorContent,
                    expirationTime: TimeSpan.FromSeconds(60)
                );
            });
        }

        private void ShowCommentReplyNotification(EventMessageReturn msg)
        {
            var title = "Alguien respondio tu comentario";
            var message = msg.Message;
            var content = new CommentReplyViewModel()
            {
                Title = title,
                Message = message
            };

            Application.Current.Dispatcher.Invoke(() =>
            {
                _notificationManager.Show(
                    content,
                    expirationTime: TimeSpan.FromSeconds(30));

                _notificationManager.Show(
                    new NotificationContent
                    {
                        Title = title,
                        Message = message,
                        Type = NotificationType.Success
                    },
                    areaName: "WindowArea");
            });
        }

        private void ShowViewsThresholdNotification(EventMessageReturn msg)
        {
            var title = "Una cancion tuya llego a muchas vistas";
            var message = msg.Message;
            var content = new ViewsThresholdViewModel()
            {
                Title = title,
                Message = message
            };

            Application.Current.Dispatcher.Invoke(() =>
            {
                _notificationManager.Show(
                    content,
                    expirationTime: TimeSpan.FromSeconds(30));

                _notificationManager.Show(
                    new NotificationContent
                    {
                        Title = title,
                        Message = message,
                        Type = NotificationType.Success
                    },
                    areaName: "WindowArea");
            });
        }


    }
}
