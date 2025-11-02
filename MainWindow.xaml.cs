using System;
using System.Windows;
using System.Windows.Interop;
using MiniPhotoshop.Core.Models;
using MiniPhotoshop.Services.Contracts;
using MiniPhotoshop.Services.ImageEditor;

#nullable enable

namespace MiniPhotoshop
{
    public partial class MainWindow : Window
    {
        private readonly ImageWorkspaceState _state = new();
        private readonly ImageEditor _editor;
        private readonly IImageLoaderService _imageLoader;
        private readonly IPixelExportService _pixelExporter;
        private readonly IFilterService _filterService;
        private readonly INegationService _negationService;
        private readonly IBrightnessService _brightnessService;
        private readonly IBinaryThresholdService _binaryThresholdService;
        private readonly IColorSelectionService _colorSelectionService;
        private readonly IHistogramService _histogramService;
        private readonly IGrayscaleComparisonService _grayscaleService;
        private readonly IProcessedImageProvider _imageProvider;
        private readonly IWorkspaceResetService _workspaceResetService;

        private HwndSource? _hwndSource;
        private double _currentZoom = 1.0;
        private bool _suppressBrightnessHandler;

        private const double MinZoom = 0.1;
        private const double MaxZoom = 5.0;
        private const double HorizontalWheelStep = 48.0;
        private const int WM_MOUSEHWHEEL = 0x020E;

        public MainWindow()
        {
            InitializeComponent();

            _editor = new ImageEditor(_state);
            _imageLoader = _editor;
            _pixelExporter = _editor;
            _filterService = _editor;
            _negationService = _editor;
            _brightnessService = _editor;
            _binaryThresholdService = _editor;
            _colorSelectionService = _editor;
            _histogramService = _editor;
            _grayscaleService = _editor;
            _imageProvider = _editor;
            _workspaceResetService = _editor;

            FilterPreviewList.ItemsSource = _state.PreviewItems;
            DisplayImage.RenderTransformOrigin = new Point(0.5, 0.5);
            WorkspaceScrollViewer.SizeChanged += WorkspaceScrollViewer_SizeChanged;
            BinaryThresholdSlider.Value = ImageWorkspaceState.DefaultBinaryThreshold;

            UpdateUiForNoImage();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _hwndSource = (HwndSource?)PresentationSource.FromVisual(this);
            _hwndSource?.AddHook(WndProc);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_hwndSource != null)
            {
                _hwndSource.RemoveHook(WndProc);
                _hwndSource = null;
            }

            base.OnClosed(e);
        }
    }
}
