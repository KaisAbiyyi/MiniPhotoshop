using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MiniPhotoshop.Views.MainWindow
{
    public partial class MainWindow
    {
        private void WorkspaceScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_state.OriginalBitmap == null)
            {
                return;
            }

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                e.Handled = true;
                double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
                Point focus = e.GetPosition(WorkspaceScrollViewer);
                ZoomImage(zoomFactor, focus);
            }
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                double newOffset = WorkspaceScrollViewer.HorizontalOffset - e.Delta;
                newOffset = Math.Clamp(newOffset, 0, WorkspaceScrollViewer.ScrollableWidth);
                WorkspaceScrollViewer.ScrollToHorizontalOffset(newOffset);
            }
        }

        private void ZoomImage(double zoomFactor, Point focusPoint)
        {
            double targetZoom = Math.Clamp(_currentZoom * zoomFactor, MinZoom, MaxZoom);
            zoomFactor = targetZoom / _currentZoom;

            if (Math.Abs(zoomFactor - 1.0) < 0.0001)
            {
                return;
            }

            double absoluteX = WorkspaceScrollViewer.HorizontalOffset + focusPoint.X;
            double absoluteY = WorkspaceScrollViewer.VerticalOffset + focusPoint.Y;

            _currentZoom = targetZoom;
            _state.CurrentZoom = targetZoom;
            ImageScaleTransform.ScaleX = targetZoom;
            ImageScaleTransform.ScaleY = targetZoom;

            WorkspaceScrollViewer.UpdateLayout();

            WorkspaceScrollViewer.ScrollToHorizontalOffset(Math.Clamp(absoluteX * zoomFactor - focusPoint.X, 0, WorkspaceScrollViewer.ScrollableWidth));
            WorkspaceScrollViewer.ScrollToVerticalOffset(Math.Clamp(absoluteY * zoomFactor - focusPoint.Y, 0, WorkspaceScrollViewer.ScrollableHeight));
        }

        private void DisplayImage_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            if (_state.OriginalBitmap == null)
            {
                return;
            }

            e.ManipulationContainer = WorkspaceScrollViewer;
            e.Mode = ManipulationModes.Scale;
            e.Handled = true;
        }

        private void DisplayImage_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (_state.OriginalBitmap == null)
            {
                return;
            }

            if (e.DeltaManipulation.Scale.X != 0 && !double.IsNaN(e.DeltaManipulation.Scale.X))
            {
                Point focus = e.ManipulationOrigin;
                ZoomImage(e.DeltaManipulation.Scale.X, focus);
            }

            Vector translation = e.DeltaManipulation.Translation;
            if (!double.IsNaN(translation.X) && !double.IsInfinity(translation.X) &&
                !double.IsNaN(translation.Y) && !double.IsInfinity(translation.Y))
            {
                double targetHorizontal = WorkspaceScrollViewer.HorizontalOffset - translation.X;
                double targetVertical = WorkspaceScrollViewer.VerticalOffset - translation.Y;

                targetHorizontal = Math.Clamp(targetHorizontal, 0, WorkspaceScrollViewer.ScrollableWidth);
                targetVertical = Math.Clamp(targetVertical, 0, WorkspaceScrollViewer.ScrollableHeight);

                WorkspaceScrollViewer.ScrollToHorizontalOffset(targetHorizontal);
                WorkspaceScrollViewer.ScrollToVerticalOffset(targetVertical);
            }

            e.Handled = true;
        }

        private void WorkspaceScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_state.PendingAutoFit)
            {
                ResetZoomToFit();
            }
        }

        private void ResetZoomToFit()
        {
            if (_state.OriginalBitmap == null)
            {
                return;
            }

            double viewportWidth = WorkspaceScrollViewer.ViewportWidth;
            double viewportHeight = WorkspaceScrollViewer.ViewportHeight;

            if (viewportWidth <= 0 || viewportHeight <= 0)
            {
                return;
            }

            double scaleX = viewportWidth / _state.CachedWidth;
            double scaleY = viewportHeight / _state.CachedHeight;
            double targetZoom = Math.Min(scaleX, scaleY);
            targetZoom = Math.Clamp(targetZoom, MinZoom, MaxZoom);

            _currentZoom = targetZoom;
            _state.CurrentZoom = targetZoom;
            ImageScaleTransform.ScaleX = targetZoom;
            ImageScaleTransform.ScaleY = targetZoom;

            WorkspaceScrollViewer.UpdateLayout();
            WorkspaceScrollViewer.ScrollToHorizontalOffset((WorkspaceScrollViewer.ExtentWidth - viewportWidth) / 2);
            WorkspaceScrollViewer.ScrollToVerticalOffset((WorkspaceScrollViewer.ExtentHeight - viewportHeight) / 2);

            _state.PendingAutoFit = false;
        }

        private void QueueAutoFit()
        {
            _state.PendingAutoFit = true;
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(ResetZoomToFit));
        }
    }
}

