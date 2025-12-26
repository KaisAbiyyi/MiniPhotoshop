using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using MiniPhotoshop.Core.Enums;

namespace MiniPhotoshop.Core.Models
{
    /// <summary>
    /// Represents the mutable runtime state for the image workspace.
    /// </summary>
    public sealed class ImageWorkspaceState
    {
        public const int DefaultBinaryThreshold = 128;

        public ImageWorkspaceState()
        {
            PreviewItems = new ObservableCollection<PreviewItem>();
            ImageObjects = new ObservableCollection<ImageObject>();
        }

        public BitmapSource? OriginalBitmap { get; set; }

        public ObservableCollection<ImageObject> ImageObjects { get; }

        public Guid? SelectedImageId { get; set; }

        public Dictionary<ImageFilterMode, BitmapSource> FilterCache { get; } = new();

        public ObservableCollection<PreviewItem> PreviewItems { get; }

        public byte[,,]? PixelCache { get; set; }

        public PixelMetadata[,]? MetadataCache { get; set; }

        public int CachedWidth { get; set; }

        public int CachedHeight { get; set; }

        public string? CurrentFilePath { get; set; }

        public HistogramData Histogram { get; } = new();

        public BrightnessState Brightness { get; } = new();

        public bool IsNegationActive { get; set; }

        public bool IsBinaryThresholdActive { get; set; }

        public int BinaryThresholdValue { get; set; } = DefaultBinaryThreshold;

        public ColorSelectionState ColorSelection { get; } = new();

        public ImageFilterMode ActiveFilter { get; set; } = ImageFilterMode.Original;

        public double CurrentZoom { get; set; } = 1.0;

        public bool PendingAutoFit { get; set; }

        public WatermarkInfo? LastWatermark { get; set; }

        public void Reset()
        {
            OriginalBitmap = null;
            ImageObjects.Clear();
            SelectedImageId = null;
            FilterCache.Clear();
            PreviewItems.Clear();
            PixelCache = null;
            MetadataCache = null;
            CachedWidth = 0;
            CachedHeight = 0;
            CurrentFilePath = null;
            Histogram.Reset();
            Brightness.Reset();
            IsNegationActive = false;
            IsBinaryThresholdActive = false;
            BinaryThresholdValue = DefaultBinaryThreshold;
            ColorSelection.Reset();
            ActiveFilter = ImageFilterMode.Original;
            CurrentZoom = 1.0;
            PendingAutoFit = false;
            LastWatermark = null;
        }
    }
}

