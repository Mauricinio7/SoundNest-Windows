using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Notifications.Wpf;
using SoundNest_Windows_Client.Notifications.Controls;

namespace SoundNest_Windows_Client.Notifications
{
    public interface INotificationManager
    {
        /// <summary>
        /// Shows a notification with the specified content and optional callbacks.
        /// </summary>
        /// <param name="content">
        /// The object to display. Can be a <see cref="string"/>, a view‐model, or any type
        /// that your <see cref="NotificationTemplateSelector"/> knows how to render.
        /// </param>
        /// <param name="areaName">
        /// The name of the target notification area.  
        /// Use an empty string ("") to route to the default overlay window.
        /// </param>
        /// <param name="expirationTime">
        /// How long the notification stays visible before auto‐closing.  
        /// If <c>null</c>, defaults to 5 seconds.
        /// </param>
        /// <param name="onClick">
        /// Optional callback invoked when the user clicks the notification.
        /// </param>
        /// <param name="onClose">
        /// Optional callback invoked when the notification has finished closing.
        /// </param>
        void Show(object content, string areaName = "", TimeSpan? expirationTime = null, Action onClick = null, Action onClose = null);
    }
    public class NotificationManager : INotificationManager
    {
        private readonly Dispatcher _dispatcher;
        private static readonly List<NotificationArea> Areas = new List<NotificationArea>();
        private static NotificationsOverlayWindow _window;

        public NotificationManager(Dispatcher dispatcher = null)
        {
            if (dispatcher == null)
            {
                dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
            }

            _dispatcher = dispatcher;
        }

        /// <inheritdoc/>
        public void Show(object content, string areaName = "", TimeSpan? expirationTime = null, Action onClick = null,
            Action onClose = null)
        {
            if (!_dispatcher.CheckAccess() && !String.IsNullOrEmpty(areaName) )
            {
                _dispatcher.BeginInvoke(
                    new Action(() => Show(content, areaName, expirationTime, onClick, onClose)));
                return;
            }

            if (expirationTime == null) expirationTime = TimeSpan.FromSeconds(5);

            if (areaName == string.Empty && _window == null)
            {
                var workArea = SystemParameters.WorkArea;

                _window = new NotificationsOverlayWindow
                {
                    Left = workArea.Left,
                    Top = workArea.Top,
                    Width = workArea.Width,
                    Height = workArea.Height
                };

                _window.Show();
            }

            foreach (var area in Areas.Where(a => a.Name == areaName))
            {
                area.Show(content, (TimeSpan)expirationTime, onClick, onClose);
            }
        }

        internal static void AddArea(NotificationArea area)
        {
            Areas.Add(area);
        }
    }
}