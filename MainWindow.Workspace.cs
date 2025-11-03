using System.Windows;
using System.Windows.Media;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop
{
    public partial class MainWindow
    {
        private void ResetWorkspaceState()
        {
            _workspaceResetService.ResetWorkspace();
            _arithmeticService.ClearArithmeticSnapshot();
            DisplayImage.Source = null;
            _state.PreviewItems.Clear();
            _currentZoom = 1.0;
            ImageScaleTransform.ScaleX = 1.0;
            ImageScaleTransform.ScaleY = 1.0;
            _currentArithmeticMode = ArithmeticToggleMode.None;
            _suppressArithmeticToggleHandlers = true;
            ArithmeticAddToggle.IsChecked = false;
            ArithmeticSubtractToggle.IsChecked = false;
            _suppressArithmeticToggleHandlers = false;
            UpdateUiForNoImage();
        }

        private void UpdateUiForNoImage()
        {
            FileNameText.Text = "Belum ada gambar";
            FileNameText.Foreground = Brushes.Gray;
            ImageInfoText.Text = "Silakan pilih gambar untuk mulai bekerja.";

            SavePixelsMenuItem.IsEnabled = false;
            NegationToggle.IsEnabled = false;
            NegationToggle.IsChecked = false;
            GrayscaleCompareButton.IsEnabled = false;
            BinaryThresholdToggle.IsEnabled = false;
            BinaryThresholdToggle.IsChecked = false;
            BinaryThresholdPanel.Visibility = Visibility.Collapsed;
            UpdateBinaryThresholdLabel(ImageWorkspaceState.DefaultBinaryThreshold);

            BrightnessPanel.Visibility = Visibility.Collapsed;
            _suppressBrightnessHandler = true;
            BrightnessSlider.Value = 0;
            _suppressBrightnessHandler = false;
            ColorSelectionPanel.Visibility = Visibility.Collapsed;
            ColorSelectionCheckBox.IsChecked = false;
            SelectedColorText.Text = "Klik pada gambar untuk memilih warna";
            SelectedColorText.Foreground = Brushes.Gray;
            DisplayImage.MouseLeftButtonDown -= DisplayImage_ColorSelection_Click;

            ArithmeticPanel.Visibility = Visibility.Collapsed;
            ArithmeticOffsetXTextBox.Text = "0";
            ArithmeticOffsetYTextBox.Text = "0";
            ArithmeticAddToggle.IsEnabled = false;
            ArithmeticSubtractToggle.IsEnabled = false;
            _suppressArithmeticToggleHandlers = true;
            ArithmeticAddToggle.IsChecked = false;
            ArithmeticSubtractToggle.IsChecked = false;
            _suppressArithmeticToggleHandlers = false;
            _currentArithmeticMode = ArithmeticToggleMode.None;
            ArithmeticInfoText.Foreground = Brushes.Gray;
            if (string.IsNullOrWhiteSpace(ArithmeticInfoText.Text))
            {
                ArithmeticInfoText.Text = "Belum ada gambar B";
            }
            UpdateArithmeticButtonsState();

            HideSidebar();
        }

        private void ShowSidebar()
        {
            SidebarColumn.Width = new GridLength(280);
            SidebarPanel.Visibility = Visibility.Visible;
        }

        private void HideSidebar()
        {
            SidebarColumn.Width = new GridLength(0);
            SidebarPanel.Visibility = Visibility.Collapsed;
        }
    }
}
