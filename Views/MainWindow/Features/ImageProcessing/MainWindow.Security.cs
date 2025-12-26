using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop.Views.MainWindow
{
    /// <summary>
    /// Security features handlers (steganography and watermark).
    /// </summary>
    public partial class MainWindow
    {
        private void SteganographyEmbed_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int capacity = _steganographyService.GetMaxMessageLengthBytes();
                if (capacity <= 0)
                {
                    MessageBox.Show("Gambar terlalu kecil untuk menyisipkan pesan.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dialog = new Dialogs.SteganographyEmbedDialog(capacity)
                {
                    Owner = this
                };

                if (dialog.ShowDialog() == true && dialog.WasApplied)
                {
                    BitmapSource result = _steganographyService.EmbedMessage(dialog.Message);
                    ApplyImageProcessingResult(result, "Steganografi LSB");
                    MessageBox.Show("Pesan berhasil disisipkan.", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menyisipkan pesan: {ex.Message}", "Error Steganografi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SteganographyExtract_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string message = _steganographyService.ExtractMessage();
                var dialog = new Dialogs.SteganographyExtractDialog(message)
                {
                    Owner = this
                };
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal mengekstrak pesan: {ex.Message}", "Error Steganografi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WatermarkText_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Dialogs.WatermarkTextDialog
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true && dialog.WasApplied)
            {
                try
                {
                    BitmapSource result = _watermarkService.ApplyTextWatermark(
                        dialog.WatermarkText,
                        dialog.WatermarkOpacity,
                        dialog.WatermarkFontSize,
                        dialog.Position);
                    ApplyImageProcessingResult(result, "Watermark Teks");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gagal menerapkan watermark teks: {ex.Message}", "Error Watermark", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void WatermarkLogo_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Dialogs.WatermarkLogoDialog
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true && dialog.WasApplied)
            {
                try
                {
                    if (dialog.WatermarkImage == null)
                    {
                        MessageBox.Show("Logo belum dipilih.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    BitmapSource result = _watermarkService.ApplyImageWatermark(
                        dialog.WatermarkImage,
                        dialog.WatermarkOpacity,
                        dialog.Scale,
                        dialog.Position);
                    ApplyImageProcessingResult(result, "Watermark Logo");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gagal menerapkan watermark logo: {ex.Message}", "Error Watermark", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void WatermarkExtract_Click(object sender, RoutedEventArgs e)
        {
            var info = _watermarkService.GetLastWatermarkInfo();
            if (info == null)
            {
                MessageBox.Show("Belum ada watermark yang diterapkan.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new Dialogs.WatermarkExtractDialog(info)
            {
                Owner = this
            };
            dialog.ShowDialog();
        }
    }
}
