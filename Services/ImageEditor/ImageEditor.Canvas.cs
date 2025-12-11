using System;
using System.Linq;
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

            // Create canvas buffer
            int stride = canvasWidth * 4;
            byte[] buffer = new byte[stride * canvasHeight];
            
            // Initialize MetadataCache
            State.MetadataCache = new PixelMetadata[canvasWidth, canvasHeight];

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
            
            // Render images sorted by ZIndex
            var sortedImages = System.Linq.Enumerable.OrderBy(State.ImageObjects, x => x.ZIndex).ToList();
            
            foreach (var imgObj in sortedImages)
            {
                if (!imgObj.IsVisible) continue;
                
                BitmapSource image = EnsureBgra32(imgObj.Bitmap);
                int imageWidth = image.PixelWidth;
                int imageHeight = image.PixelHeight;
                int offsetX = imgObj.OffsetX;
                int offsetY = imgObj.OffsetY;
                
                int imageStride = imageWidth * 4;
                byte[] imagePixels = new byte[imageStride * imageHeight];
                image.CopyPixels(imagePixels, imageStride, 0);
                
                // Calculate visible region
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
                            
                            int srcIndex = (srcY * imageStride) + (srcX * 4);
                            int dstIndex = (dstY * stride) + (dstX * 4);
                            
                            byte srcA = imagePixels[srcIndex + 3];
                            
                            if (srcA > 0)
                            {
                                float alpha = srcA / 255f;
                                float invAlpha = 1.0f - alpha;
                                
                                buffer[dstIndex] = (byte)((imagePixels[srcIndex] * alpha) + (buffer[dstIndex] * invAlpha));     // B
                                buffer[dstIndex + 1] = (byte)((imagePixels[srcIndex + 1] * alpha) + (buffer[dstIndex + 1] * invAlpha)); // G
                                buffer[dstIndex + 2] = (byte)((imagePixels[srcIndex + 2] * alpha) + (buffer[dstIndex + 2] * invAlpha)); // R
                                buffer[dstIndex + 3] = 255; 
                                
                                // Update Metadata
                                State.MetadataCache[dstX, dstY] = new PixelMetadata
                                {
                                    ImageObjectId = imgObj.Id,
                                    LocalX = srcX,
                                    LocalY = srcY
                                };
                            }
                        }
                    }

                    // Draw selection border if selected
                    if (imgObj.IsSelected)
                    {
                        DrawSelectionBorder(buffer, canvasWidth, canvasHeight, dstStartX, dstStartY, copyWidth, copyHeight);
                    }
                }
            }

            return CreateBitmapFromBuffer(buffer, canvasWidth, canvasHeight);
        }

        private void DrawSelectionBorder(byte[] buffer, int canvasWidth, int canvasHeight, int x, int y, int width, int height)
        {
            int stride = canvasWidth * 4;
            int thickness = 2;
            byte borderR = 0, borderG = 120, borderB = 215; // Windows Blue

            // Top and Bottom
            for (int i = 0; i < width; i++)
            {
                for (int t = 0; t < thickness; t++)
                {
                    // Top
                    int topY = y + t;
                    if (topY < canvasHeight)
                    {
                        int idx = (topY * stride) + ((x + i) * 4);
                        if (idx >= 0 && idx < buffer.Length - 4)
                        {
                            buffer[idx] = borderB;
                            buffer[idx + 1] = borderG;
                            buffer[idx + 2] = borderR;
                        }
                    }

                    // Bottom
                    int bottomY = y + height - 1 - t;
                    if (bottomY >= 0)
                    {
                        int idx = (bottomY * stride) + ((x + i) * 4);
                        if (idx >= 0 && idx < buffer.Length - 4)
                        {
                            buffer[idx] = borderB;
                            buffer[idx + 1] = borderG;
                            buffer[idx + 2] = borderR;
                        }
                    }
                }
            }

            // Left and Right
            for (int i = 0; i < height; i++)
            {
                for (int t = 0; t < thickness; t++)
                {
                    // Left
                    int leftX = x + t;
                    if (leftX < canvasWidth)
                    {
                        int idx = ((y + i) * stride) + (leftX * 4);
                        if (idx >= 0 && idx < buffer.Length - 4)
                        {
                            buffer[idx] = borderB;
                            buffer[idx + 1] = borderG;
                            buffer[idx + 2] = borderR;
                        }
                    }

                    // Right
                    int rightX = x + width - 1 - t;
                    if (rightX >= 0)
                    {
                        int idx = ((y + i) * stride) + (rightX * 4);
                        if (idx >= 0 && idx < buffer.Length - 4)
                        {
                            buffer[idx] = borderB;
                            buffer[idx + 1] = borderG;
                            buffer[idx + 2] = borderR;
                        }
                    }
                }
            }
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

            // Add image to object manager
            AddImage(image, "Layer " + (State.ImageObjects.Count + 1));
            
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
