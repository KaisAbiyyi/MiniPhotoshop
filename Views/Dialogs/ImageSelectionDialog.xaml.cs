using System;
using System.Windows;

namespace MiniPhotoshop.Views.Dialogs
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
        public bool HasExistingImageA { get; set; }

        public ImageSelectionDialog()
        {
            InitializeComponent();
            SelectedTarget = ImageTarget.Cancel;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            
            // Update teks berdasarkan apakah sudah ada Image A
            if (HasExistingImageA)
            {
                TitleText.Text = "Gambar A sudah ada!";
                DescriptionText.Text = "Pilih apakah ingin mengganti Gambar A atau menambahkan sebagai Gambar B:";
                SetImageARadio.Content = "Ganti Gambar A (mengganti gambar utama)";
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (SetImageARadio.IsChecked == true)
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
