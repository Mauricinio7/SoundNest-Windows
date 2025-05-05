using Microsoft.Extensions.DependencyInjection;
using SoundNest_Windows_Client.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SoundNest_Windows_Client.Views
{
    public partial class MusicPlayerBar : UserControl
    {
        private bool isDraggingThumb = false;
        private bool isDraggingBar = false;
        private const double barLeft = 475;
        private const double barWidth = 483;

        private MusicPlayerBarViewModel vm => DataContext as MusicPlayerBarViewModel;

        public MusicPlayerBar()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<MusicPlayerBarViewModel>();
            Loaded += (_, _) => CompositionTarget.Rendering += (_, _) => { if (!isDraggingThumb) UpdateThumb(); };
            this.Unloaded += MusicPlayerBar_Unloaded;
        }

        private void MusicPlayerBar_Unloaded(object sender, RoutedEventArgs e)
        {
            vm?.Cleanup();
        }

        private void UpdateThumb()
        {
            if (vm?.MaxProgress <= 0) return;
            double percent = vm.Progress / vm.MaxProgress;
            double x = percent * barWidth;
            Canvas.SetLeft(ProgressThumb, barLeft + x - ProgressThumb.Width / 2);
        }

        private void SetProgressFromPosition(Point pos)
        {
            if (vm == null) return;
            double percent = Math.Clamp(pos.X / barWidth, 0, 1);
            vm.SetProgress(percent * vm.MaxProgress);
            UpdateThumb();
        }

        private void DragBarOverlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDraggingBar = true;
            SetProgressFromPosition(e.GetPosition(DragBarOverlay));
            DragBarOverlay.CaptureMouse();
        }

        private void DragBarOverlay_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingBar) SetProgressFromPosition(e.GetPosition(DragBarOverlay));
        }

        private void DragBarOverlay_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDraggingBar)
            {
                SetProgressFromPosition(e.GetPosition(DragBarOverlay));
                DragBarOverlay.ReleaseMouseCapture();
                isDraggingBar = false;
            }
        }

        private void ProgressThumb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDraggingThumb = true;
            ProgressThumb.CaptureMouse();
        }

        private void ProgressThumb_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingThumb)
            {
                var pos = e.GetPosition(DragBarOverlay);
                double clampedX = Math.Clamp(pos.X, 0, barWidth);
                double percent = clampedX / barWidth;
                vm.Progress = percent * vm.MaxProgress;
                Canvas.SetLeft(ProgressThumb, barLeft + clampedX - ProgressThumb.Width / 2);
            }
        }

        private void ProgressThumb_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDraggingThumb)
            {
                SetProgressFromPosition(e.GetPosition(DragBarOverlay));
                ProgressThumb.ReleaseMouseCapture();
                isDraggingThumb = false;
            }
        }
    }
}