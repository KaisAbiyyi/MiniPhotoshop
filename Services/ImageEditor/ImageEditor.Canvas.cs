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
        /// Renders the current canvas with any placed images.
        /// </summary>
        public BitmapSource? RenderCanvas()
        {
            if (!_canvasState.IsInitialized)
            {
                return null;
            }

            int width = _canvasState.Width;
            int height = _canvasState.Height;
            Color backgroundColor = _canvasState.BackgroundColor;

            // Create canvas buffer
            int stride = width * 4;
            byte[] buffer = new byte[stride * height];

            byte b = backgroundColor.B;
            byte g = backgroundColor.G;
            byte r = backgroundColor.R;
            byte a = backgroundColor.A;

            // Fill with background color
            for (int i = 0; i < buffer.Length; i += 4)
            {
                buffer[i] = b;
                buffer[i + 1] = g;
                buffer[i + 2] = r;
                buffer[i + 3] = a;
            }

            // If there's an image loaded, place it at (0,0)
            if (State.OriginalBitmap != null)
            {
                BitmapSource image = EnsureBgra32(State.OriginalBitmap);
                int imageWidth = image.PixelWidth;
                int imageHeight = image.PixelHeight;

                int imageStride = imageWidth * 4;
                byte[] imageBuffer = new byte[imageStride * imageHeight];
                image.CopyPixels(imageBuffer, imageStride, 0);

                int copyWidth = Math.Min(imageWidth, width);
                int copyHeight = Math.Min(imageHeight, height);

                for (int y = 0; y < copyHeight; y++)
                {
                    for (int x = 0; x < copyWidth; x++)
                    {
                        int srcIndex = y * imageStride + x * 4;
                        int dstIndex = y * stride + x * 4;

                        buffer[dstIndex] = imageBuffer[srcIndex];
                        buffer[dstIndex + 1] = imageBuffer[srcIndex + 1];
                        buffer[dstIndex + 2] = imageBuffer[srcIndex + 2];
                        buffer[dstIndex + 3] = imageBuffer[srcIndex + 3];
                    }
                }
            }

            return CreateBitmapFromBuffer(buffer, width, height);
        }

        /// <summary>
        /// Places an image onto the canvas at position (0,0).
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

            int canvasWidth = _canvasState.Width;
            int canvasHeight = _canvasState.Height;
            Color backgroundColor = _canvasState.BackgroundColor;

            BitmapSource sourceImage = EnsureBgra32(image);
            int imageWidth = sourceImage.PixelWidth;
            int imageHeight = sourceImage.PixelHeight;

            // Create canvas buffer
            int stride = canvasWidth * 4;
            byte[] resultBuffer = new byte[stride * canvasHeight];

            // Fill with background color
            byte bgB = backgroundColor.B;
            byte bgG = backgroundColor.G;
            byte bgR = backgroundColor.R;
            byte bgA = backgroundColor.A;

            for (int i = 0; i < resultBuffer.Length; i += 4)
            {
                resultBuffer[i] = bgB;
                resultBuffer[i + 1] = bgG;
                resultBuffer[i + 2] = bgR;
                resultBuffer[i + 3] = bgA;
            }

            // Copy image pixels to position (0,0)
            int imageStride = imageWidth * 4;
            byte[] imageBuffer = new byte[imageStride * imageHeight];
            sourceImage.CopyPixels(imageBuffer, imageStride, 0);

            int copyWidth = Math.Min(imageWidth, canvasWidth);
            int copyHeight = Math.Min(imageHeight, canvasHeight);

            for (int y = 0; y < copyHeight; y++)
            {
                for (int x = 0; x < copyWidth; x++)
                {
                    int srcIndex = y * imageStride + x * 4;
                    int dstIndex = y * stride + x * 4;

                    resultBuffer[dstIndex] = imageBuffer[srcIndex];
                    resultBuffer[dstIndex + 1] = imageBuffer[srcIndex + 1];
                    resultBuffer[dstIndex + 2] = imageBuffer[srcIndex + 2];
                    resultBuffer[dstIndex + 3] = imageBuffer[srcIndex + 3];
                }
            }

            return CreateBitmapFromBuffer(resultBuffer, canvasWidth, canvasHeight);
        }

        /// <summary>
        /// Gets the current canvas dimensions.
        /// </summary>
        public (int Width, int Height) GetCanvasDimensions()
        {
            return (_canvasState.Width, _canvasState.Height);
        }
    }
}
