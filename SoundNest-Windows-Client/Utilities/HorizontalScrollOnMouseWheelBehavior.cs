using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace SoundNest_Windows_Client.Utilities
{
    public class HorizontalScrollOnMouseWheelBehavior : Behavior<ScrollViewer>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseWheel += OnPreviewMouseWheel;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewMouseWheel -= OnPreviewMouseWheel;
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                double offsetChange = e.Delta > 0 ? -100 : 100;
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + offsetChange);
                e.Handled = true;
            }
        }
    }

}
