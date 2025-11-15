using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MiniPhotoshop
{
    public partial class MainWindow
    {
        // New toggle handlers
        private void ColorSelectionToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (_state.PixelCache == null)
            {
                MessageBox.Show("Tidak ada gambar yang dimuat.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                ColorSelectionToggle.IsChecked = false;
                return;
            }

            try
            {
                ColorSelectionPanel.Visibility = Visibility.Visible;
                DisplayImage.Source = _colorSelectionService.SetColorSelectionActive(true);
                DisplayImage.MouseLeftButtonDown += DisplayImage_ColorSelection_Click;
                SelectedColorText.Text = "Klik pada gambar untuk memilih warna";
                SelectedColorText.Foreground = Brushes.Blue;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal mengaktifkan seleksi warna: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ColorSelectionToggle.IsChecked = false;
            }
        }

        private void ColorSelectionToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            ColorSelectionPanel.Visibility = Visibility.Collapsed;
            DisplayImage.MouseLeftButtonDown -= DisplayImage_ColorSelection_Click;
            try
            {
                DisplayImage.Source = _colorSelectionService.SetColorSelectionActive(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menonaktifkan seleksi warna: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            SelectedColorText.Text = "Klik pada gambar untuk memilih warna";
            SelectedColorText.Foreground = Brushes.Gray;
        }

        // Legacy checkbox handlers (kept for backward compatibility if needed)
        private void ColorSelection_Checked(object sender, RoutedEventArgs e)
        {
            if (_state.PixelCache == null)
            {
                MessageBox.Show("Tidak ada gambar yang dimuat.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                DisplayImage.Source = _colorSelectionService.SetColorSelectionActive(true);
                DisplayImage.MouseLeftButtonDown += DisplayImage_ColorSelection_Click;
                SelectedColorText.Text = "Klik pada gambar untuk memilih warna";
                SelectedColorText.Foreground = Brushes.Blue;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal mengaktifkan seleksi warna: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ColorSelection_Unchecked(object sender, RoutedEventArgs e)
        {
            DisplayImage.MouseLeftButtonDown -= DisplayImage_ColorSelection_Click;
            try
            {
                DisplayImage.Source = _colorSelectionService.SetColorSelectionActive(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menonaktifkan seleksi warna: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            SelectedColorText.Text = "Klik pada gambar untuk memilih warna";
            SelectedColorText.Foreground = Brushes.Gray;
        }

        private void DisplayImage_ColorSelection_Click(object sender, MouseButtonEventArgs e)
        {
            if (_state.PixelCache == null)
            {
                return;
            }

            try
            {
                Point clickPoint = e.GetPosition(DisplayImage);
                double scaleX = _state.CachedWidth / DisplayImage.ActualWidth;
                double scaleY = _state.CachedHeight / DisplayImage.ActualHeight;

                int pixelX = (int)Math.Clamp(clickPoint.X * scaleX, 0, _state.CachedWidth - 1);
                int pixelY = (int)Math.Clamp(clickPoint.Y * scaleY, 0, _state.CachedHeight - 1);

                DisplayImage.Source = _colorSelectionService.ApplySelection(pixelX, pixelY);

                byte r = _state.ColorSelection.TargetR;
                byte g = _state.ColorSelection.TargetG;
                byte b = _state.ColorSelection.TargetB;
                SelectedColorText.Text = $"RGB({r}, {g}, {b})";
                SelectedColorText.Foreground = new SolidColorBrush(Color.FromRgb(r, g, b));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memilih warna: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

