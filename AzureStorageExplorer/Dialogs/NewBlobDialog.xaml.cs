using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Neudesic.AzureStorageExplorer.Dialogs
{
    /// <summary>
    /// Interaction logic for NewContainerDialog.xaml
    /// </summary>
    public partial class NewBlobDialog : Window
    {
        public NewBlobDialog()
        {
            InitializeComponent();

            BlobName.Focus();
        }

        private void CreateBlob_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateInput()
        {
            if (String.IsNullOrEmpty(BlobName.Text))
            {
                MessageBox.Show("A blob name is required", "Blob Name Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            return true;
        }

        private void BlockBlob_Checked(object sender, RoutedEventArgs e)
        {
            if (PageBlobSizeLabel == null) return;

            PageBlobSizeLabel.Visibility = System.Windows.Visibility.Collapsed;
            PageBlobSize.Visibility = System.Windows.Visibility.Collapsed;
            TextContentLabel.Opacity = 1;
            TextContent.Opacity = 1;
        }

        private void PageBlob_Checked(object sender, RoutedEventArgs e)
        {
            if (PageBlobSizeLabel == null) return;

            TextContentLabel.Opacity = 0;
            TextContent.Opacity = 0;
            PageBlobSizeLabel.Visibility = System.Windows.Visibility.Visible;
            PageBlobSize.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
