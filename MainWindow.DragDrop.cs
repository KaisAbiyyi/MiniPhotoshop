using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

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
                            LoadImageFromFile(filePath);
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
