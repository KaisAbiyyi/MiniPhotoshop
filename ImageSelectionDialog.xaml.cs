using System.Windows;

namespace MiniPhotoshop
{
    public partial class ImageSelectionDialog : Window
    {
        public enum ImageTarget
        {
            ImageA,
            ImageB,
            Cancel
        }

        public ImageTarget SelectedTarget { get; private set; }

        public ImageSelectionDialog()
        {
            InitializeComponent();
            SelectedTarget = ImageTarget.Cancel;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (ReplaceImageARadio.IsChecked == true)
            {
                SelectedTarget = ImageTarget.ImageA;
            }
            else if (SetImageBRadio.IsChecked == true)
            {
                SelectedTarget = ImageTarget.ImageB;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedTarget = ImageTarget.Cancel;
            DialogResult = false;
            Close();
        }
    }
}
