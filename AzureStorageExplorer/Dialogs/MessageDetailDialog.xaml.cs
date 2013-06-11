using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Neudesic.AzureStorageExplorer.ViewModel;
using Neudesic.AzureStorageExplorer.Windows;

namespace Neudesic.AzureStorageExplorer.Dialogs
{
    /// <summary>
    /// Interaction logic for CopyContainerDialog.xaml
    /// </summary>
    public partial class MessageDetailDialog : Window
    {
        public static string TextDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static string ImageDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        public static string VideoDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);

        public MessageDetailDialog()
        {
            InitializeComponent();
        }

        public BlobViewModel ViewModel
        {
            get
            {
                return DataContext as BlobViewModel;
            }
        }

        private void SaveProperties_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (MessageBox.Show("Are you SURE you want to update this blob's properties?", "Confirm Update",
                 MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                Cursor = Cursors.Wait;

                try
                {
                    ViewModel.SaveProperties();
                    MessageBox.Show("The blob properties have been updated", "Update Successful", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The blob update failed due to an error.\r\n\r\n" + ex.ToString(),
                        "Update Failed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                Cursor = Cursors.Arrow;
            }
        }

        private void CancelProperties_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void TabItem_Loaded(object sender, RoutedEventArgs e)
        {
        }

    }
}
