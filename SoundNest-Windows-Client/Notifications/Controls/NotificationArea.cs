using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace SoundNest_Windows_Client.Notifications.Controls
{
    public class NotificationArea : Control
    {

        public NotificationPosition Position
        {
            get { return (NotificationPosition)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(NotificationPosition), typeof(NotificationArea), new PropertyMetadata(NotificationPosition.BottomRight));


        public int MaxItems
        {
            get { return (int)GetValue(MaxItemsProperty); }
            set { SetValue(MaxItemsProperty, value); }
        }

        public static readonly DependencyProperty MaxItemsProperty =
            DependencyProperty.Register("MaxItems", typeof(int), typeof(NotificationArea), new PropertyMetadata(int.MaxValue));

        private IList _items;

        public NotificationArea()
        {
            NotificationManager.AddArea(this);
        }
        
        static NotificationArea()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NotificationArea),
                new FrameworkPropertyMetadata(typeof(NotificationArea)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var itemsControl = GetTemplateChild("PART_Items") as Panel;
            _items = itemsControl?.Children;
        }

        /// <summary>
        /// Creates and displays a new notification with the specified content,
        /// lifetime, and click/close callbacks.
        /// </summary>
        /// <param name="content">
        /// The content to display in the notification. Can be a <see cref="string"/>,
        /// a <see cref="NotificationContent"/> instance, or any other object
        /// that your <see cref="NotificationTemplateSelector"/> knows how to render.
        /// </param>
        /// <param name="expirationTime">
        /// The time to wait before automatically closing the notification.
        /// Use <see cref="TimeSpan.MaxValue"/> to disable auto‐close.
        /// </param>
        /// <param name="onClick">
        /// An optional callback invoked when the user clicks the notification.
        /// </param>
        /// <param name="onClose">
        /// An optional callback invoked when the notification has finished closing.
        /// </param>
        public async void Show(object content, TimeSpan expirationTime, Action onClick, Action onClose)

        {
            var notification = new Notification
            {
                Content = content
            };

            notification.MouseLeftButtonDown += (sender, args) =>
            {
                if (onClick != null)
                {
                    onClick.Invoke();
                    (sender as Notification)?.Close();
                }
            };
            notification.NotificationClosed += (sender, args) => onClose?.Invoke();
            notification.NotificationClosed += OnNotificationClosed;

            if (!IsLoaded)
            {
                return;
            }

            var w = Window.GetWindow(this);
            var x = PresentationSource.FromVisual(w);
            if (x == null)
            {
                return;
            }

            lock (_items)
            {
                _items.Add(notification);

                if (_items.OfType<Notification>().Count(i => !i.IsClosing) > MaxItems)
                {
                    _items.OfType<Notification>()
                          .First(i => !i.IsClosing)
                          .Close();
                }
            }
            if (expirationTime == TimeSpan.MaxValue)
            {
                return;
            }
            await Task.Delay(expirationTime);

            notification.Close();
        }

        private void OnNotificationClosed(object sender, RoutedEventArgs routedEventArgs)
        {
            var notification = sender as Notification;
            _items.Remove(notification);
        }
    }

    public enum NotificationPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}