using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MiniPhotoshop
{
    public partial class MainWindow
    {
        private bool _isDraggingFile = false;
        private Brush? _originalBorderBrush;
        private Brush? _originalBackground;

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0 && IsImageFile(files[0]))
                {
                    e.Effects = DragDropEffects.Copy;
                    _isDraggingFile = true;
                    ShowDragOverlay();
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0 && IsImageFile(files[0]))
                {
                    e.Effects = DragDropEffects.Copy;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void Window_DragLeave(object sender, DragEventArgs e)
        {
            _isDraggingFile = false;
            HideDragOverlay();
            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            _isDraggingFile = false;
            HideDragOverlay();

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    string filePath = files[0];
                    if (IsImageFile(filePath))
                    {
                        try
                        {
                            // Cek apakah sudah ada gambar A
                            if (_state.OriginalBitmap != null)
                            {
                                // Tampilkan dialog untuk memilih
                                var dialog = new ImageSelectionDialog
                                {
                                    Owner = this
                                };

                                if (dialog.ShowDialog() == true)
                                {
                                    if (dialog.SelectedTarget == ImageSelectionDialog.ImageTarget.ImageA)
                                    {
                                        // Ganti gambar A
                                        LoadImageFromFile(filePath);
                                    }
                                    else if (dialog.SelectedTarget == ImageSelectionDialog.ImageTarget.ImageB)
                                    {
                                        // Set sebagai gambar B
                                        LoadImageBFromFile(filePath);
                                    }
                                }
                            }
                            else
                            {
                                // Jika belum ada gambar A, langsung load sebagai gambar A
                                LoadImageFromFile(filePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                $"Gagal memuat gambar: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show(
                            "File yang dipilih bukan gambar.\n\nFormat yang didukung: .jpg, .jpeg, .png, .bmp, .gif, .tiff",
                            "Format Tidak Valid",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }
            }
            e.Handled = true;
        }

        private bool IsImageFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            string[] supportedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".tif" };
            
            return supportedExtensions.Contains(extension);
        }

        private void LoadImageFromFile(string filePath)
        {
            var result = _imageLoader.Load(filePath);
            ApplyLoadedImage(result);
        }

        private void LoadImageBFromFile(string filePath)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                // Set sebagai gambar B untuk operasi aritmatika dan biner
                _arithmeticOverlayBitmap = bitmap;
                _binaryOverlayBitmap = bitmap;

                // Deactivate mode jika sedang aktif
                if (_currentArithmeticMode != ArithmeticToggleMode.None)
                {
                    DeactivateArithmeticMode();
                }
                if (_currentBinaryMode != BinaryToggleMode.None && _currentBinaryMode != BinaryToggleMode.Not)
                {
                    DeactivateBinaryMode();
                }

                MessageBox.Show(
                    $"Gambar B berhasil dimuat:\n{Path.GetFileName(filePath)}\n({bitmap.PixelWidth} x {bitmap.PixelHeight})",
                    "Gambar B Dimuat",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat gambar B: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _arithmeticOverlayBitmap = null;
                _binaryOverlayBitmap = null;
            }
        }

        private void ShowDragOverlay()
        {
            // Visual feedback saat drag over
            if (DisplayImageBorder != null)
            {
                _originalBorderBrush = DisplayImageBorder.BorderBrush;
                _originalBackground = DisplayImageBorder.Background;
                
                DisplayImageBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(76, 139, 245));
                DisplayImageBorder.BorderThickness = new Thickness(3);
                DisplayImageBorder.Background = new SolidColorBrush(Color.FromArgb(20, 76, 139, 245));
            }
        }

        private void HideDragOverlay()
        {
            // Kembalikan tampilan normal
            if (DisplayImageBorder != null)
            {
                if (_originalBorderBrush != null)
                    DisplayImageBorder.BorderBrush = _originalBorderBrush;
                else
                    DisplayImageBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(223, 226, 229));
                
                DisplayImageBorder.BorderThickness = new Thickness(1);
                
                if (_originalBackground != null)
                    DisplayImageBorder.Background = _originalBackground;
                else
                    DisplayImageBorder.Background = Brushes.White;
            }
        }
    }
}
