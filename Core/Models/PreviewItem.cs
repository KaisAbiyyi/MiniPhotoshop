using System.ComponentModel;
using System.Windows.Media;
using MiniPhotoshop.Core.Enums;

namespace MiniPhotoshop.Core.Models
{
    /// <summary>
    /// Represents a single preview entry displayed in the filter selector list.
    /// </summary>
    public sealed class PreviewItem : INotifyPropertyChanged
    {
        private bool _isActive;

        public PreviewItem(ImageFilterMode mode, string title, ImageSource previewSource, bool isActive)
        {
            Mode = mode;
            Title = title;
            PreviewSource = previewSource;
            _isActive = isActive;
        }

        public ImageFilterMode Mode { get; }

        public string Title { get; }

        public ImageSource PreviewSource { get; }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsActive)));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}

