using System.Windows;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop.Views.MainWindow
{
    public partial class MainWindow
    {
        /// <summary>
        /// Updates the state of toolbar buttons based on whether an image is selected.
        /// </summary>
        private void UpdateToolbarState()
        {
            var selectedImage = _imageObjectManager.GetSelectedImage();
            bool hasSelection = selectedImage != null;

            // Enable/Disable features based on selection
            BrightnessToggle.IsEnabled = hasSelection;
            BinaryThresholdToggle.IsEnabled = hasSelection;
            GrayscaleCompareButton.IsEnabled = hasSelection;
            
            // Arithmetic operations might require at least one image, or specific logic
            ArithmeticAddToggle.IsEnabled = _state.ImageObjects.Count > 0;
            ArithmeticSubtractToggle.IsEnabled = _state.ImageObjects.Count > 0;
            
            // Binary operations
            BinaryAndToggle.IsEnabled = hasSelection;
            BinaryOrToggle.IsEnabled = hasSelection;
            BinaryNotToggle.IsEnabled = hasSelection;
            BinaryXorToggle.IsEnabled = hasSelection;
            
            ColorSelectionToggle.IsEnabled = hasSelection;
            RotateButton.IsEnabled = hasSelection;
            DistortionButton.IsEnabled = hasSelection;
            // Convolution operates on the global OriginalBitmap, not the specific selection
            ConvolutionMenu.IsEnabled = _state.OriginalBitmap != null;
            EdgeDetectionMenu.IsEnabled = _state.OriginalBitmap != null;
            ContrastMenu.IsEnabled = _state.OriginalBitmap != null;
            SecurityMenu.IsEnabled = _state.OriginalBitmap != null;
            NegationButton.IsEnabled = hasSelection;
            
            // Reset Image menu item
            ResetImageMenuItem.IsEnabled = hasSelection;
            SavePixelsMenuItem.IsEnabled = hasSelection;
            SaveImageAsMenuItem.IsEnabled = hasSelection;

            // Update status text
            if (hasSelection)
            {
                ImageInfoText.Text = $"Selected: {selectedImage.Name} ({selectedImage.Width}x{selectedImage.Height})";
            }
            else
            {
                ImageInfoText.Text = "No image selected.";
            }
        }
    }
}
