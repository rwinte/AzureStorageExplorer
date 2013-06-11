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
using Neudesic.AzureStorageExplorer;

namespace Neudesic.AzureStorageExplorer.Dialogs
{
    /// <summary>
    /// Interaction logic for CopyContainerDialog.xaml
    /// </summary>
    public partial class BlobDetailDialog : Window
    {
        public static string TextDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static string ImageDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        public static string VideoDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        
        public BlobDetailDialog()
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

        private void SaveProperties_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you SURE you want to update this blob's properties?", "Confirm Update",
                 MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                Cursor = Cursors.Wait;

                try
                {
                    ViewModel.SaveProperties();
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The blob update failed due to an error.\r\n\r\n" + ex.ToString(),
                        "Update Failed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                Cursor = Cursors.Arrow;
            }
        }

        private void SaveMetadata_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you SURE you want to update this blob's metadata?", "Confirm Update",
                 MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                Cursor = Cursors.Wait;

                try
                {
                    ViewModel.SaveMetadata();
                    DialogResult = true;
                    Close();
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

        private void PreviewImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            ImageSpinner.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void PreviewImage_ImageCompleted(object sender, ExceptionRoutedEventArgs e)
        {
            ImageSpinner.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void PreviewImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void CancelPreview_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CancelTextPreview_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (TabControl.SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    ViewModel.ImageVisible = true;
                    break;
                case 3:
                    ViewModel.VideoVisible = true;
                    break;
                case 4:
                    ViewModel.TextVisible = true;
                    break;
            }
        }

        private void PreviewVideo_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            VideoSpinner.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void PreviewVideo_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void VideoTab_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void PreviewVideo_MediaOpened(object sender, RoutedEventArgs e)
        {
            VideoSpinner.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void SaveTextContent_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you SURE you want to update the content of this blob?", "Confirm Overwrite",
                 MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                Cursor = Cursors.Wait;

                try
                {
                    ViewModel.SaveText(TextEdit.Text);
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The blob update failed due to an error.\r\n\r\n" + ex.ToString(),
                        "Update Failed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                Cursor = Cursors.Arrow;
            }
        }

        private void UploadVideo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Title = "Upload Video File to Blob";
            dlg.InitialDirectory = VideoDirectory;

            HwndSource source = PresentationSource.FromVisual(MainWindow.Window) as HwndSource;
            System.Windows.Forms.IWin32Window win = new OldWindow(source.Handle);

            System.Windows.Forms.DialogResult result = dlg.ShowDialog(win);
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Cursor = Cursors.Wait;

                try
                {
                    ViewModel.SaveVideoFile(dlg.FileName);
                    MessageBox.Show("The blob has been updated", "Update Successful", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The blob update failed due to an error.\r\n\r\n" + ex.ToString(),
                        "Update Failed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                Cursor = Cursors.Arrow;
            }
        }

        private void UploadImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Title = "Upload Image File to Blob";
            dlg.InitialDirectory = ImageDirectory;

            HwndSource source = PresentationSource.FromVisual(MainWindow.Window) as HwndSource;
            System.Windows.Forms.IWin32Window win = new OldWindow(source.Handle);

            System.Windows.Forms.DialogResult result = dlg.ShowDialog(win);
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Cursor = Cursors.Wait;

                try
                {
                    ViewModel.SaveImageFile(dlg.FileName);
                    MessageBox.Show("The blob has been updated", "Update Successful", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The blob update failed due to an error.\r\n\r\n" + ex.ToString(),
                        "Update Failed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                Cursor = Cursors.Arrow;
            }
        }

        private void UploadTextFile_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Title = "Upload Text File to Blob";
            dlg.InitialDirectory = TextDirectory;

            dlg.Filter = "Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|Data Files (*.dat)|*.dat|All Files (*.*)|*.*";
            HwndSource source = PresentationSource.FromVisual(MainWindow.Window) as HwndSource;
            System.Windows.Forms.IWin32Window win = new OldWindow(source.Handle);

            System.Windows.Forms.DialogResult result = dlg.ShowDialog(win);
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Cursor = Cursors.Wait;

                try
                {
                    ViewModel.SaveTextFile(dlg.FileName);
                    MessageBox.Show("The blob has been updated", "Update Successful", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The blob update failed due to an error.\r\n\r\n" + ex.ToString(),
                        "Update Failed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                Cursor = Cursors.Arrow;
            }
        }

        private void ViewInBrowser_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string uri = ViewModel.Blob.CloudBlob.Uri.AbsoluteUri;
                System.Diagnostics.Process.Start(uri);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The blob Uri could not be opened.\r\n\r\n" + ex.ToString(), "Unable to View Blob", 
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

    }
}
