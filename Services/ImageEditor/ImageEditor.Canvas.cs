using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MiniPhotoshop.Core.Enums;
using MiniPhotoshop.Core.Models;

namespace MiniPhotoshop.Services.ImageEditor
{
    /// <summary>
    /// Canvas management operations for ImageEditor.
    /// Single Responsibility: Only handles canvas-related operations.
    /// </summary>
    internal sealed partial class ImageEditor
    {
        private CanvasState _canvasState = new CanvasState();
        
        /// <summary>
        /// Stores the original image pixels separately for future drag/move feature.
        /// This preserves the original image data even when rendering on canvas.
        /// </summary>
        private byte[]? _originalImagePixels;
        private int _originalImageWidth;
        private int _originalImageHeight;

        /// <summary>
        /// Initializes a new canvas with specified dimensions and background color.
        /// </summary>
        public void InitializeCanvas(int width, int height, Color backgroundColor)
        {
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentException("Canvas dimensions must be positive.");
            }

            _canvasState = new CanvasState
            {
                Width = width,
                Height = height,
                BackgroundColor = backgroundColor,
                IsInitialized = true
            };
        }

        /// <summary>
        /// Updates the canvas size while preserving existing content.
        /// </summary>
        public void UpdateCanvasSize(int newWidth, int newHeight)
        {
            if (newWidth <= 0 || newHeight <= 0)
            {
                throw new ArgumentException("Canvas dimensions must be positive.");
            }

            _canvasState.Width = newWidth;
            _canvasState.Height = newHeight;
        }

        /// <summary>
        /// Updates the canvas background color.
        /// </summary>
        public void UpdateCanvasBackground(Color newColor)
        {
            _canvasState.BackgroundColor = newColor;
        }
        
        /// <summary>
        /// Sets the image offset position on canvas (for drag feature).
        /// </summary>
        public void SetImageOffset(int offsetX, int offsetY)
        {
            _canvasState.ImageOffsetX = offsetX;
            _canvasState.ImageOffsetY = offsetY;
        }

        /// <summary>
        /// Renders the current canvas with any placed images.
        /// Images are clipped to canvas bounds and placed at the offset position.
        /// Original image pixels are preserved for future drag feature.
        /// </summary>
        public BitmapSource? RenderCanvas()
        {
            if (!_canvasState.IsInitialized)
            {
                return null;
            }

            int canvasWidth = _canvasState.Width;
            int canvasHeight = _canvasState.Height;
            Color backgroundColor = _canvasState.BackgroundColor;
            int offsetX = _canvasState.ImageOffsetX;
            int offsetY = _canvasState.ImageOffsetY;

            // Create canvas buffer
            int stride = canvasWidth * 4;
            byte[] buffer = new byte[stride * canvasHeight];

            byte bgB = backgroundColor.B;
            byte bgG = backgroundColor.G;
            byte bgR = backgroundColor.R;
            byte bgA = backgroundColor.A;

            // Fill with background color
            for (int i = 0; i < buffer.Length; i += 4)
            {
                buffer[i] = bgB;
                buffer[i + 1] = bgG;
                buffer[i + 2] = bgR;
                buffer[i + 3] = bgA;
            }

            // If there's an image loaded, place it at offset position
            if (State.OriginalBitmap != null)
            {
                BitmapSource image = EnsureBgra32(State.OriginalBitmap);
                int imageWidth = image.PixelWidth;
                int imageHeight = image.PixelHeight;

                // Store original image pixels for future drag feature
                int imageStride = imageWidth * 4;
                if (_originalImagePixels == null || 
                    _originalImageWidth != imageWidth || 
                    _originalImageHeight != imageHeight)
                {
                    _originalImagePixels = new byte[imageStride * imageHeight];
                    image.CopyPixels(_originalImagePixels, imageStride, 0);
                    _originalImageWidth = imageWidth;
                    _originalImageHeight = imageHeight;
                }

                // Calculate visible region of image on canvas
                // Only copy pixels that fall within canvas bounds
                int srcStartX = Math.Max(0, -offsetX);
                int srcStartY = Math.Max(0, -offsetY);
                int dstStartX = Math.Max(0, offsetX);
                int dstStartY = Math.Max(0, offsetY);
                
                int copyWidth = Math.Min(imageWidth - srcStartX, canvasWidth - dstStartX);
                int copyHeight = Math.Min(imageHeight - srcStartY, canvasHeight - dstStartY);

                if (copyWidth > 0 && copyHeight > 0)
                {
                    for (int y = 0; y < copyHeight; y++)
                    {
                        int srcY = srcStartY + y;
                        int dstY = dstStartY + y;
                        
                        for (int x = 0; x < copyWidth; x++)
                        {
                            int srcX = srcStartX + x;
                            int dstX = dstStartX + x;
                            
                            int srcIndex = srcY * imageStride + srcX * 4;
                            int dstIndex = dstY * stride + dstX * 4;

                            // Copy BGRA values from image to canvas
                            buffer[dstIndex] = _originalImagePixels[srcIndex];         // B
                            buffer[dstIndex + 1] = _originalImagePixels[srcIndex + 1]; // G
                            buffer[dstIndex + 2] = _originalImagePixels[srcIndex + 2]; // R
                            buffer[dstIndex + 3] = _originalImagePixels[srcIndex + 3]; // A
                        }
                    }
                }
            }

            return CreateBitmapFromBuffer(buffer, canvasWidth, canvasHeight);
        }

        /// <summary>
        /// Places an image onto the canvas at the current offset position.
        /// </summary>
        public BitmapSource PlaceImageOnCanvas(BitmapSource image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (!_canvasState.IsInitialized)
            {
                // Initialize canvas with image dimensions if not initialized
                _canvasState.Width = image.PixelWidth;
                _canvasState.Height = image.PixelHeight;
                _canvasState.IsInitialized = true;
            }

            // Store original bitmap in state
            State.OriginalBitmap = image;
            
            // Clear cached original pixels to force refresh
            _originalImagePixels = null;

            return RenderCanvas()!;
        }

        /// <summary>
        /// Clears cached image pixels to force refresh from State.OriginalBitmap.
        /// Call this after modifying the original bitmap (e.g., after convolution).
        /// </summary>
        public void RefreshImagePixels()
        {
            _originalImagePixels = null;
        }

        /// <summary>
        /// Gets the current canvas dimensions.
        /// </summary>
        public (int Width, int Height) GetCanvasDimensions()
        {
            return (_canvasState.Width, _canvasState.Height);
        }
        
        /// <summary>
        /// Gets the original image dimensions (before clipping to canvas).
        /// </summary>
        public (int Width, int Height) GetOriginalImageDimensions()
        {
            return (_originalImageWidth, _originalImageHeight);
        }
        
        /// <summary>
        /// Gets the current image offset on canvas.
        /// </summary>
        public (int X, int Y) GetImageOffset()
        {
            return (_canvasState.ImageOffsetX, _canvasState.ImageOffsetY);
        }
    }
}
