using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Neudesic.AzureStorageExplorer.Controls;
using Neudesic.AzureStorageExplorer.Data;
using Neudesic.AzureStorageExplorer.ViewModel;
using Neudesic.AzureStorageExplorer.Dialogs;
using Neudesic.AzureStorageExplorer.Properties;
using Neudesic.AzureStorageExplorer.Windows;
using Microsoft.WindowsAzure.Storage;
using Neudesic.AzureStorageExplorer;

namespace Neudesic.AzureStorageExplorer.View
{
    public partial class StorageAccountView : System.Windows.Controls.UserControl
    {
        public CommandViewModel ViewBlobsCommandView, ViewQueuesCommandView, ViewTablesCommandView, RefreshCommandView;
        public CommandViewModel NewContainerCommandView, CopyContainerCommandView, RenameContainerCommandView, DeleteContainerCommandView;
        public CommandViewModel NewBlobCommandView, CopyBlobCommandView, RenameBlobCommandView, DeleteBlobCommandView, UploadBlobCommandView, DownloadBlobCommandView;
        public CommandViewModel NewQueueCommandView, CopyQueueCommandView, RenameQueueCommandView, DeleteQueueCommandView;
        public CommandViewModel NewMessageCommandView, CopyMessageCommandView, RenameMessageCommandView, DeleteMessageCommandView, DeleteAllMessagesCommandView, UploadMessageCommandView, DownloadMessageCommandView;
        public CommandViewModel NewTableCommandView, CopyTableCommandView, RenameTableCommandView, DeleteTableCommandView;
        public CommandViewModel NewEntityCommandView, DeleteEntityCommandView, UploadEntityCommandView, DownloadEntityCommandView;

        private string DownloadDirectory { get; set; }

        public StorageAccountViewModel ViewModel
        {
            get
            {
                return DataContext as StorageAccountViewModel;
            }
        }

        public StorageAccountView()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(StorageAccountView_Loaded);

            ViewBlobsCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_ViewBlobs,
                    new RelayCommand(param => this.ViewBlobsCommandExecute()));

            ViewQueuesCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_ViewQueues,
                    new RelayCommand(param => this.ViewQueuesCommandExecute()));

            ViewTablesCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_ViewTables,
                    new RelayCommand(param => this.ViewTablesCommandExecute()));

            RefreshCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_Refresh,
                    new RelayCommand(param => this.RefreshCommandExecute()));

            NewContainerCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_NewContainer,
                    new RelayCommand(param => this.NewContainerCommandExecute()));

            CopyContainerCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_CopyContainer,
                    new RelayCommand(param => this.CopyContainerCommandExecute()));

            RenameContainerCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_RenameContainer,
                    new RelayCommand(param => this.RenameContainerCommandExecute()));

            DeleteContainerCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_DeleteContainer,
                    new RelayCommand(param => this.DeleteContainerCommandExecute()));

            NewBlobCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_NewBlob,
                    new RelayCommand(param => this.NewBlobCommandExecute()));

            CopyBlobCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_CopyBlob,
                    new RelayCommand(param => this.CopyBlobCommandExecute()));

            RenameBlobCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_RenameBlob,
                    new RelayCommand(param => this.RenameBlobCommandExecute()));

            DeleteBlobCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_DeleteBlob,
                    new RelayCommand(param => this.DeleteBlobCommandExecute()));

            UploadBlobCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_UploadBlob,
                    new RelayCommand(param => this.UploadBlobCommandExecute()));

            DownloadBlobCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_DownloadBlob,
                    new RelayCommand(param => this.DownloadBlobCommandExecute()));

            NewQueueCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_NewQueue,
                    new RelayCommand(param => this.NewQueueCommandExecute()));

            CopyQueueCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_CopyQueue,
                    new RelayCommand(param => this.CopyQueueCommandExecute()));

            RenameQueueCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_RenameQueue,
                    new RelayCommand(param => this.RenameQueueCommandExecute()));

            DeleteQueueCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_DeleteQueue,
                    new RelayCommand(param => this.DeleteQueueCommandExecute()));


            NewMessageCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_NewMessage,
                    new RelayCommand(param => this.NewMessageCommandExecute()));

            DeleteMessageCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_DeleteMessage,
                    new RelayCommand(param => this.DeleteMessageCommandExecute()));

            DeleteAllMessagesCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_DeleteAllMessages,
                    new RelayCommand(param => this.DeleteAllMessagesCommandExecute()));

            UploadMessageCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_UploadMessage,
                    new RelayCommand(param => this.UploadMessageCommandExecute()));

            DownloadMessageCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_DownloadMessage,
                    new RelayCommand(param => this.DownloadMessageCommandExecute()));

            NewTableCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_NewTable,
                    new RelayCommand(param => this.NewTableCommandExecute()));

            CopyTableCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_CopyTable,
                    new RelayCommand(param => this.CopyTableCommandExecute()));

            RenameTableCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_RenameTable,
                    new RelayCommand(param => this.RenameTableCommandExecute()));

            DeleteTableCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_DeleteTable,
                    new RelayCommand(param => this.DeleteTableCommandExecute()));

            NewEntityCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_NewEntity,
                    new RelayCommand(param => this.NewEntityCommandExecute()));

            DeleteEntityCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_DeleteEntity,
                    new RelayCommand(param => this.DeleteEntityCommandExecute()));

            UploadEntityCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_UploadEntity,
                    new RelayCommand(param => this.UploadEntityCommandExecute()));

            DownloadEntityCommandView = new CommandViewModel(
                    Strings.MainWindowViewModel_Command_DownloadEntity,
                    new RelayCommand(param => this.DownloadEntityCommandExecute()));
       }

        void StorageAccountView_Loaded(object sender, RoutedEventArgs e)
        {
            StorageAccountViewModel.Error += new RoutedEventHandler(ViewModel_Error);
            StorageAccountViewModel.TableColumnsChanged += new RoutedEventHandler(StorageAccountViewModel_TableColumnsChanged);
        }

        void StorageAccountViewModel_TableColumnsChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel != null)
                {
                    CreateEntityListColumns(ViewModel.TableColumnNames);
                }
            }
            catch (Exception)
            {
                // TODO: investigate "The calling thread cannot access this object because a different thread owns it." exception after an upload of entities.
            }
        }

        private void CommandBinding_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ViewBlobsCommandExecute()
        {
            Cursor = Cursors.Wait;
            ViewModel.ClearStatus();
            ViewModel.ViewBlobContainers();
            Cursor = Cursors.Arrow;
        }

        private void ViewQueuesCommandExecute()
        {
            Cursor = Cursors.Wait;
            ViewModel.ClearStatus();
            ViewModel.ViewQueues();
            Cursor = Cursors.Arrow;
        }

        private void ViewTablesCommandExecute()
        {
            Cursor = Cursors.Wait;
            ViewModel.ClearStatus();
            ViewModel.ViewTables();
            Cursor = Cursors.Arrow;
        }

        #region Container commands execution

        private void NewContainerCommandExecute()
        {
            NewContainerDialog dlg = new NewContainerDialog();
            dlg.Owner = MainWindow.Window;
            dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            if (dlg.ShowDialog().Value)
            {
                string name = dlg.ContainerName.Text;
                int access = 0;
                if (dlg.PublicBlob.IsChecked.Value)
                {
                    access = 1;
                }
                else if (dlg.PublicContainer.IsChecked.Value)
                {
                    access = 2;
                }
                ViewModel.NewContainer(name, access);
            }
        }

        private void CopyContainerCommandExecute()
        {
            CopyContainerDialog dlg = new CopyContainerDialog();
            dlg.Owner = MainWindow.Window;
            dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            if (FolderTree.SelectedItem != null)
            {
                dlg.SourceContainerName.Text = (FolderTree.SelectedItem as TreeItem).Text;
            }
            int access = 0;
            if (dlg.PublicBlob.IsChecked.Value)
            {
                access = 1;
            }
            else if (dlg.PublicContainer.IsChecked.Value)
            {
                access = 2;
            }
            if (dlg.ShowDialog().Value)
            {
                string name = dlg.SourceContainerName.Text;
                string destName = dlg.DestContainerName.Text;
                ViewModel.CopyContainer(name, destName, access);
            }
        }

        private void RenameContainerCommandExecute()
        {
            RenameContainerDialog dlg = new RenameContainerDialog();
            dlg.Owner = MainWindow.Window;
            dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            if (FolderTree.SelectedItem != null)
            {
                dlg.SourceContainerName.Text = (FolderTree.SelectedItem as TreeItem).Text;
            }
            int access = 0;
            if (dlg.PublicBlob.IsChecked.Value)
            {
                access = 1;
            }
            else if (dlg.PublicContainer.IsChecked.Value)
            {
                access = 2;
            }
            if (dlg.ShowDialog().Value)
            {
                string name = dlg.SourceContainerName.Text;
                string destName = dlg.DestContainerName.Text;
                ViewModel.RenameContainer(name, destName, access);
            }
        }

        private void DeleteContainerCommandExecute()
        {
            string name = (FolderTree.SelectedItem as TreeItem).Text;
            if (MessageBox.Show("Are you SURE you want to delete the container '" + name + "'?\r\n\r\nThe CONTAINER and its content will be permanently deleted.",
                "Confirm Container Delete", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                ViewModel.DeleteContainer(name);
            }
        }

        #endregion

        #region Blob commands execution

        private void NewBlobCommandExecute()
        {
            NewBlobDialog dlg = new NewBlobDialog();
            dlg.Owner = MainWindow.Window;
            dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            if (dlg.ShowDialog().Value)
            {
                string containerName = (FolderTree.SelectedItem as TreeItem).Text;
                string blobName = dlg.BlobName.Text;
                string textContent = dlg.TextContent.Text;

                if (dlg.BlockBlob.IsChecked.Value)
                {
                    ViewModel.NewBlockBlob(containerName, blobName, textContent);
                }
                else if (dlg.PageBlob.IsChecked.Value)
                {
                    long size = 0;
                    try
                    {
                        long multiplier = 1;
                        string value = dlg.PageBlobSize.Text.Trim().ToLower();
                        if (value.EndsWith("k"))
                        {
                            value = value.Substring(0, value.Length - 1);
                            multiplier = 1024;
                        }
                        else if (value.EndsWith("kb"))
                        {
                            value = value.Substring(0, value.Length - 2);
                            multiplier = 1024;
                        }
                        else if (value.EndsWith("m"))
                        {
                            value = value.Substring(0, value.Length - 1);
                            multiplier = 1024 * 1024;
                        }
                        else if (value.EndsWith("mb"))
                        {
                            value = value.Substring(0, value.Length - 2);
                            multiplier = 1024 * 1024;
                        }
                        else if (value.EndsWith("g"))
                        {
                            value = value.Substring(0, value.Length - 1);
                            multiplier = 1073741824;
                        }
                        else if (value.EndsWith("gb"))
                        {
                            value = value.Substring(0, value.Length - 2);
                            multiplier = 1073741824;
                        }
                        else if (value.EndsWith("t"))
                        {
                            value = value.Substring(0, value.Length - 1);
                            multiplier = 1099511627776;
                        }
                        else if (value.EndsWith("tb"))
                        {
                            value = value.Substring(0, value.Length - 2);
                            multiplier = 1099511627776;
                        }
                        size = long.Parse(value) * multiplier;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("The page blob size value is invalid.", "Size Invalid", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }

                    try
                    {
                        ViewModel.NewPageBlob(containerName, blobName, size);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("The page blob could not be created due to an error.\r\n\r\n" + ex.ToString(),
                            "Page Blob Creation Failed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                } 
            }
        }

        private void CopyBlobCommandExecute()
        {
            if (BlobList.SelectedItem == null)
            {
                MessageBox.Show("To copy a blob, select the blob name then click the Copy Blob button.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string containerName = (FolderTree.SelectedItem as TreeItem).Text;
            string sourceBlobName = (BlobList.SelectedItem as BlobDescriptor).Name;
            string destBlobName = String.Empty;

            CopyBlobDialog dlg = new CopyBlobDialog();
            dlg.Owner = MainWindow.Window;
            dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            if (FolderTree.SelectedItem != null)
            {
                dlg.SourceBlobName.Text = sourceBlobName;
            }
            if (dlg.ShowDialog().Value)
            {
                sourceBlobName = dlg.SourceBlobName.Text;
                destBlobName = dlg.DestBlobName.Text;
                ViewModel.CopyBlob(containerName, sourceBlobName, destBlobName);
            }
        }

        private void RenameBlobCommandExecute()
        {
            if (BlobList.SelectedItem == null)
            {
                MessageBox.Show("To rename a blob, select the blob name then click the Rename Blob button.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string containerName = (FolderTree.SelectedItem as TreeItem).Text;
            string sourceBlobName = (BlobList.SelectedItem as BlobDescriptor).Name;
            string destBlobName = String.Empty;

            RenameBlobDialog dlg = new RenameBlobDialog();
            dlg.Owner = MainWindow.Window;
            dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            if (FolderTree.SelectedItem != null)
            {
                dlg.SourceBlobName.Text = sourceBlobName;
            }
            if (dlg.ShowDialog().Value)
            {
                sourceBlobName = dlg.SourceBlobName.Text;
                destBlobName = dlg.DestBlobName.Text;
                ViewModel.RenameBlob(containerName, sourceBlobName, destBlobName);
            }
        }

        private void DeleteBlobCommandExecute()
        {
            if (BlobList.SelectedItems.Count == 0)
            {
                MessageBox.Show("To delete a blob, select the blob name then click the Delete Blob button.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string containerName = (FolderTree.SelectedItem as TreeItem).Text;
            
            int count = BlobList.SelectedItems.Count;

            string message;
            if (count == 1)
            {
                string blobName = (BlobList.SelectedItem as BlobDescriptor).Name;
                message = "Are you SURE you want to delete the blob '" + blobName + "'?" +
                    "\r\n\r\nThe blob will be permanently deleted.";
            }
            else
            {
                message = "Are you SURE you want to delete these " + count.ToString() + " blobs?" +
                    "\r\n\r\nThe blobs will be permanently deleted.";
            }

            if (MessageBox.Show(message,
                "Confirm Blob Delete", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                foreach (BlobDescriptor blob in BlobList.SelectedItems)
                {
                    ViewModel.DeleteBlob(containerName, blob.Name);
                }
            }
        }

        private void UploadBlobCommandExecute()
        {
            if (FolderTree.SelectedItem == null)
            {
                MessageBox.Show("To upload a blob, select the container to upload to then click the Upload Blob button.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string containerName = (FolderTree.SelectedItem as TreeItem).Text;

            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Title = "Upload Blob(s)";
            dlg.Multiselect = true;

            HwndSource source = PresentationSource.FromVisual(MainWindow.Window) as HwndSource;
            System.Windows.Forms.IWin32Window win = new OldWindow(source.Handle);

            System.Windows.Forms.DialogResult result = dlg.ShowDialog(win);
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Cursor = Cursors.Wait;

                try
                {
                    ViewModel.UploadBlobs(containerName, dlg.FileNames);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The blob upload failed due to an error.\r\n\r\n" + ex.ToString(),
                        "Update Failed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                Cursor = Cursors.Arrow;
            }
        }

        private void DownloadBlobCommandExecute()
        {
            string containerName = (FolderTree.SelectedItem as TreeItem).Text;

            bool abort = false;
            bool proceed = false;

            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "Choose a folder location to download blobs to.";
            if (DownloadDirectory != null)
            {
                dlg.SelectedPath = DownloadDirectory;
            }
            
            HwndSource source = PresentationSource.FromVisual(MainWindow.Window) as HwndSource;
            System.Windows.Forms.IWin32Window win = new OldWindow(source.Handle);

            System.Windows.Forms.DialogResult result = dlg.ShowDialog(win);
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Cursor = Cursors.Wait;

                DownloadDirectory = dlg.SelectedPath;

                try
                {
                    string folder = dlg.SelectedPath;
                    string subfolder;
                    string blobname;
                    string filename = String.Empty;
                    bool firstOverwrite = true;
                    bool overwriteAll = false;
                    List<CloudBlockBlob> blobList = new List<CloudBlockBlob>();
                    List<string> filenameList = new List<string>();

                    foreach (BlobDescriptor blob in BlobList.SelectedItems)
                    {
                        proceed = true;

                        subfolder = folder;
                        filename = folder;
                        if (!filename.EndsWith("\\"))
                        {
                            filename += "\\";
                        }
                        blobname = BlobDescriptor.BlobName(blob.CloudBlob).Replace("/", "_");
                        if (blobname.StartsWith("_"))
                        {
                            blobname = blobname.Substring(1);
                        }
                        filename += blobname;

                        if (File.Exists(filename))
                        {
                            if (firstOverwrite)
                            {
                                firstOverwrite = false;
                                switch (MessageBox.Show("One or more local files will be overwritten by this action. Do you want to overwrite existing files?\r\n\r\n" +
                                   "Press Yes to overwrite all files, No to prompt file-by-file, or Cancel to stop downloading.",
                                   "Overwrite Local Files?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                                    {
                                        case MessageBoxResult.Yes:
                                            overwriteAll = true;
                                            break;
                                        case MessageBoxResult.No:
                                            overwriteAll = false;
                                            break;
                                        case MessageBoxResult.Cancel:
                                            abort = true;
                                            break;
                                    }
                            }

                            if (!abort)
                            {
                                if (overwriteAll)
                                {
                                    proceed = true;
                                }
                                else
                                {
                                    switch (MessageBox.Show("File " + filename + " exists, do you want to overwrite it?\r\n\r\n" +
                                        "Press Yes to overwrite, No to skip, or Cancel to stop downloading.",
                                        "Confirm Local File Overwrite", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                                    {
                                        case MessageBoxResult.Yes:
                                            proceed = true;
                                            break;
                                        case MessageBoxResult.No:
                                            proceed = false;
                                            break;
                                        case MessageBoxResult.Cancel:
                                            proceed = false;
                                            abort = true;
                                            break;
                                    }
                                }
                            }
                        }
                        
                        if (proceed)
                        {
                            blobList.Add(blob.CloudBlob);
                            filenameList.Add(filename);
                        }

                        if (abort) break;
                    }

                    if (!abort && blobList.Count > 0)
                    {
                        ViewModel.DownloadBlobs(containerName, blobList.ToArray(), filenameList.ToArray());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The blob download failed due to an error.\r\n\r\n" + ex.ToString(),
                        "Update Failed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                Cursor = Cursors.Arrow;
            }
        }

        #endregion

        #region Queue commands execution

        private void NewQueueCommandExecute()
        {
            NewQueueDialog dlg = new NewQueueDialog();
            dlg.Owner = MainWindow.Window;
            dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            if (dlg.ShowDialog().Value)
            {
                string name = dlg.QueueName.Text;
                ViewModel.NewQueue(name);
            }
        }

        private void CopyQueueCommandExecute()
        {
            CopyQueueDialog dlg = new CopyQueueDialog();
            dlg.Owner = MainWindow.Window;
            dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            if (FolderTree.SelectedItem != null)
            {
                dlg.SourceQueueName.Text = (FolderTree.SelectedItem as TreeItem).Text;
            }
            if (dlg.ShowDialog().Value)
            {
                string name = dlg.SourceQueueName.Text;
                string destName = dlg.DestQueueName.Text;
                ViewModel.CopyQueue(name, destName);
            }
        }

        private void RenameQueueCommandExecute()
        {
            RenameQueueDialog dlg = new RenameQueueDialog();
            dlg.Owner = MainWindow.Window;
            dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            if (FolderTree.SelectedItem != null)
            {
                dlg.SourceQueueName.Text = (FolderTree.SelectedItem as TreeItem).Text;
            }
            if (dlg.ShowDialog().Value)
            {
                string name = dlg.SourceQueueName.Text;
                string destName = dlg.DestQueueName.Text;
                ViewModel.RenameQueue(name, destName);
            }
        }

        private void DeleteQueueCommandExecute()
        {
            string name = (FolderTree.SelectedItem as TreeItem).Text;
            if (MessageBox.Show("Are you SURE you want to delete the queue '" + name + "'?\r\n\r\nThe queue and its messages will be permanently deleted.",
                "Confirm Queue Delete", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                ViewModel.DeleteQueue(name);
            }
        }

        #endregion

        #region Message command execution

        private void NewMessageCommandExecute()
        {
            NewMessageDialog dlg = new NewMessageDialog();
            dlg.Owner = MainWindow.Window;
            dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            if (dlg.ShowDialog().Value)
            {
                string queueName = (FolderTree.SelectedItem as TreeItem).Text;
                string textContent = dlg.TextContent.Text;

                ViewModel.NewMessage(queueName, textContent);
            }
        }

        private void DeleteMessageCommandExecute()
        {
            string queueName = (FolderTree.SelectedItem as TreeItem).Text;

            string messageText;
            string messageId = (ViewModel.Messages[0] as CloudQueueMessage).Id;
            messageText = "Are you SURE you want to delete the top-most queue message '" + messageId + "'?" +
                "\r\n\r\nThe message will be permanently deleted.";

            if (MessageBox.Show(messageText,
                "Confirm Message Delete", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                ViewModel.DeleteMessage(queueName);
            }
        }

        private void DeleteAllMessagesCommandExecute()
        {
            string queueName = (FolderTree.SelectedItem as TreeItem).Text;

            string messageText;
            string messageId = (ViewModel.Messages[0] as CloudQueueMessage).Id;
            messageText = "Are you SURE you want to delete ALL messages from queue '" + queueName + "'?" +
                "\r\n\r\nThe messages will be permanently deleted.";

            if (MessageBox.Show(messageText,
                "Confirm Delete All Messages", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                ViewModel.DeleteAllMessages(queueName);
            }
        }

        private void UploadMessageCommandExecute()
        {
            if (FolderTree.SelectedItem == null)
            {
                MessageBox.Show("To upload a message, select the queue to upload to then click the Upload Message button.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string queueName = (FolderTree.SelectedItem as TreeItem).Text;

            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Title = "Upload Messages(s)";
            dlg.Multiselect = true;

            HwndSource source = PresentationSource.FromVisual(MainWindow.Window) as HwndSource;
            System.Windows.Forms.IWin32Window win = new OldWindow(source.Handle);

            System.Windows.Forms.DialogResult result = dlg.ShowDialog(win);
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Cursor = Cursors.Wait;

                try
                {
                    ViewModel.UploadMessages(queueName, dlg.FileNames);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The message upload failed due to an error.\r\n\r\n" + ex.ToString(),
                        "Update Failed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                Cursor = Cursors.Arrow;
            }
        }

        private void DownloadMessageCommandExecute()
        {
            string queueName = (FolderTree.SelectedItem as TreeItem).Text;

            bool abort = false;
            bool proceed = false;

            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "Choose a folder location to download messages to.";
            if (DownloadDirectory != null)
            {
                dlg.SelectedPath = DownloadDirectory;
            }

            HwndSource source = PresentationSource.FromVisual(MainWindow.Window) as HwndSource;
            System.Windows.Forms.IWin32Window win = new OldWindow(source.Handle);

            System.Windows.Forms.DialogResult result = dlg.ShowDialog(win);
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Cursor = Cursors.Wait;

                DownloadDirectory = dlg.SelectedPath;

                try
                {
                    string folder = dlg.SelectedPath;
                    string subfolder;
                    string messagename;
                    string filename = String.Empty;
                    bool firstOverwrite = true;
                    bool overwriteAll = false;
                    List<CloudQueueMessage> messageList = new List<CloudQueueMessage>();
                    List<string> filenameList = new List<string>();

                    foreach (CloudQueueMessage message in MessageList.SelectedItems)
                    {
                        proceed = true;

                        subfolder = folder;
                        filename = folder;
                        if (!filename.EndsWith("\\"))
                        {
                            filename += "\\";
                        }
                        messagename = message.Id;
                        if (messagename.StartsWith("_"))
                        {
                            messagename = messagename.Substring(1);
                        }
                        filename += messagename + ".xml";

                        if (File.Exists(filename))
                        {
                            if (firstOverwrite)
                            {
                                firstOverwrite = false;
                                switch (MessageBox.Show("One or more local files will be overwritten by this action. Do you want to overwrite existing files?\r\n\r\n" +
                                   "Press Yes to overwrite all files, No to prompt file-by-file, or Cancel to stop downloading.",
                                   "Overwrite Local Files?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                                {
                                    case MessageBoxResult.Yes:
                                        overwriteAll = true;
                                        break;
                                    case MessageBoxResult.No:
                                        overwriteAll = false;
                                        break;
                                    case MessageBoxResult.Cancel:
                                        abort = true;
                                        break;
                                }
                            }

                            if (!abort)
                            {
                                if (overwriteAll)
                                {
                                    proceed = true;
                                }
                                else
                                {
                                    switch (MessageBox.Show("File " + filename + " exists, do you want to overwrite it?\r\n\r\n" +
                                        "Press Yes to overwrite, No to skip, or Cancel to stop downloading.",
                                        "Confirm Local File Overwrite", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                                    {
                                        case MessageBoxResult.Yes:
                                            proceed = true;
                                            break;
                                        case MessageBoxResult.No:
                                            proceed = false;
                                            break;
                                        case MessageBoxResult.Cancel:
                                            proceed = false;
                                            abort = true;
                                            break;
                                    }
                                }
                            }
                        }

                        if (proceed)
                        {
                            messageList.Add(message);
                            filenameList.Add(filename);
                        }

                        if (abort) break;
                    }

                    if (!abort && messageList.Count > 0)
                    {
                        ViewModel.DownloadMessages(queueName, messageList.ToArray(), filenameList.ToArray());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The message download failed due to an error.\r\n\r\n" + ex.ToString(),
                        "Update Failed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                Cursor = Cursors.Arrow;
            }
        }

        #endregion

        #region Table commands execution

        private void NewTableCommandExecute()
        {
            NewTableDialog dlg = new NewTableDialog();
            dlg.Owner = MainWindow.Window;
            dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            if (dlg.ShowDialog().Value)
            {
                string name = dlg.TableName.Text;
                ViewModel.NewTable(name);
            }
        }

        private void CopyTableCommandExecute()
        {
            CopyTableDialog dlg = new CopyTableDialog();
            dlg.Owner = MainWindow.Window;
            dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            if (FolderTree.SelectedItem != null)
            {
                dlg.SourceTableName.Text = (FolderTree.SelectedItem as TreeItem).Text;
            }
            if (dlg.ShowDialog().Value)
            {
                Cursor = Cursors.Wait;
                string name = dlg.SourceTableName.Text;
                string destName = dlg.DestTableName.Text;
                ViewModel.CopyTable(name, destName);
                Cursor = Cursors.Arrow;
            }
        }

        private void RenameTableCommandExecute()
        {
            RenameTableDialog dlg = new RenameTableDialog();
            dlg.Owner = MainWindow.Window;
            dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            if (FolderTree.SelectedItem != null)
            {
                dlg.SourceTableName.Text = (FolderTree.SelectedItem as TreeItem).Text;
            }
            if (dlg.ShowDialog().Value)
            {
                string name = dlg.SourceTableName.Text;
                string destName = dlg.DestTableName.Text;
                ViewModel.RenameTable(name, destName);
            }
        }

        private void DeleteTableCommandExecute()
        {
            string name = (FolderTree.SelectedItem as TreeItem).Text;
            if (MessageBox.Show("Are you SURE you want to delete the table '" + name + "'?\r\n\r\nThe table and its entities will be permanently deleted.",
                "Confirm Table Delete", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                ViewModel.DeleteTable(name);
            }
        }

        #endregion

        #region Entity command execution

        private void NewEntityCommandExecute()
        {
            GenericEntity entity = new GenericEntity();
            EditEntityDialog dlg = new EditEntityDialog("new");
            dlg.Owner = MainWindow.Window;
            dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            EntityViewModel evm = new EntityViewModel(entity, ViewModel.TableColumnNames);
            dlg.DataContext = evm;
            if (dlg.ShowDialog().Value)
            {
                string tableName = (FolderTree.SelectedItem as TreeItem).Text;

                ViewModel.NewEntity(tableName, evm.UpdatedEntity);
            }
        }

        private void DeleteEntityCommandExecute()
        {
            if (EntityList.SelectedItems.Count == 0)
            {
                MessageBox.Show("To delete an entity, select the entity name then click the Delete Entity button.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string tableName = (FolderTree.SelectedItem as TreeItem).Text;

            int count = EntityList.SelectedItems.Count;

            string message;
            if (count == 1)
            {
                GenericEntity targetEntity = (EntityList.SelectedItem as GenericEntity);

                message = "Are you SURE you want to delete the entity '" + targetEntity.Key() + "'?" +
                    "\r\n\r\nThe entity will be permanently deleted.";
            }
            else
            {
                message = "Are you SURE you want to delete these " + count.ToString() + " entities?" +
                    "\r\n\r\nThe entities will be permanently deleted.";
            }

            if (MessageBox.Show(message,
                "Confirm Entity Delete", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                int deleted = 0;
                int errors = 0;
                foreach (GenericEntity entity in EntityList.SelectedItems)
                {
                    deleted++;
                    if (!ViewModel.DeleteEntity(tableName, entity))
                    {
                        errors++;
                    }
                }
                ViewModel.ReportDeleteEntities(tableName, deleted, errors); // Show final status and refresh entities list.
            }

        }

        private void UploadEntityCommandExecute()
        {
            if (FolderTree.SelectedItem == null)
            {
                MessageBox.Show("To upload entities to a table, select the table to upload to then click the Upload Entity button.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string tableName = (FolderTree.SelectedItem as TreeItem).Text;
            string format = "csv";
            string filename;
            bool optionColumnDefinitions = true;

            UploadTableDialog dlg = new UploadTableDialog();
            dlg.Owner = MainWindow.Window;
            dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            dlg.TableName.Text = tableName;
            dlg.Format = format;

            if (dlg.ShowDialog().Value)
            {
                Cursor = Cursors.Wait;

                format = dlg.Format;
                tableName = dlg.TableName.Text;
                filename = dlg.FileName.Text;
                optionColumnDefinitions = dlg.OptionColumnDefinitions.IsChecked.Value;

                try
                {
                    ViewModel.UploadEntities(filename, format, optionColumnDefinitions, tableName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The table upload failed due to an error.\r\n\r\n" + ex.ToString(),
                        "Update Failed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                Cursor = Cursors.Arrow;
            }
        }

        private void DownloadEntityCommandExecute()
        {
            string tableName = (FolderTree.SelectedItem as TreeItem).Text;

            bool proceed = true;

            string format = "csv";

            bool optionOutputColumnHeader = true;
            bool includeColumnTypes = true;
            bool includeNullableValues = true;

            DownloadTableDialog dlg = new DownloadTableDialog();
            dlg.Owner = MainWindow.Window;
            dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            dlg.TableName.Text = tableName;
            dlg.Format = format;
            dlg.OptionColumnDefinitions.IsChecked = optionOutputColumnHeader;
            dlg.OptionTypeDefinitions.IsChecked = includeColumnTypes;
            dlg.OptionNullValues.IsChecked = includeNullableValues;

            if (dlg.ShowDialog().Value)
            {
                tableName = dlg.TableName.Text;
                format = dlg.Format;
                string filename = dlg.FileName.Text;
                optionOutputColumnHeader = dlg.OptionColumnDefinitions.IsChecked.Value;
                includeColumnTypes = dlg.OptionTypeDefinitions.IsChecked.Value;
                includeNullableValues = dlg.OptionNullValues.IsChecked.Value;

                Cursor = Cursors.Wait;

                try
                {
                    if (File.Exists(filename))
                    {
                        if (MessageBox.Show("File " + filename + " exists - overwrite it?", "Overwrite Local Files?",
                            MessageBoxButton.YesNoCancel, MessageBoxImage.Question) != MessageBoxResult.Yes)
                        {
                            proceed = false;
                        }
                    }

                    if (proceed)
                    {
                        ViewModel.DownloadEntities(tableName, format, optionOutputColumnHeader, includeColumnTypes, includeNullableValues, filename);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The table download failed due to an error.\r\n\r\n" + ex.ToString(),
                        "Update Failed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                Cursor = Cursors.Arrow;
            }
        }

        #endregion


        private void RefreshCommandExecute()
        {
            Cursor = Cursors.Wait;
            ViewModel.ClearStatus();
            ViewModel.Refresh();
            Cursor = Cursors.Arrow;
        }

        #region Toolbar Button Handlers

        #region View Toolbar Handlers

        private void ViewBlobsButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ClearStatus();
            ViewBlobsCommandView.Command.Execute(null);
        }

        private void ViewQueuesButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ClearStatus();
            ViewQueuesCommandView.Command.Execute(null);
        }

        private void ViewTablesButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ClearStatus();
            ViewTablesCommandView.Command.Execute(null);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ClearStatus();
            RefreshCommandView.Command.Execute(null);
        }

        #endregion

        #region Container Toolbar Handlers

        private void NewContainerButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ClearStatus();
            NewContainerCommandView.Command.Execute(null);
        }

        private void CopyContainerButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem == null)
            {
                MessageBox.Show("Please select a container to copy.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            ViewModel.ClearStatus();
            CopyContainerCommandView.Command.Execute(null);
        }

        private void RenameContainerButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem == null)
            {
                MessageBox.Show("Please select a container to rename.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            ViewModel.ClearStatus();
            RenameContainerCommandView.Command.Execute(null);
        }

        private void DeleteContainerButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem == null)
            {
                MessageBox.Show("Please select a container to delete.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            ViewModel.ClearStatus();
            DeleteContainerCommandView.Command.Execute(null);
        }

        #endregion

        #region Blob Toolbar Handlers

        private void ViewBlobButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem == null || BlobList.SelectedItems.Count == 0)
            {
                MessageBox.Show("To view a blob, select a blob then click View.\r\n\r\nYou can also double-click a blob to view it.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            ViewModel.ClearStatus();

            BlobDescriptor blob = BlobList.SelectedItems[0] as BlobDescriptor;

            if (blob != null)
            {
                BlobDetailDialog dlg = new BlobDetailDialog();

                dlg.Owner = MainWindow.Window;
                dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;

                dlg.DataContext = new BlobViewModel(blob);

                if (dlg.ShowDialog().Value)
                {
                    RefreshDetail();
                }
            }
        }

        private void RefreshDetail()
        {
            if (FolderTree.SelectedItem != null)
            {
                ViewModel.RefreshDetail((FolderTree.SelectedItem as TreeItem).Text);
            }
        }

        private void NewBlobButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem == null)
            {
                MessageBox.Show("To create a blob, select a container then click New Blob.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ViewModel.ClearStatus();
            NewBlobCommandView.Command.Execute(null);
        }

        private void CopyBlobButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem == null || BlobList.SelectedItems.Count == 0)
            {
                MessageBox.Show("To copy a blob, select a blob then click Copy Blob.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ViewModel.ClearStatus();
            CopyBlobCommandView.Command.Execute(null);
        }

        private void RenameBlobButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem == null || BlobList.SelectedItems.Count == 0)
            {
                MessageBox.Show("To rename a blob, select a blob then click Rename Blob.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ViewModel.ClearStatus();
            RenameBlobCommandView.Command.Execute(null);
        }

        private void DeleteBlobButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem == null || BlobList.SelectedItems.Count == 0)
            {
                MessageBox.Show("To delete a blob, select a blob then click Delete Blob.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ViewModel.ClearStatus();
            DeleteBlobCommandView.Command.Execute(null);
        }

        private void UploadBlobButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem == null)
            {
                MessageBox.Show("To upload a blob, select a container then click Upload Blob.", 
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ViewModel.UploadInProgress)
            {
                MessageBox.Show("An upload is already in progress and must completed before another upload can be started.",
                    "Previous Upload Active", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ViewModel.DownloadInProgress)
            {
                MessageBox.Show("A download is already in progress and must completed before an upload can be started.",
                    "Previous Download Active", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ViewModel.ClearStatus();
            UploadBlobCommandView.Command.Execute(null);
        }

        private void DownloadBlobButton_Click(object sender, RoutedEventArgs e)
        {
            if (BlobList.SelectedItems.Count == 0)
            {
                MessageBox.Show("To download a blob, select a blob then click Download Blob.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ViewModel.DownloadInProgress)
            {
                MessageBox.Show("A download is already in progress and must completed before another download can be started.",
                    "Previous Download Active", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ViewModel.UploadInProgress)
            {
                MessageBox.Show("An upload is already in progress and must completed before a download can be started.",
                    "Previous Upload Active", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ViewModel.ClearStatus();
            DownloadBlobCommandView.Command.Execute(null);
        }

        #endregion

        #region Queue Toolbar Handlers

        private void NewQueueButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ClearStatus();
            NewQueueCommandView.Command.Execute(null);
        }

        private void CopyQueueButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ClearStatus();
            CopyQueueCommandView.Command.Execute(null);
        }

        private void RenameQueueButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ClearStatus();
            RenameQueueCommandView.Command.Execute(null);
        }

        private void DeleteQueueButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem == null)
            {
                MessageBox.Show("Please select a queue to delete.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            ViewModel.ClearStatus();
            DeleteQueueCommandView.Command.Execute(null);
        }

        #endregion

        #region Message Toolbar Handlers

        private void ViewMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem == null || MessageList.SelectedItems.Count == 0)
            {
                MessageBox.Show("To view a message, select a message then click View.\r\n\r\nYou can also double-click a message to view it.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            ViewModel.ClearStatus();
            CloudQueueMessage message = MessageList.SelectedItems[0] as CloudQueueMessage;

            if (message != null)
            {
                MessageDetailDialog dlg = new MessageDetailDialog();

                dlg.Owner = MainWindow.Window;
                dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;

                dlg.DataContext = new MessageViewModel(message);

                dlg.ShowDialog();
            }
        }

        private void NewMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem == null)
            {
                MessageBox.Show("To create a message, select a queue then click New Message.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ViewModel.ClearStatus();
            NewMessageCommandView.Command.Execute(null);
        }

        private void DeleteMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Messages.Count == 0)
            {
                MessageBox.Show("There are no messages in the queue. To delete a message, add one or more messages to the queue then click Delete Message.",
                    "No Messages", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ViewModel.Messages.Count != 1)
            {
                if (MessageList.SelectedItems.Count > 0 && (MessageList.SelectedItems[0] as CloudQueueMessage) != ViewModel.Messages[0])
                {
                    MessageBox.Show("Only the top-most message in a queue may be deleted. To delete a message, select the top-most message then click Delete Message.",
                        "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            ViewModel.ClearStatus();
            DeleteMessageCommandView.Command.Execute(null);
        }

        private void DeleteAllMessagesButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Messages.Count == 0)
            {
                MessageBox.Show("There are no messages in the queue.",
                    "No Messages", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ViewModel.ClearStatus();
            DeleteAllMessagesCommandView.Command.Execute(null);
        }

        private void UploadMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem == null)
            {
                MessageBox.Show("To upload a message, select a container then click Upload Message.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ViewModel.UploadInProgress)
            {
                MessageBox.Show("An upload is already in progress and must completed before another upload can be started.",
                    "Previous Upload Active", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ViewModel.DownloadInProgress)
            {
                MessageBox.Show("A download is already in progress and must completed before an upload can be started.",
                    "Previous Download Active", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ViewModel.ClearStatus();
            UploadMessageCommandView.Command.Execute(null);
        }

        private void DownloadMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageList.SelectedItems.Count == 0)
            {
                MessageBox.Show("To download a message, select a message then click Download Message.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ViewModel.DownloadInProgress)
            {
                MessageBox.Show("A download is already in progress and must completed before another download can be started.",
                    "Previous Download Active", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ViewModel.UploadInProgress)
            {
                MessageBox.Show("An upload is already in progress and must completed before a download can be started.",
                    "Previous Upload Active", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ViewModel.ClearStatus();
            DownloadMessageCommandView.Command.Execute(null);
        }

        #endregion

        #region Table Toolbar Handlers

        private void NewTableButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ClearStatus();
            NewTableCommandView.Command.Execute(null);
        }

        private void CopyTableButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ClearStatus();
            CopyTableCommandView.Command.Execute(null);
        }

        private void RenameTableButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("To 'rename' a table, it is copied and then the original is deleted. It is safest to not delete the original until the copy is confirmed.\r\n\r\nDo you want to proceed with the copy?",
                "Rename Table Note", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                ViewModel.ClearStatus();
                RenameTableCommandView.Command.Execute(null);
            }
        }

        private void DeleteTableButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem == null)
            {
                MessageBox.Show("Please select a table to delete.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            ViewModel.ClearStatus();
            DeleteTableCommandView.Command.Execute(null);
        }

        #endregion

        #region Entity Toolbar Handlers

        private void NewEntityButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem == null)
            {
                MessageBox.Show("To create an entity, select a table then click New Entity.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ViewModel.ClearStatus();
            NewEntityCommandView.Command.Execute(null);
        }

        private void CopyEntityButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem == null || EntityList.SelectedItems.Count == 0)
            {
                MessageBox.Show("To view or edit an entity, select an entity then click Edit.\r\n\r\nYou can also double-click an entity to view it.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            ViewModel.ClearStatus();

            GenericEntity entity = EntityList.SelectedItems[0] as GenericEntity;

            if (entity != null)
            {
                EditEntityDialog dlg = new EditEntityDialog("copy");

                dlg.Owner = MainWindow.Window;
                dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;

                EntityViewModel evm = new EntityViewModel(entity, ViewModel.TableColumnNames);
                dlg.DataContext = evm;

                if (dlg.ShowDialog().Value)
                {
                    string tableName = (FolderTree.SelectedItem as TreeItem).Text;

                    ViewModel.NewEntity(tableName, evm.UpdatedEntity);
                }
            }

        }

        private void DeleteEntityButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem == null || EntityList.SelectedItems.Count == 0)
            {
                MessageBox.Show("To delete an entity, select an entity then click Delete Entity.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ViewModel.ClearStatus();
            DeleteEntityCommandView.Command.Execute(null);
        }

        private void UploadEntityButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem == null)
            {
                MessageBox.Show("To upload an entity, select a table then click Upload Entity.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ViewModel.UploadInProgress)
            {
                MessageBox.Show("An upload is already in progress and must completed before another upload can be started.",
                    "Previous Upload Active", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ViewModel.DownloadInProgress)
            {
                MessageBox.Show("A download is already in progress and must completed before an upload can be started.",
                    "Previous Download Active", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ViewModel.ClearStatus();
            UploadEntityCommandView.Command.Execute(null);
        }

        private void DownloadEntityButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem == null)
            {
                MessageBox.Show("Please select a table to download.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            string tableName = (FolderTree.SelectedItem as TreeItem).Text;

            if (ViewModel.DownloadInProgress)
            {
                MessageBox.Show("A download is already in progress and must completed before another download can be started.",
                    "Previous Download Active", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ViewModel.UploadInProgress)
            {
                MessageBox.Show("An upload is already in progress and must completed before a download can be started.",
                    "Previous Upload Active", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            //if (MessageBox.Show("All entities from table '" + tableName + "' will be downloaded to a file in CSV format that can be opened in Excel or a text editor.\r\n\r\nColumn headers on the first row will be based on the fields contained in the first entity.\r\n\r\nDo you want to proceed?",
            //    "Confirm Table Download", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            //{
                ViewModel.ClearStatus();
                DownloadEntityCommandView.Command.Execute(null);
            //}
        }

        #endregion

        private void FolderTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (ViewModel == null) return;

            ViewModel.ClearFolderSummary();

            TreeItem treeItem = FolderTree.SelectedItem as TreeItem;

            if (treeItem != null)
            {
                ViewModel.HideEntities();

                if (ViewModel.BlobsSelected)
                {
                    ViewModel.ViewBlobItems(treeItem.Text);
                }
                else if (ViewModel.QueuesSelected)
                {
                    ViewModel.ViewMessages(treeItem.Text);
                }
                else if (ViewModel.TablesSelected)
                {
                    ViewModel.ReportFolderSummary("Table: " + treeItem.Text);
                }
            }
        }

        private string EntityQuery()
        {
            string query = QueryText.Text.Trim();
            if (string.IsNullOrEmpty(query))
            {
                return null;
            }
            else
            {
                return query;
            }
        }

        private int EntityMaxRecords()
        {
            string text = MaxRowsText.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                return -1;
            }
            else
            {
                try
                {
                    return Int32.Parse(text);
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }


        private void CreateEntityListColumns(Dictionary<string, Column> columns)
        {
            GridView view = new GridView();
            GridViewColumn col;

            view.AllowsColumnReorder = true;
            view.ColumnHeaderContainerStyle = Resources["GridViewColumnHeaderStyle"] as Style;

            col = new GridViewColumn();

            col.DisplayMemberBinding = new Binding("PartitionKey");
            col.Header = "PartitionKey";
            view.Columns.Add(col);

            col = new GridViewColumn();
            col.DisplayMemberBinding = new Binding("RowKey");
            col.Header = "RowKey";
            view.Columns.Add(col);

            col = new GridViewColumn();
            col.DisplayMemberBinding = new Binding("Timestamp");
            col.Header = "Timestamp";
            view.Columns.Add(col);

            foreach (KeyValuePair<string, Column> column in columns)
            {
                switch (column.Key)
                {
                    case "PartitionKey":
                    case "RowKey":
                    case "Timestamp":
                        break;
                    default:
                        col = new GridViewColumn();
                        col.DisplayMemberBinding = new Binding("Properties[" + column.Key + "]");
                        col.Header = column.Key;
                        view.Columns.Add(col);
                        break;
                }
            }

            EntityList.View = view;
            ViewModel.RefreshBindingEntities();
        }

        #endregion

        // If a blob is double-clicked, this has same the same effect as clicking the View Blob button
        // for a selected blob.

        private void BlobList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (BlobList.SelectedItems.Count == 0) return;

            ViewModel.ClearStatus();
            ViewBlobButton_Click(sender, new RoutedEventArgs());
        }

        private void BlobList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void ViewEntityButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem == null || EntityList.SelectedItems.Count == 0)
            {
                MessageBox.Show("To view or edit an entity, select an entity then click Edit.\r\n\r\nYou can also double-click an entity to view it.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            ViewModel.ClearStatus();
            GenericEntity entity = EntityList.SelectedItems[0] as GenericEntity;

            if (entity != null)
            {
                EditEntityDialog dlg = new EditEntityDialog("edit");

                dlg.Owner = MainWindow.Window;
                dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;

                EntityViewModel evm = new EntityViewModel(entity, ViewModel.TableColumnNames);
                dlg.DataContext = evm;

                if (dlg.ShowDialog().Value)
                {
                    string tableName = (FolderTree.SelectedItem as TreeItem).Text;
                    ViewModel.UpdateEntity(tableName, entity, evm.UpdatedEntity);
                }
            }
        }


        private void MessageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void MessageList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (MessageList.SelectedItems.Count == 0) return;

            ViewModel.ClearStatus();
            ViewMessageButton_Click(sender, new RoutedEventArgs());
        }

        #region Error Reporting UI

        static void ViewModel_Error(object sender, RoutedEventArgs e)
        {

            PlaySoundAsterisk();
        }

        static private void PlaySoundAsterisk()
        {
            System.Media.SystemSounds.Asterisk.Play();
        }

        static private void PlaySoundBeep()
        {
            System.Media.SystemSounds.Beep.Play();
        }

        static private void PlaySoundExclamation()
        {
            System.Media.SystemSounds.Exclamation.Play();
        }

        static private void PlaySoundHand()
        {
            System.Media.SystemSounds.Hand.Play();
        }

        static private void PlaySoundQuestion()
        {
            System.Media.SystemSounds.Question.Play();
        }

        #endregion

        private void EntityList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FolderTree.SelectedItem == null)
            {
                MessageBox.Show("To view entities, select a table.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
        }

        private void EntityList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (EntityList.SelectedItems.Count == 0) return;

            ViewModel.ClearStatus();
            ViewEntityButton_Click(sender, new RoutedEventArgs());
        }

        // View entities match query condition.

        private void QueryEntities_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem == null)
            {
                MessageBox.Show("Please select a table to query.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            ViewModel.ClearStatus();
            string tableName = (FolderTree.SelectedItem as TreeItem).Text;
            ViewModel.ViewEntities(tableName, EntityQuery(), EntityMaxRecords());
        }

        private void QueryText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(QueryText.Text))
            {
                QueryTextGhost.Opacity = 1;
            }
            else
            {
                QueryTextGhost.Opacity = 0;
            }
        }

        // Delete entities matching query condition.

        private void DeleteEntities_Click(object sender, RoutedEventArgs e)
        {
            string message;
            string query = QueryText.Text;
            string tableName = (FolderTree.SelectedItem as TreeItem).Text;

            if (String.IsNullOrEmpty(QueryText.Text))
            {
                message = "Are you SURE you want to delete ALL ENTITIES from table '" + tableName + "'?\r\n\r\nEntities will be permanently deleted.";
            }
            else
            {
                message = "Are you SURE you want to delete ALL MATCHING ENTITIES from table '" + tableName + "'?\r\n\r\nEntities will be permanently deleted.";
                
            }

            if (MessageBox.Show(message, "Confirm Delete Entities", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                ViewModel.ClearStatus();
                ViewModel.DeleteEntities(tableName, query);
            }
        }

        private void SecurityBlobButton_Click(object sender, RoutedEventArgs e)
        {
            if (FolderTree.SelectedItem == null)
            {
                MessageBox.Show("Please select a container to use.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            string containerName = (FolderTree.SelectedItem as TreeItem).Text;

            string blobName = null;

            if (FolderTree.SelectedItem != null && BlobList.SelectedItems.Count > 0)
            {
                BlobDescriptor blob = BlobList.SelectedItems[0] as BlobDescriptor;
                blobName = blob.Name;
            }

            ViewModel.ClearStatus();

            BlobSecurityDialog dlg = new BlobSecurityDialog();
            dlg.Owner = MainWindow.Window;
            dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            dlg.ContainerName = containerName;
            dlg.BlobName = blobName;
            dlg.ViewModel = ViewModel;
            dlg.ShowDialog();

            //if (dlg.ShowDialog().Value)
            //{
            //    //string name = dlg.ContainerName.Text;
                //int access = 0;
                //if (dlg.PublicBlob.IsChecked.Value)
                //{
                //    access = 1;
                //}
                //else if (dlg.PublicContainer.IsChecked.Value)
                //{
                //    access = 2;
                //}
                //ViewModel.NewContainer(name, access);
            //}
        }
    }
}