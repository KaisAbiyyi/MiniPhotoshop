using System;
using System.Windows;

namespace MiniPhotoshop.Views.MainWindow
{
    public partial class MainWindow
    {
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_MOUSEHWHEEL && WorkspaceScrollViewer.IsLoaded)
            {
                if (_hwndSource?.CompositionTarget != null)
                {
                    Point devicePoint = new((short)((long)lParam & 0xFFFF), (short)(((long)lParam >> 16) & 0xFFFF));
                    Point targetPoint = _hwndSource.CompositionTarget.TransformFromDevice.Transform(devicePoint);

                    if (WorkspaceScrollViewer.IsMouseOver && WorkspaceScrollViewer.InputHitTest(targetPoint) != null)
                    {
                        int delta = (short)((long)wParam >> 16);
                        double newOffset = WorkspaceScrollViewer.HorizontalOffset - Math.Sign(delta) * HorizontalWheelStep;
                        newOffset = Math.Clamp(newOffset, 0, WorkspaceScrollViewer.ScrollableWidth);
                        WorkspaceScrollViewer.ScrollToHorizontalOffset(newOffset);
                        handled = true;
                    }
                }
            }

            return IntPtr.Zero;
        }
    }
}

