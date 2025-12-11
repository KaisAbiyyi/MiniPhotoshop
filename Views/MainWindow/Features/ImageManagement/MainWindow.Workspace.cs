using System.Windows;
using System.Windows.Media;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop.Views.MainWindow
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
            _currentScalarMode = ScalarToggleMode.None;
            _suppressScalarToggleHandlers = true;
            // ScalarMultiplyToggle.IsChecked = false;
            // ScalarDivideToggle.IsChecked = false;
            _suppressScalarToggleHandlers = false;
            _currentRotationMode = RotationMode.None;
            _rotationService.ClearRotationSnapshot();
            _distortionService.ClearDistortionSnapshot();
            UpdateUiForNoImage();
        }

        private void UpdateUiForNoImage()
        {
            FileNameText.Text = "Belum ada gambar";
            FileNameText.Foreground = Brushes.Gray;
            ImageInfoText.Text = "Silakan pilih gambar untuk mulai bekerja.";

            SavePixelsMenuItem.IsEnabled = false;
            ResetImageMenuItem.IsEnabled = false;
            NegationToggle.IsEnabled = false;
            NegationToggle.IsChecked = false;
            NegationButton.IsEnabled = false;
            UpdateNegationButtonStyle(false);
            GrayscaleCompareButton.IsEnabled = false;
            BinaryThresholdToggle.IsEnabled = false;
            BinaryThresholdToggle.IsChecked = false;
            BinaryThresholdPanel.Visibility = Visibility.Collapsed;
            UpdateBinaryThresholdLabel(ImageWorkspaceState.DefaultBinaryThreshold);

            BrightnessPanel.Visibility = Visibility.Collapsed;
            _suppressBrightnessHandler = true;
            BrightnessSlider.Value = 0;
            _suppressBrightnessHandler = false;
            
            ColorSelectionToggle.IsEnabled = false;
            ColorSelectionToggle.IsChecked = false;
            ColorSelectionPanel.Visibility = Visibility.Collapsed;
            SelectedColorText.Text = "Klik pada gambar untuk memilih warna";
            SelectedColorText.Foreground = Brushes.Gray;
            DisplayImage.MouseLeftButtonDown -= DisplayImage_ColorSelection_Click;

            // ArithmeticPanel.Visibility = Visibility.Collapsed;
            // ArithmeticOffsetXTextBox.Text = "0";
            // ArithmeticOffsetYTextBox.Text = "0";
            ArithmeticAddToggle.IsEnabled = false;
            ArithmeticSubtractToggle.IsEnabled = false;
            _suppressArithmeticToggleHandlers = true;
            ArithmeticAddToggle.IsChecked = false;
            ArithmeticSubtractToggle.IsChecked = false;
            _suppressArithmeticToggleHandlers = false;
            _currentArithmeticMode = ArithmeticToggleMode.None;
            // // ArithmeticInfoText.Foreground = Brushes.Gray;
            // if (string.IsNullOrWhiteSpace(ArithmeticInfoText.Text))
            // {
            //     ArithmeticInfoText.Text = "Belum ada gambar B";
            // }
            // UpdateArithmeticButtonsState();

            // ScalarValueTextBox.Text = "2";
            // ScalarMultiplyToggle.IsEnabled = false;
            // ScalarDivideToggle.IsEnabled = false;
            _suppressScalarToggleHandlers = true;
            // ScalarMultiplyToggle.IsChecked = false;
            // ScalarDivideToggle.IsChecked = false;
            _suppressScalarToggleHandlers = false;
            _currentScalarMode = ScalarToggleMode.None;
            UpdateScalarButtonsState();

            RotateButton.IsEnabled = false;
            RotationPanel.Visibility = Visibility.Collapsed;
            _currentRotationMode = RotationMode.None;
            _rotationService.ClearRotationSnapshot();

            DistortionButton.IsEnabled = false;
            DistortionPanel.Visibility = Visibility.Collapsed;
            _distortionService.ClearDistortionSnapshot();

            ConvolutionMenu.IsEnabled = false;
            EdgeDetectionMenu.IsEnabled = false;

            // MoveImageToggle.IsEnabled = false;
            // MoveImageToggle.IsChecked = false;

            RectangleSelectToggle.IsEnabled = false;
            RectangleSelectToggle.IsChecked = false;
            LassoSelectToggle.IsEnabled = false;
            LassoSelectToggle.IsChecked = false;
            MoveSelectionToggle.IsEnabled = false;
            MoveSelectionToggle.IsChecked = false;
            ApplySelectionButton.IsEnabled = false;
            ClearSelectionButton.IsEnabled = false;

            HideSidebar();
        }

        private void ShowSidebar()
        {
            SidebarColumn.Width = new GridLength(320);
            SidebarPanel.Visibility = Visibility.Visible;
        }

        private void HideSidebar()
        {
            SidebarColumn.Width = new GridLength(0);
            SidebarPanel.Visibility = Visibility.Collapsed;
        }
    }
}
