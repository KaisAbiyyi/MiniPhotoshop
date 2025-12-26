using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MiniPhotoshop.Core.Enums;

namespace MiniPhotoshop.Views.MainWindow
{
    public partial class MainWindow
    {
        private void NegationButton_Click(object sender, RoutedEventArgs e)
        {
            if (_state.OriginalBitmap == null)
            {
                return;
            }

            // Toggle negation
            bool isCurrentlyNegated = NegationToggle.IsChecked.GetValueOrDefault();
            NegationToggle.IsChecked = !isCurrentlyNegated;

            // Update button appearance
            UpdateNegationButtonStyle(!isCurrentlyNegated);
        }

        private void UpdateNegationButtonStyle(bool isActive)
        {
            if (isActive)
            {
                NegationButton.Background = new SolidColorBrush(Color.FromRgb(76, 139, 245)); // #FF4C8BF5
                NegationButton.Foreground = Brushes.White;
                NegationButton.BorderBrush = new SolidColorBrush(Color.FromRgb(76, 139, 245));
            }
            else
            {
                NegationButton.Background = new SolidColorBrush(Color.FromRgb(248, 250, 252)); // #FFF8FAFC
                NegationButton.Foreground = new SolidColorBrush(Color.FromRgb(53, 66, 78)); // #FF35424E
                NegationButton.BorderBrush = new SolidColorBrush(Color.FromRgb(223, 226, 229)); // #FFDFE2E5
            }
        }


    }
}
