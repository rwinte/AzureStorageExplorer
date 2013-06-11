using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Services.Client;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.DataServices;
using Neudesic.AzureStorageExplorer.Controls;
using Neudesic.AzureStorageExplorer.Data;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Neudesic.AzureStorageExplorer.ViewModel
{
    /// <summary>
    /// ViewModel for Storage Account. 
    /// </summary>
    /// 
    public class StorageAccountViewModel : WorkspaceViewModel
    {
        XNamespace AtomNamespace = "http://www.w3.org/2005/Atom";
        XNamespace AstoriaDataNamespace = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        XNamespace AstoriaMetadataNamespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

        #region Properties

        #region Status Reporting

        public static event RoutedEventHandler Error;
        public static event RoutedEventHandler TableColumnsChanged;

        private Brush StatusBackgroundSuccess = new SolidColorBrush(Colors.LightGreen);
        private Brush StatusBackgroundActive = new SolidColorBrush(Colors.PowderBlue);
        private Brush StatusBackgroundWarning = new SolidColorBrush(Colors.Khaki);
        private Brush StatusBackgroundError = new SolidColorBrush(Colors.Pink);
        private Brush StatusBackgroundNone = new SolidColorBrush(Colors.Transparent);
        
        public Brush StatusMessageBackground { get; set; }

        public string StatusMessageText { get; set; }

        public string FolderSummary { get; set; }
        public string ItemSummary { get; set; }

        #endregion

        #region Entity Type Mapping

        private static Dictionary<Type, string> typesMapping = new Dictionary<Type, string>();
        
        #endregion

        private bool BlobsLoaded { get; set; }
        private bool QueuesLoaded { get; set; }
        private bool TablesLoaded { get; set; }

        public string SelectedContainerName { get; set; }
        public string SelectedQueueName { get; set; }
        public string SelectedTableName { get; set; }

        private List<CloudBlobContainer> Containers = new List<CloudBlobContainer>();
        private List<CloudQueue> Queues = new List<CloudQueue>();
        private List<string> Tables = new List<string>();

        private List<BlobDescriptor> BlobItemNodes = new List<BlobDescriptor>();
        private List<CloudQueueMessage> MessageNodes = new List<CloudQueueMessage>();
        private List<GenericEntity> EntityNodes = new List<GenericEntity>();

        private List<TreeItem> ContainerNodes = new List<TreeItem>();
        private List<TreeItem> QueueNodes = new List<TreeItem>();
        private List<TreeItem> TableNodes = new List<TreeItem>();

        private string SourceTableName, DestTableName;

        private Visibility _blobListVisibility = Visibility.Collapsed;
        public System.Windows.Visibility BlobListVisibility
        {
            get
            {
                return _blobListVisibility;
            }
            set
            {
                _blobListVisibility = value;
                base.OnPropertyChanged("BlobListVisibility");
            }
        }

        private Visibility _messageListVisibility = Visibility.Collapsed;
        public System.Windows.Visibility MessageListVisibility
        {
            get
            {
                return _messageListVisibility;
            }
            set
            {
                _messageListVisibility = value;
                base.OnPropertyChanged("MessageListVisibility");
            }
        }

        private Visibility _entityListVisibility = Visibility.Collapsed;
        public System.Windows.Visibility EntityListVisibility
        {
            get
            {
                return _entityListVisibility;
            }
            set
            {
                _entityListVisibility = value;
                base.OnPropertyChanged("EntityListVisibility");
            }
        }

        private Visibility _listSpinnerVisible = Visibility.Visible;
        public System.Windows.Visibility ListSpinnerVisible
        {
            get
            {
                return _listSpinnerVisible;
            }
            set
            {
                _listSpinnerVisible = value;
                base.OnPropertyChanged("ListSpinnerVisible");
            }
        }

        private Visibility _detailSpinnerVisible = Visibility.Collapsed;
        public System.Windows.Visibility DetailSpinnerVisible
        {
            get
            {
                return _detailSpinnerVisible;
            }
            set
            {
                if (value == Visibility.Collapsed)
                {
                    if (UploadInProgress || DownloadInProgress) return;
                }
                _detailSpinnerVisible = value;
                base.OnPropertyChanged("DetailSpinnerVisible");
            }
        }

        StorageAccount _account;

        public StorageAccount Account
        {
            get
            {
                return _account;
            }
            set
            {
                _account = value;
            }
        }

        private CloudStorageAccount CloudStorageAccount { get; set; }
        
        public bool BlobContainersUpgraded { get; set; }

        #region Storage Type Selection & Visibility

        private bool _blobsSelected = false;
        public bool BlobsSelected
        {
            get
            {
                return _blobsSelected;
            }
            set
            {
                _blobsSelected = value;

                if (value)
                {
                    BlobListVisibility = Visibility.Visible;
                    MessageListVisibility = Visibility.Collapsed;
                    EntityListVisibility = Visibility.Collapsed;
                    base.OnPropertyChanged("BlobsSelected");
                    base.OnPropertyChanged("BlobsVisible");
                    base.OnPropertyChanged("QueuesSelected");
                    base.OnPropertyChanged("QueuesVisible");
                    base.OnPropertyChanged("TablesSelected");
                    base.OnPropertyChanged("TablesVisible");
                    base.OnPropertyChanged("Folders");
                }
            }
        }

        public System.Windows.Visibility BlobsVisible
        {
            get
            {
                if (BlobsSelected)
                {
                    return System.Windows.Visibility.Visible;
                }
                else
                {
                    return System.Windows.Visibility.Collapsed;
                }
            }
        }

        private bool _queuesSelected = false;
        public bool QueuesSelected
        {
            get
            {
                return _queuesSelected;
            }
            set
            {
                _queuesSelected = value;

                if (value)
                {
                    BlobListVisibility = Visibility.Collapsed;
                    MessageListVisibility = Visibility.Visible;
                    EntityListVisibility = Visibility.Collapsed;
                    base.OnPropertyChanged("BlobsSelected");
                    base.OnPropertyChanged("BlobsVisible");
                    base.OnPropertyChanged("QueuesSelected");
                    base.OnPropertyChanged("QueuesVisible");
                    base.OnPropertyChanged("TablesSelected");
                    base.OnPropertyChanged("TablesVisible");
                    base.OnPropertyChanged("Folders");
                }
            }
        }

        public System.Windows.Visibility QueuesVisible
        {
            get
            {
                if (QueuesSelected)
                {
                    return System.Windows.Visibility.Visible;
                }
                else
                {
                    return System.Windows.Visibility.Collapsed;
                }
            }
        }

        private bool _tablesSelected = false;
        public bool TablesSelected
        {
            get
            {
                return _tablesSelected;
            }
            set
            {
                _tablesSelected = value;

                if (value)
                {
                    BlobListVisibility = Visibility.Collapsed;
                    MessageListVisibility = Visibility.Collapsed;
                    EntityListVisibility = Visibility.Visible;
                    base.OnPropertyChanged("BlobsSelected");
                    base.OnPropertyChanged("BlobsVisible");
                    base.OnPropertyChanged("QueuesSelected");
                    base.OnPropertyChanged("QueuesVisible");
                    base.OnPropertyChanged("TablesSelected");
                    base.OnPropertyChanged("TablesVisible");
                    base.OnPropertyChanged("Folders");
                }
            }
        }

        public System.Windows.Visibility TablesVisible
        {
            get
            {
                if (TablesSelected)
                {
                    return System.Windows.Visibility.Visible;
                }
                else
                {
                    return System.Windows.Visibility.Collapsed;
                }
            }
        }

        #endregion

        #region Left Pane (Folders) Backing Storage

        public ObservableCollection<TreeItem> Folders
        {
            get
            {
                if (BlobsSelected)
                {
                    return new ObservableCollection<TreeItem>(ContainerNodes);
                }
                else if (QueuesSelected)
                {
                    return new ObservableCollection<TreeItem>(QueueNodes);
                }
                else if (TablesSelected)
                {
                    return new ObservableCollection<TreeItem>(TableNodes);
                }
                else
                {
                    return null;
                }
            }
            set
            {

            }
        }

        #endregion

        #region Right Pane (Items) Backing Detail

        public ObservableCollection<BlobDescriptor> BlobItems
        {
            get
            {
                lock (BlobItemNodes)
                {
                    return new ObservableCollection<BlobDescriptor>(BlobItemNodes);
                }
            }
        }

        public ObservableCollection<CloudQueueMessage> Messages
        {
            get
            {
                lock (MessageNodes)
                {
                    return new ObservableCollection<CloudQueueMessage>(MessageNodes);
                }
            }
        }

        public ObservableCollection<GenericEntity> Entities 
        {
            get
            {
                lock (EntityNodes)
                {
                    return new ObservableCollection<GenericEntity>(EntityNodes);
                }
            }
        }

        #endregion

        #endregion

        #region Constructor

        static StorageAccountViewModel()
        {
            typesMapping.Add(typeof(int), "Edm.Int32");

            typesMapping.Add(typeof(double), "Edm.Double");

            typesMapping.Add(typeof(byte[]), "Edm.Binary");

            typesMapping.Add(typeof(Guid), "Edm.Guid");

            typesMapping.Add(typeof(DateTime), "Edm.DateTime");

            typesMapping.Add(typeof(bool), "Edm.Boolean");

            typesMapping.Add(typeof(long), "Edm.Int64");
        }

        // Create a view model from a storage account.

        public StorageAccountViewModel(StorageAccount account)
        {
            if (account == null)
                throw new ArgumentNullException("account");

            this.Account = account;
            base.DisplayName = account.Name;

            this.BlobContainersUpgraded = account.BlobContainersUpgraded;

            if (OpenAccount())
            {
                this.ViewBlobContainers();
            }
        }

        #endregion // Constructor

        #region  Disposal

        protected override void OnDispose()
        {
        }

        #endregion // Base Class Overrides

        #region Status Reporting Methodsd

        private void ReportSuccess(string text)
        {
            StatusMessageBackground = StatusBackgroundSuccess;
            StatusMessageText = text;
            OnPropertyChanged("StatusMessageBackground");
            OnPropertyChanged("StatusMessageText");
        }

        private void ReportError(string text)
        {
            StatusMessageBackground = StatusBackgroundError;
            StatusMessageText = text;
            OnPropertyChanged("StatusMessageBackground");
            OnPropertyChanged("StatusMessageText");
            if (Error != null) Error(this, null);
        }

        private void ReportException(Exception ex)
        {
            lock (MainWindow.Exceptions)
            {
                MainWindow.Exceptions.Add(ex);
            }
            StatusMessageBackground = StatusBackgroundError;
            string text;
            if (ex.Message != null)
            {
                text = ex.Message;
            }
            else
            {
                text = "An error has occurred";
            }

            StatusMessageText = text;
            OnPropertyChanged("StatusMessageBackground");
            OnPropertyChanged("StatusMessageText");
            if (Error != null) Error(this, null);
        }

        private void ReportActive(string text)
        {
            StatusMessageBackground = StatusBackgroundActive;
            StatusMessageText = text;
            OnPropertyChanged("StatusMessageBackground");
            OnPropertyChanged("StatusMessageText");
        }

        private void ReportWarning(string text)
        {
            StatusMessageBackground = StatusBackgroundWarning;
            StatusMessageText = text;
            OnPropertyChanged("StatusMessageBackground");
            OnPropertyChanged("StatusMessageText");
        }

        public void ClearStatus()
        {
            StatusMessageBackground = StatusBackgroundNone;
            StatusMessageText = String.Empty;
            OnPropertyChanged("StatusMessageBackground");
            OnPropertyChanged("StatusMessageText");
        }

        private void ReportStatus(Brush background, string text)
        {
            StatusMessageBackground = background;
            StatusMessageText = text;
            OnPropertyChanged("StatusMessageBackground");
            OnPropertyChanged("StatusMessageText");
        }

        private void ReportItemSummary(string text)
        {
            ItemSummary = text;
            OnPropertyChanged("ItemSummary");
        }

        public void ClearItemSummary()
        {
            ItemSummary = String.Empty;
            OnPropertyChanged("ItemSummary");
        }

        public void ReportFolderSummary(string text)
        {
            FolderSummary = text;
            OnPropertyChanged("FolderSummary");
        }

        public void ClearFolderSummary()
        {
            FolderSummary = String.Empty;
            OnPropertyChanged("FolderSummary");
        }

        #endregion

        #region Public Methods

        #region Left Pane (Folders) Content Loading & Refresh

        #region View Blob Containers

        private int ViewBlobErrors = 0;

        public void ViewBlobContainers()
        {
            ViewBlobErrors = 0;

            QueuesSelected = false;
            TablesSelected = false;
            BlobsSelected = true;

            ClearItemSummary();

            lock (BlobItemNodes)
            {
                BlobItemNodes.Clear();
            }
            OnPropertyChanged("BlobItems");

            if (!BlobsLoaded)
            {
                ListSpinnerVisible = Visibility.Visible;

                if (!BlobContainersUpgraded)
                {
                    ReportWarning("Loading blob containers - may take some time if older containers need upgrading");
                }
                else
                {
                    ReportActive("Loading blob containers");
                }

                BackgroundWorker background = new BackgroundWorker();
                background.DoWork += new DoWorkEventHandler(Background_LoadContainers);
                background.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Background_LoadContainersCompleted);
                background.RunWorkerAsync();
            }
        }

        private void Background_LoadContainers(object sender, DoWorkEventArgs e)
        {
            LoadContainers();
        }

        void Background_LoadContainersCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            int upgraded = 0;
            TreeItem tvi;
            List<TreeItem> items = new List<TreeItem>();

            foreach (CloudBlobContainer container in Containers)
            {
                tvi = new TreeItem();
                tvi.Text = NormalizeContainerName(container.Uri.LocalPath.Substring(1));

                try
                {
                    System.Diagnostics.Debug.Print(container.Name);
                    if (container.GetPermissions().PublicAccess != BlobContainerPublicAccessType.Off)
                    {
                        tvi.Image = new BitmapImage(new Uri("/Images/Folder24.png", UriKind.Relative));
                    }
                    else
                    {
                        tvi.Image = new BitmapImage(new Uri("/Images/LockedFolder24.png", UriKind.Relative));
                    }
                }
                catch (InvalidOperationException ex)
                {
                    // This means we encountered an old-style container with permissions
                    // set the old way (true/false), prior to the breaking change
                    // Reset the permissions so the problem goes away.

                    BlobContainerPermissions perms = new BlobContainerPermissions();
                    if (ex.Message.Contains("True"))
                    {
                        perms.PublicAccess = BlobContainerPublicAccessType.Blob;
                        tvi.Image = new BitmapImage(new Uri("/Images/LockedFolder24.png", UriKind.Relative));
                    }
                    else
                    {
                        perms.PublicAccess = BlobContainerPublicAccessType.Off;
                        tvi.Image = new BitmapImage(new Uri("/Images/Folder24.png", UriKind.Relative));
                    }
                    container.SetPermissions(perms);
                }
                
                tvi.Tag = container;

                items.Add(tvi);
            }

            ContainerNodes = items;
            base.OnPropertyChanged("Folders");

            if (ViewBlobErrors == 0)
            {
                if (upgraded > 0)
                {
                    ReportWarning("Upgraded " + numberof(upgraded, "container", "containers") + " to current standards");
                }
                else
                {
                    ClearStatus();
                }
            }

            if (!BlobContainersUpgraded)
            {
                BlobContainersUpgraded = true;
                Account.BlobContainersUpgraded = true;
                AccountViewModel avm = MainWindow.GetAccount(Account.Name);
                if (avm != null)
                {
                    avm.BlobContainersUpgraded = true;
                    MainWindow.SaveConfiguration();
                }
            }
            
            ListSpinnerVisible = Visibility.Collapsed;
        }

        #endregion

        #region View Queues

        private int ViewQueuesErrors = 0;

        public void ViewQueues()
        {
            ViewQueuesErrors = 0;

            BlobsSelected = false;
            TablesSelected = false;
            QueuesSelected = true;

            ClearItemSummary();

            lock (MessageNodes)
            {
                MessageNodes.Clear();
            }
            OnPropertyChanged("Messages");

            if (!QueuesLoaded)
            {
                ReportActive("Loading queues");

                BackgroundWorker background = new BackgroundWorker();
                background.DoWork += new DoWorkEventHandler(Background_LoadQueues);
                background.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Background_LoadQueuesCompleted);
                background.RunWorkerAsync();
            }
        }

        private void Background_LoadQueues(object sender, DoWorkEventArgs e)
        {
            LoadQueues();
        }

        void Background_LoadQueuesCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<TreeItem> items = new List<TreeItem>();
            TreeItem tvi;
            foreach (CloudQueue queue in Queues)
            {
                tvi = new TreeItem();
                tvi.Text = NormalizeQueueName(queue.Uri.LocalPath.Substring(1));
                tvi.Image = new BitmapImage(new Uri("/Images/Queue.png", UriKind.Relative));

                tvi.Tag = queue;

                items.Add(tvi);
            }
            QueueNodes = items;
            base.OnPropertyChanged("Folders");

            if (ViewQueuesErrors == 0)
            {
                ClearStatus();
            }

            ListSpinnerVisible = Visibility.Collapsed;
        }

        #endregion

        #region View Tables

        private int ViewTablesErrors = 0;

        public void ViewTables()
        {
            ViewTablesErrors = 0;

            BlobsSelected = false;
            QueuesSelected = false;
            TablesSelected = true;

            ClearItemSummary();

            OnPropertyChanged("Entities");

            if (!TablesLoaded)
            {
                ReportActive("Loading Tables");
                TableNodes.Clear();
                BackgroundWorker background = new BackgroundWorker();
                background.DoWork += new DoWorkEventHandler(Background_LoadTables);
                background.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Background_LoadTablesCompleted);
                background.RunWorkerAsync();
            }
            else
            {
                base.OnPropertyChanged("Folders");
            }
        }

        public void RefreshBindingEntities()
        {
            OnPropertyChanged("Entities");
        }

        private void Background_LoadTables(object sender, DoWorkEventArgs e)
        {
            LoadTables();
        }

        void Background_LoadTablesCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<TreeItem> items = new List<TreeItem>();
            TreeItem tvi;
            foreach (string table in Tables)
            {
                tvi = new TreeItem();
                tvi.Text = table;
                tvi.Image = new BitmapImage(new Uri("/Images/Table.png", UriKind.Relative));

                tvi.Tag = table;

                items.Add(tvi);
            }
            TableNodes = items;
            base.OnPropertyChanged("Folders");

            if (ViewTablesErrors == 0)
            {
                ClearStatus();
            }

            ListSpinnerVisible = Visibility.Collapsed;
        }

        #endregion

        // Refresh left pane - containers/queues/tables.

        public void Refresh()
        {
            if (BlobsSelected)
            {
                BlobsLoaded = false;
                ViewBlobContainers();
            }
            else if (QueuesSelected)
            {
                QueuesLoaded = false;
                ViewQueues();
            }
            else if (TablesSelected)
            {
                TablesLoaded = false;
                ViewTables();
            }
        }

        #endregion

        #region Right Pane (Items) Content Loading & Refresh

        public void RefreshDetail(string name)
        {
            if (BlobsSelected)
            {
                ViewBlobItems(name);
            }
            else if (QueuesSelected)
            {
                ViewMessages(name);
            }
            else if (TablesSelected)
            {
                ViewEntities(name, TableQuery, TableMaxRecords);
            }
        }

        #region View Blob Items

        public void ViewBlobItems(string selectedContainerName)
        {
            if (!BlobsSelected) return;

            selectedContainerName = NormalizeContainerName(selectedContainerName);
            ReportFolderSummary("Container: " + selectedContainerName);
            SelectedContainerName = selectedContainerName;
            DetailSpinnerVisible = Visibility.Visible;
            BackgroundWorker background = new BackgroundWorker();
            background.DoWork += new DoWorkEventHandler(Background_LoadBlobItems);
            background.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Background_LoadBlobItemsCompleted);
            background.RunWorkerAsync();
        }

        private void Background_LoadBlobItems(object sender, DoWorkEventArgs e)
        {
            LoadBlobItems(SelectedContainerName);
        }

        void Background_LoadBlobItemsCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            base.OnPropertyChanged("BlobItems");
            DetailSpinnerVisible = Visibility.Collapsed;
        }

        #endregion

        #region View Messages

        public void ViewMessages(string selectedQueueName)
        {
            if (!QueuesSelected) return;

            ReportFolderSummary("Queue: " + selectedQueueName);
            SelectedQueueName = selectedQueueName;
            DetailSpinnerVisible = Visibility.Visible;
            BackgroundWorker background = new BackgroundWorker();
            background.DoWork += new DoWorkEventHandler(Background_LoadMessages);
            background.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Background_LoadMessagesCompleted);
            background.RunWorkerAsync();
        }

        private void Background_LoadMessages(object sender, DoWorkEventArgs e)
        {
            LoadMessages(SelectedQueueName);
        }

        void Background_LoadMessagesCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            base.OnPropertyChanged("Messages");
            DetailSpinnerVisible = Visibility.Collapsed;
        }

        #endregion

        #region View Entities

        public Dictionary<string, Column> TableColumnNames = new Dictionary<string, Column>();

        private string TableQuery = null;
        private int TableMaxRecords = -1;

        public void ViewEntities(string selectedTableName, string query, int maxRecords)
        {
            if (!TablesSelected) return;

            ReportFolderSummary("Table: " + selectedTableName);
            TableColumnNames.Clear();
            SelectedTableName = selectedTableName;

            if (query != null)
            {
                query = query.Replace("\"", "'");
                query = query.Replace(" = ", " eq ");
                query = query.Replace(" == ", " eq ");
                query = query.Replace(" <> ", " ne ");
                query = query.Replace(" != ", " ne ");
                query = query.Replace(" <= ", " le ");
                query = query.Replace(" < ", " lt ");
                query = query.Replace(" >= ", " ge ");
                query = query.Replace(" > ", " gt ");
            }
            TableQuery = query;
            
            TableMaxRecords = maxRecords;
            DetailSpinnerVisible = Visibility.Visible;
            BackgroundWorker background = new BackgroundWorker();
            background.DoWork += new DoWorkEventHandler(Background_LoadEntities);
            background.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Background_LoadEntitiesCompleted);
            background.RunWorkerAsync();
        }

        private void Background_LoadEntities(object sender, DoWorkEventArgs e)
        {
            LoadEntities(SelectedTableName);
        }

        void Background_LoadEntitiesCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (TableColumnsChanged != null)
            {
                TableColumnsChanged(this, new RoutedEventArgs());
            }
            DetailSpinnerVisible = Visibility.Collapsed;
        }

        public void HideEntities()
        {
            lock (EntityNodes)
            {
                EntityNodes.Clear();
            }
            OnPropertyChanged("Entities");
        }

        #endregion

        #endregion

        #region Container Actions

        #region New Container

        public void NewContainer(string name, int access /* 2=public container, 1=public blob, 0=private */)
        {
            if (CloudStorageAccount == null) return;

            name = NormalizeContainerName(name);

            try
            {
                CloudBlobClient client = CloudStorageAccount.CreateCloudBlobClient();
                client.RetryPolicy = new LinearRetry(TimeSpan.Zero, 20);
                //client.RetryPolicy = RetryPolicies.Retry(20, TimeSpan.Zero);

                CloudBlobContainer container = client.GetContainerReference(name);
                container.Create();
                BlobContainerPermissions permissions = new BlobContainerPermissions();
                switch (access)
                {
                    case 0: // Private
                        permissions.PublicAccess = BlobContainerPublicAccessType.Off;
                        break;
                    case 1: // Public Blob
                        permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
                        break;
                    case 2: // Public Container
                        permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                        break;
                }
                container.SetPermissions(permissions);
                ReportSuccess("New container " + name + " created");
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
            Refresh();
        }

        #endregion

        #region Copy Container

        public void CopyContainer(string name, string destName, int access)
        {
            try
            {
                ListSpinnerVisible = Visibility.Visible;
                ClearStatus();

                name = NormalizeContainerName(name);
                destName = NormalizeContainerName(destName);

                CloudBlockBlob blob, destBlob;
                CloudBlobClient client = CloudStorageAccount.CreateCloudBlobClient();
                client.RetryPolicy = new LinearRetry(TimeSpan.Zero, 20);
                //client.RetryPolicy = RetryPolicies.Retry(20, TimeSpan.Zero);

                CloudBlobContainer container = client.GetContainerReference(name);
                CloudBlobContainer destContainer = client.GetContainerReference(destName);
                destContainer.CreateIfNotExists();

                BlobContainerPermissions permissions = new BlobContainerPermissions();
                switch (access)
                {
                    case 0: // Private
                        permissions.PublicAccess = BlobContainerPublicAccessType.Off;
                        break;
                    case 1: // Public Blob
                        permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
                        break;
                    case 2: // Public Container
                        permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                        break;
                }
                destContainer.SetPermissions(permissions);

                //BlobRequestOptions options = new BlobRequestOptions();
                //options.UseFlatBlobListing = true;
                string blobName;

                foreach (IListBlobItem blobItem in container.ListBlobs(useFlatBlobListing: true))
                {
                    blob = blobItem as CloudBlockBlob;

                    blobName = BlobDescriptor.BlobName(blob);

                    destBlob = destContainer.GetBlockBlobReference(blobName);
                    destBlob.StartCopyFromBlob(blob);
                }

                ReportSuccess("Container " + name + " copied to new container " + destName);
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
            Refresh();
        }

        #endregion

        #region Rename Container

        public void RenameContainer(string name, string destName, int access)
        {
            try
            {
                ListSpinnerVisible = Visibility.Visible;
                ClearStatus();

                name = NormalizeContainerName(name);
                destName = NormalizeContainerName(destName);

                CloudBlockBlob blob, destBlob;
                CloudBlobClient client = CloudStorageAccount.CreateCloudBlobClient();
                client.RetryPolicy = new LinearRetry(TimeSpan.Zero, 20);
                //client.RetryPolicy = RetryPolicies.Retry(20, TimeSpan.Zero);

                CloudBlobContainer container = client.GetContainerReference(name);
                CloudBlobContainer destContainer = client.GetContainerReference(destName);

                try
                {
                    destContainer.FetchAttributes();
                    ReportError("Cannot rename container - the destination container '" + destName + "' already exists");
                    ListSpinnerVisible = Visibility.Collapsed;
                    return;
                }
                catch (StorageException)
                {
                }
                
                destContainer.Create();

                BlobContainerPermissions permissions = new BlobContainerPermissions();
                switch (access)
                {
                    case 0: // Private
                        permissions.PublicAccess = BlobContainerPublicAccessType.Off;
                        break;
                    case 1: // Public Blob
                        permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
                        break;
                    case 2: // Public Container
                        permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                        break;
                }
                destContainer.SetPermissions(permissions);

                //BlobRequestOptions options = new BlobRequestOptions();
                //options.UseFlatBlobListing = true;
                string blobName;

                foreach (IListBlobItem blobItem in container.ListBlobs(useFlatBlobListing: true))
                {
                    blob = blobItem as CloudBlockBlob;

                    blobName = BlobDescriptor.BlobName(blob);

                    destBlob = destContainer.GetBlockBlobReference(blobName);
                    destBlob.StartCopyFromBlob(blob);

                    ReportSuccess("Container " + name + " renamed to " + destName);
                }

                container.Delete();
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
            Refresh();
        }

        #endregion

        #region Delete Container

        public void DeleteContainer(string name)
        {
            try
            {
                ListSpinnerVisible = Visibility.Visible;
                ClearStatus();

                name = NormalizeContainerName(name);

                CloudBlobClient client = CloudStorageAccount.CreateCloudBlobClient();
                client.RetryPolicy = new LinearRetry(TimeSpan.Zero, 20);
                //client.RetryPolicy = RetryPolicies.Retry(20, TimeSpan.Zero);

                CloudBlobContainer container = client.GetContainerReference(name);

                container.Delete();

                ReportSuccess("Container " + name + " deleted");
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
            Refresh();
        }

        #endregion

        #region Container Access Level

        public BlobContainerPublicAccessType GetContainerAccessLevel(string containerName)
        {
            CloudBlobClient client = CloudStorageAccount.CreateCloudBlobClient();
            client.RetryPolicy = new LinearRetry(TimeSpan.Zero, 20);
            //client.RetryPolicy = RetryPolicies.Retry(20, TimeSpan.Zero);
            CloudBlobContainer container = client.GetContainerReference(containerName);
            BlobContainerPermissions permissions = container.GetPermissions();
            return permissions.PublicAccess;
        }

        public void SetContainerAccessLevel(string containerName, BlobContainerPublicAccessType accessType)
        {
            CloudBlobClient client = CloudStorageAccount.CreateCloudBlobClient();
            client.RetryPolicy = new LinearRetry(TimeSpan.Zero, 20);
            //client.RetryPolicy = RetryPolicies.Retry(20, TimeSpan.Zero);
            CloudBlobContainer container = client.GetContainerReference(containerName);
            BlobContainerPermissions permissions = container.GetPermissions();
            permissions.PublicAccess = accessType;
            container.SetPermissions(permissions);
        }

        #endregion

        #endregion

        #region Blob Actions

        #region New Blob

        public void NewBlockBlob(string containerName, string blobName, string textContent)
        {
            if (CloudStorageAccount == null) return;

            try
            {
                ClearStatus();

                containerName = NormalizeContainerName(containerName);

                CloudBlobClient client = CloudStorageAccount.CreateCloudBlobClient();
                client.RetryPolicy = new LinearRetry(TimeSpan.Zero, 20);
                //client.RetryPolicy = RetryPolicies.Retry(20, TimeSpan.Zero);

                CloudBlobContainer container = client.GetContainerReference(containerName);
                container.CreateIfNotExists();
                CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
                if (textContent == null)
                {
                    MemoryStream stream = new MemoryStream();
                    blob.UploadFromStream(stream);
                    //blob.UploadByteArray(new byte[] { });
                }
                else
                {
                    Encoding encoding = Encoding.UTF8;
                    MemoryStream stream = new MemoryStream(encoding.GetBytes(textContent));
                    blob.UploadFromStream(stream);
                }

                RefreshDetail(containerName);

                ReportSuccess("Block blob " + blobName + " created");
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        public void NewPageBlob(string containerName, string blobName, long size)
        {
            if (CloudStorageAccount == null) return;

            try
            {
                ClearStatus();

                containerName = NormalizeContainerName(containerName);

                CloudBlobClient client = CloudStorageAccount.CreateCloudBlobClient();
                client.RetryPolicy = new LinearRetry(TimeSpan.Zero, 20);
                //client.RetryPolicy = RetryPolicies.Retry(20, TimeSpan.Zero);

                CloudBlobContainer container = client.GetContainerReference(containerName);
                container.CreateIfNotExists();
                CloudPageBlob blob = container.GetPageBlobReference(blobName);
                blob.Create(size);
                RefreshDetail(containerName);

                ReportSuccess("Page blob " + blobName + " created of size " + size.ToString() + " bytes");
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        #endregion

        #region Copy Blob

        public void CopyBlob(string containerName, string sourceBlobName, string destBlobName)
        {
            if (CloudStorageAccount == null) return;

            try
            {
                ClearStatus();
                CloudBlobClient client = CloudStorageAccount.CreateCloudBlobClient();
                client.RetryPolicy = new LinearRetry(TimeSpan.Zero, 20);
                //client.RetryPolicy = RetryPolicies.Retry(20, TimeSpan.Zero);

                CloudBlobContainer container = client.GetContainerReference(containerName);
                if (container != null)
                {
                    CloudBlockBlob blob = container.GetBlockBlobReference(sourceBlobName);
                    CloudBlockBlob newBlob = container.GetBlockBlobReference(destBlobName);
                    newBlob.UploadFromStream(new MemoryStream());
                    //newBlob.UploadByteArray(new byte[] { });
                    newBlob.StartCopyFromBlob(blob);
                }
                RefreshDetail(containerName);
                ReportSuccess("Blob " + sourceBlobName + " copied to " + destBlobName);
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        #endregion

        #region Rename Blob

        public void RenameBlob(string containerName, string sourceBlobName, string destBlobName)
        {
            if (CloudStorageAccount == null) return;

            try
            {
                ClearStatus();
                CloudBlobClient client = CloudStorageAccount.CreateCloudBlobClient();
                client.RetryPolicy = new LinearRetry(TimeSpan.Zero, 20);
                //client.RetryPolicy = RetryPolicies.Retry(20, TimeSpan.Zero);

                CloudBlobContainer container = client.GetContainerReference(containerName);
                if (container != null)
                {
                    CloudBlockBlob blob = container.GetBlockBlobReference(sourceBlobName);
                    CloudBlockBlob newBlob = container.GetBlockBlobReference(destBlobName);
                    newBlob.UploadFromStream(new MemoryStream());
                    //newBlob.UploadByteArray(new byte[] { });
                    newBlob.StartCopyFromBlob(blob);
                    blob.Delete();
                }
                RefreshDetail(containerName);
                ReportSuccess("Blob " + sourceBlobName + " renamed to " + destBlobName);
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        #endregion

        #region Delete Blob

        public void DeleteBlob(string containerName, string blobName)
        {
            try
            {
                DetailSpinnerVisible = Visibility.Visible;
                ClearStatus();

                containerName = NormalizeContainerName(containerName);

                CloudBlobClient client = CloudStorageAccount.CreateCloudBlobClient();
                client.RetryPolicy = new LinearRetry(TimeSpan.Zero, 20);
                //client.RetryPolicy = RetryPolicies.Retry(20, TimeSpan.Zero);

                CloudBlobContainer container = client.GetContainerReference(containerName);
                CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

                blob.Delete();

                ReportSuccess("Blob " + blobName + " deleted");
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
            RefreshDetail(containerName);
        }

        #endregion

        #region Upload Blob

        public bool UploadInProgress { get; internal set; }
        private string[] UploadFileList;
        private string UploadContainerName;
        private CloudBlobContainer UploadContainer;

        public void UploadBlobs(string containerName, string[] filenames)
        {
            if (CloudStorageAccount == null || 
                filenames == null || 
                filenames.Length == 0 ||
                UploadInProgress) 
            return;

            containerName = NormalizeContainerName(containerName);


            if (filenames != null && filenames.Count() == 1)
            {
                ReportActive("Uploading Blob...");
            }
            else
            {
                ReportActive("Uploading Blobs...");
            }

            UploadInProgress = true;

            UploadFileList = filenames;

            DetailSpinnerVisible = Visibility.Visible;

            UploadContainerName = containerName;
            CloudBlobClient client = CloudStorageAccount.CreateCloudBlobClient();
            client.RetryPolicy = new LinearRetry(TimeSpan.Zero, 20);
            //client.RetryPolicy = RetryPolicies.Retry(20, TimeSpan.Zero);

            UploadContainer = client.GetContainerReference(containerName);

            BackgroundWorker background = new BackgroundWorker();
            background.DoWork += new DoWorkEventHandler(Background_UploadBlobs);
            background.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Background_UploadBlobsCompleted);
            background.RunWorkerAsync();
        }

        void Background_UploadBlobs(object sender, DoWorkEventArgs e)
        {
            int pos;
            string blobName;
            int count = 0;
            int errors = 0;
            CloudBlockBlob blob;

            try
            {
                CloudBlobClient client = CloudStorageAccount.CreateCloudBlobClient();
                client.ServerTimeout = new TimeSpan(1, 0, 0);
                //client.WriteBlockSizeInBytes = 4 * 1024 * 1024;
                client.RetryPolicy = new LinearRetry(TimeSpan.Zero, 20);
                //client.RetryPolicy = RetryPolicies.Retry(20, TimeSpan.Zero);

                BlobRequestOptions options = new BlobRequestOptions();
                options.ServerTimeout = new TimeSpan(1, 0, 0);
                options.RetryPolicy = new LinearRetry(TimeSpan.Zero, 20);
                //options.RetryPolicy = RetryPolicies.Retry(20, TimeSpan.Zero);
                
                if (UploadContainer != null)
                {
                    foreach (string filename in UploadFileList)
                    {
                        try
                        {
                            DetailSpinnerVisible = Visibility.Visible;
                            blobName = filename;
                            pos = blobName.LastIndexOf("\\");
                            if (pos != -1)
                            {
                                blobName = blobName.Substring(pos + 1);
                            }
                            blob = UploadContainer.GetBlockBlobReference(blobName);
                            
                            //client.ResponseReceived += client_ResponseReceived; // Not currently used
                            FileStream stream = File.OpenRead(filename);
                            blob.UploadFromStream(stream, options: options);
                            stream.Close();

                            if (ContentTypeMapping.SetContentTypeAutomatically)
                            {
                                string contentType = ContentTypeMapping.GetFileContentType(filename);
                                if (contentType != null)
                                {
                                    blob.Properties.ContentType = contentType;
                                    blob.SetProperties();
                                }
                            }

                            count++;
                        }
                        catch (Exception ex)
                        {
                            if (errors == 0)
                            {
                                ReportException(ex);
                            }
                            errors++;
                        }

                        RefreshDetail(UploadContainerName);
                    }

                    if (errors > 0)
                    {
                        if (count > 0)
                        {
                            ReportWarning("Upload complete, " + numberof(count, "blob", "blobs") + " added, " + numberof(errors, "error", "errors"));
                        }
                        else
                        {
                            ReportError("Upload failed, " + numberof(count, "blob", "blobs") + " added, " + numberof(errors, "error", "errors"));
                        }
                    }
                    else
                    {
                        ReportSuccess("Upload complete, " + numberof(count, "blob", "blobs") + " added");
                    }
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        //void client_ResponseReceived(object sender, ResponseReceivedEventArgs e)
        //{
        //}

        void Background_UploadBlobsCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UploadInProgress = false;
            DetailSpinnerVisible = Visibility.Collapsed;
        }

        #endregion

        #region Download Blobs

        public bool DownloadInProgress { get; internal set; }
        private CloudBlockBlob[] DownloadBlobList;
        private string[] DownloadFileList;
        private string DownloadContainerName;

        public void DownloadBlobs(string containerName, CloudBlockBlob[] blobs, string[] filenames)
        {
            if (CloudStorageAccount == null || blobs == null) return;

            DownloadContainerName = NormalizeContainerName(containerName);
            DownloadBlobList = blobs;
            DownloadFileList = filenames;

            DetailSpinnerVisible = Visibility.Visible;

            if (blobs != null && blobs.Count() == 1)
            {
                ReportActive("Downloading Blob...");
            }
            else
            {
                ReportActive("Downloading Blobs...");
            }

            DownloadInProgress = true;

            BackgroundWorker background = new BackgroundWorker();
            background.DoWork += new DoWorkEventHandler(Background_DownloadBlobs);
            background.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Background_DownloadBlobsCompleted);
            background.RunWorkerAsync();
        }

        void Background_DownloadBlobs(object sender, DoWorkEventArgs e)
        {
            try
            {
                CloudBlockBlob blob;
                string filename;
                int count = DownloadBlobList.Length;

                CloudBlobClient client = CloudStorageAccount.CreateCloudBlobClient();
                client.ServerTimeout = new TimeSpan(1, 0, 0);
                //client.WriteBlockSizeInBytes = 4 * 1024 * 1024;
                client.RetryPolicy = new LinearRetry(TimeSpan.Zero, 20);
                //client.RetryPolicy = RetryPolicies.Retry(20, TimeSpan.Zero);

                BlobRequestOptions options = new BlobRequestOptions();
                options.ServerTimeout = new TimeSpan(1, 0, 0);
                options.RetryPolicy = new LinearRetry(TimeSpan.Zero, 20);
                //options.RetryPolicy = RetryPolicies.Retry(20, TimeSpan.Zero);

                for (int i = 0; i < count; i++)
                {
                    blob = DownloadBlobList[i];
                    filename = DownloadFileList[i];

                    var blobReference = client.GetBlobReferenceFromServer(blob.Uri);

                    FileStream stream = File.OpenWrite(filename);
                    blobReference.DownloadToStream(stream);
                    stream.Close();
                }
                
                ReportSuccess("Download Complete");
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        void Background_DownloadBlobsCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DownloadInProgress = false;
            DetailSpinnerVisible = Visibility.Collapsed;
        }

        #region Shared Access Signatures

        public SharedAccessBlobPolicies GetContainerAccessPolicies(string containerName)
        {
            SharedAccessBlobPolicies policies = new SharedAccessBlobPolicies();

            CloudBlobClient client = CloudStorageAccount.CreateCloudBlobClient();
            client.RetryPolicy = new LinearRetry(TimeSpan.Zero, 20);
            //client.RetryPolicy = RetryPolicies.Retry(20, TimeSpan.Zero);
            CloudBlobContainer container = client.GetContainerReference(containerName);
            BlobContainerPermissions permissions = container.GetPermissions();

            if (permissions != null)
            {
                foreach (KeyValuePair<string, SharedAccessBlobPolicy> policy in permissions.SharedAccessPolicies)
                {
                    policies.Add(policy.Key, policy.Value);
                }
            }

            return policies;
        }

        public void SetContainerAccessPolicies(string containerName, SharedAccessBlobPolicies policies)
        {
            CloudBlobClient client = CloudStorageAccount.CreateCloudBlobClient();
            client.RetryPolicy = new LinearRetry(TimeSpan.Zero, 20);
            //client.RetryPolicy = RetryPolicies.Retry(20, TimeSpan.Zero);
            CloudBlobContainer container = client.GetContainerReference(containerName);
            BlobContainerPermissions permissions = container.GetPermissions();
            permissions.SharedAccessPolicies.Clear();

            if (policies != null)
            {
                foreach (KeyValuePair<string, SharedAccessBlobPolicy> policy in policies)
                {
                    permissions.SharedAccessPolicies.Add(policy.Key, policy.Value);
                }
            }

            container.SetPermissions(permissions);
        }

        public string GenerateSharedAccessSignature(string containerName, string blobName, 
            bool read, bool write, bool delete, bool list, DateTime startTime, DateTime endTime)
        {
            CloudBlobClient client = CloudStorageAccount.CreateCloudBlobClient();
            client.RetryPolicy = new LinearRetry(TimeSpan.Zero, 20);
            //client.RetryPolicy = RetryPolicies.Retry(20, TimeSpan.Zero);
            CloudBlobContainer container = client.GetContainerReference(containerName);

            string path;

            if (string.IsNullOrEmpty(blobName))
            {
                path = container.Uri.AbsoluteUri;
            }
            else
            {
                CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
                path = blob.Uri.AbsoluteUri;
            }

            SharedAccessBlobPermissions permissions = new SharedAccessBlobPermissions();
            if (read) permissions |= SharedAccessBlobPermissions.Read;
            if (write) permissions |= SharedAccessBlobPermissions.Write;
            if (delete) permissions |= SharedAccessBlobPermissions.Delete;
            if (list) permissions |= SharedAccessBlobPermissions.List;

            SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy()
            {
                Permissions = permissions,
                SharedAccessStartTime = startTime,
                SharedAccessExpiryTime = endTime
            };

            string queryString = container.GetSharedAccessSignature(policy);

            return path + queryString;
        }

        public string GenerateSharedAccessSignatureFromPolicy(
            string containerName, string blobName, string policyName)
        {
            CloudBlobClient client = CloudStorageAccount.CreateCloudBlobClient();
            client.RetryPolicy = new LinearRetry(TimeSpan.Zero, 20);
            //client.RetryPolicy = RetryPolicies.Retry(20, TimeSpan.Zero);
            CloudBlobContainer container = client.GetContainerReference(containerName);

            string path;

            if (string.IsNullOrEmpty(blobName))
            {
                path = container.Uri.AbsoluteUri;
            }
            else
            {
                CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
                path = blob.Uri.AbsoluteUri;
            }

            string queryString = container.GetSharedAccessSignature(new SharedAccessBlobPolicy(), policyName);

            return path + queryString;
        }

        #endregion

        #endregion

        #endregion

        #region Queue Actions

        #region New Queue

        public void NewQueue(string name)
        {
            if (CloudStorageAccount == null) return;

            try
            {
                ClearStatus();
                CloudQueueClient client = CloudStorageAccount.CreateCloudQueueClient();
                CloudQueue queue = client.GetQueueReference(name);
                queue.Create();

                Refresh();

                ReportSuccess("Queue " + name + " created");
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        #endregion

        #region Copy Queue

        public void CopyQueue(string name, string destName)
        {
            try
            {
                ListSpinnerVisible = Visibility.Visible;
                ClearStatus();

                CloudQueueClient client = CloudStorageAccount.CreateCloudQueueClient();
                CloudQueue queue = client.GetQueueReference(name);
                CloudQueue destQueue = client.GetQueueReference(destName);
                destQueue.CreateIfNotExists();

                CloudQueueMessage message;
                while ((message = queue.GetMessage(TimeSpan.FromSeconds(5))) != null)
                {
                    destQueue.AddMessage(new CloudQueueMessage(message.AsBytes));
                }

                ReportSuccess("Queue " + name + " copied to " + destName);
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
            Refresh();
        }

        #endregion

        #region Rename Queue

        public void RenameQueue(string name, string destName)
        {
            try
            {
                ListSpinnerVisible = Visibility.Visible;
                ClearStatus();

                CloudQueueClient client = CloudStorageAccount.CreateCloudQueueClient();
                CloudQueue queue = client.GetQueueReference(name);
                CloudQueue destQueue = client.GetQueueReference(destName);

                if (destQueue.Exists())
                {
                    ReportError("Cannot rename queue - the destination queue '" + destName + "' already exists");
                    ListSpinnerVisible = Visibility.Collapsed;
                    return;
                }

                destQueue.Create();

                CloudQueueMessage message;
                while ((message = queue.GetMessage()) != null)
                {
                    destQueue.AddMessage(new CloudQueueMessage(message.AsBytes));
                    queue.DeleteMessage(message);
                }

                queue.Delete();

                ReportSuccess("Queue " + name + " renamed to " + destName);
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
            Refresh();
        }

        #endregion

        #region Delete Queue

        public void DeleteQueue(string name)
        {
            try
            {
                ListSpinnerVisible = Visibility.Visible;
                ClearStatus();

                CloudQueueClient client = CloudStorageAccount.CreateCloudQueueClient();
                CloudQueue queue = client.GetQueueReference(name);

                queue.Delete();

                ReportSuccess("Queue " + name + " deleted");
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
            Refresh();
        }

        #endregion

        #endregion

        #region Message Actions

        public void NewMessage(string queueName, string textContent)
        {
            if (CloudStorageAccount == null) return;

            try
            {
                ClearStatus();
                CloudQueueClient client = CloudStorageAccount.CreateCloudQueueClient();
                CloudQueue queue = client.GetQueueReference(queueName);
                queue.CreateIfNotExists();
                queue.AddMessage(new CloudQueueMessage(textContent));
                RefreshDetail(queueName);

                ReportSuccess("New message queued");
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        public void DeleteMessage(string queueName)
        {
            try
            {
                DetailSpinnerVisible = Visibility.Visible;
                ClearStatus();

                CloudQueueClient client = CloudStorageAccount.CreateCloudQueueClient();
                CloudQueue queue = client.GetQueueReference(queueName);

                CloudQueueMessage message = queue.GetMessage();
                if (message != null)
                {
                    queue.DeleteMessage(message);
                }

                ReportSuccess("Message removed from queue");
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
            RefreshDetail(queueName);
        }

        public void DeleteAllMessages(string queueName)
        {
            CloudQueueMessage message;

            try
            {
                DetailSpinnerVisible = Visibility.Visible;
                ClearStatus();

                CloudQueueClient client = CloudStorageAccount.CreateCloudQueueClient();
                CloudQueue queue = client.GetQueueReference(queueName);

                while ((message = queue.GetMessage()) != null)
                {
                    queue.DeleteMessage(message);
                }

                ReportSuccess("Queue " + queueName + " cleared");
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
            RefreshDetail(queueName);
        }

        #region Upload Message

        private string UploadQueueName;
        private CloudQueue UploadQueue;

        public void UploadMessages(string queueName, string[] filenames)
        {
            if (CloudStorageAccount == null ||
                filenames == null ||
                filenames.Length == 0 ||
                UploadInProgress)
                return;

            UploadInProgress = true;

            UploadFileList = filenames;

            DetailSpinnerVisible = Visibility.Visible;
            ReportActive("Uploading Messages..");

            UploadQueueName = queueName;
            CloudQueueClient client = CloudStorageAccount.CreateCloudQueueClient();
            UploadQueue = client.GetQueueReference(queueName);

            BackgroundWorker background = new BackgroundWorker();
            background.DoWork += new DoWorkEventHandler(Background_UploadMessages);
            background.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Background_UploadMessagesCompleted);
            background.RunWorkerAsync();
        }

        void Background_UploadMessages(object sender, DoWorkEventArgs e)
        {
            try
            {
                int count = 0;
                int errors = 0;

                if (UploadQueue != null)
                {
                    foreach (string filename in UploadFileList)
                    {
                        DetailSpinnerVisible = Visibility.Visible;

                        try
                        {
                            UploadQueue.AddMessage(new CloudQueueMessage(File.ReadAllBytes(filename)));
                            count++;
                        }
                        catch (Exception ex)
                        {
                            if (errors == 0)
                            {
                                ReportException(ex);
                            }
                            errors++;
                        }

                        RefreshDetail(UploadQueueName);
                    }
                }

                if (errors > 0)
                {
                    if (count > 0)
                    {
                        ReportWarning("Upload complete, " + numberof(count, "message", "messages") + " added, " + numberof(errors, "error", "errors"));
                    }
                    else
                    {
                        ReportError("Upload failed, " + numberof(count, "message", "messages") + " added, " + numberof(errors, "error", "errors"));
                    }
                }
                else
                {
                    ReportSuccess("Upload complete, " + numberof(count, "message", "messages") + " added");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        void Background_UploadMessagesCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UploadInProgress = false;
            DetailSpinnerVisible = Visibility.Collapsed;
        }

        #endregion

        #region Download Messages

        private CloudQueueMessage[] DownloadMessageList;

        public void DownloadMessages(string containerName, CloudQueueMessage[] messages, string[] filenames)
        {
            if (CloudStorageAccount == null || messages == null) return;

            DownloadMessageList = messages;
            DownloadFileList = filenames;

            DetailSpinnerVisible = Visibility.Visible;

            ReportActive("Downloading Messages...");

            DownloadInProgress = true;

            BackgroundWorker background = new BackgroundWorker();
            background.DoWork += new DoWorkEventHandler(Background_DownloadMessages);
            background.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Background_DownloadMessagesCompleted);
            background.RunWorkerAsync();
        }

        void Background_DownloadMessages(object sender, DoWorkEventArgs e)
        {
            try
            {
                CloudQueueMessage message;
                string filename;
                int count = DownloadMessageList.Length;

                for (int i = 0; i < count; i++)
                {
                    message = DownloadMessageList[i];
                    filename = DownloadFileList[i];
                    File.WriteAllBytes(filename, message.AsBytes);
                }

                ReportSuccess("Download Complete");
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        void Background_DownloadMessagesCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DownloadInProgress = false;
            DetailSpinnerVisible = Visibility.Collapsed;
        }

        #endregion

        #endregion

        #region Table Actions

        #region New Table

        public void NewTable(string tableName)
        {
            if (CloudStorageAccount == null) return;

            try
            {
                ClearStatus();
                CloudTableClient tableClient = CloudStorageAccount.CreateCloudTableClient();
                var table = tableClient.GetTableReference(tableName);
                table.CreateIfNotExists();

                Refresh();

                ReportSuccess("Table " + tableName + " created.");
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        #endregion

        #region Copy Table

        public void CopyTable(string name, string destName)
        {
            DetailSpinnerVisible = Visibility.Visible;

            SourceTableName = name;
            DestTableName = destName;

            ReportActive("Copying table " + name + " to " + destName + "...");

            UploadInProgress = true;

            BackgroundWorker background = new BackgroundWorker();

            background.DoWork += new DoWorkEventHandler(Background_CopyEntities);

            background.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Background_CopyEntitiesCompleted);
            background.RunWorkerAsync();
        }

        private void Background_CopyEntities(object sender, DoWorkEventArgs e)
        {
            List<GenericEntity> entityList = new List<GenericEntity>();

            if (!OpenAccount()) return;

            try
            {
                CloudTableClient tableClient = CloudStorageAccount.CreateCloudTableClient();
                var table = tableClient.GetTableReference(DestTableName);
                table.CreateIfNotExists();

                TableServiceContext tableServiceContext = CreateTableServiceContext(tableClient);

                TableServiceQuery<GenericEntity> cloudTableQuery = null;
                if (!String.IsNullOrEmpty(TableQuery))
                {
                    cloudTableQuery =
                    (from entity in tableServiceContext.CreateQuery<GenericEntity>(SourceTableName)
                     .AddQueryOption("$filter", TableQuery)
                     select entity).AsTableServiceQuery<GenericEntity>(tableServiceContext);
                }
                else
                {
                    cloudTableQuery =
                    (from entity in tableServiceContext.CreateQuery<GenericEntity>(SourceTableName)
                     select entity).AsTableServiceQuery<GenericEntity>(tableServiceContext);
                }
                IEnumerable<GenericEntity> entities = cloudTableQuery.Execute() as IEnumerable<GenericEntity>;

                // Read entities from source table and add to destination table.

                entityList.Clear();
                foreach (GenericEntity entity in entities)
                {
                    entityList.Add(entity);
                }

                tableClient = CloudStorageAccount.CreateCloudTableClient();
                tableServiceContext = CreateTableServiceContext(tableClient);

                const int batchSize = 10;
                int batchRecords = 0;
                int entitiesCopiedCount = 0;
                foreach (GenericEntity entity in entityList)
                {
                    tableServiceContext.AddObject(DestTableName, new GenericEntity(entity));
                    entitiesCopiedCount++;
                    batchRecords++;
                    if (batchRecords >= batchSize)
                    {
                        tableServiceContext.SaveChanges(SaveChangesOptions.Batch);
                        batchRecords = 0;
                    }
                }
                if (batchRecords > 0)
                {
                    tableServiceContext.SaveChanges(SaveChangesOptions.Batch);
                }

                ReportSuccess("Table copy complete (" + DestTableName + ", " + entitiesCopiedCount.ToString() + " entities)");
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }

        }

        void Background_CopyEntitiesCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UploadInProgress = false;
            DetailSpinnerVisible = Visibility.Collapsed;
            Refresh();
        }

        #endregion

        #region Rename Table

        public void RenameTable(string name, string destName)
        {
            CloudTableClient tableClient = CloudStorageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(destName);

            if (table.Exists())
            {
                ReportError("Cannot rename table - the destination table '" + destName + "' already exists");
                ListSpinnerVisible = Visibility.Collapsed;
                return;
            }
                
            DetailSpinnerVisible = Visibility.Visible;

            SourceTableName = name;
            DestTableName = destName;

            ReportActive("Copying table " + name + " to " + destName + "...");

            UploadInProgress = true;

            BackgroundWorker background = new BackgroundWorker();

            background.DoWork += new DoWorkEventHandler(Background_CopyEntities);

            background.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Background_CopyEntitiesCompleted);
            background.RunWorkerAsync();
        }

        #endregion

        #region Delete Table

        public void DeleteTable(string tableName)
        {
            if (CloudStorageAccount == null) return;

            try
            {
                ClearStatus();
                CloudTableClient tableClient = CloudStorageAccount.CreateCloudTableClient();
                var table = tableClient.GetTableReference(tableName);
                table.Delete();

                Refresh();

                ReportSuccess("Table " + tableName + " deleted.");
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        #endregion

        #endregion

        #region Entity Actions

        public void NewEntity(string tableName, GenericEntity entity)
        {
            if (CloudStorageAccount == null) return;

            try
            {
                ClearStatus();
                CloudTableClient tableClient = CloudStorageAccount.CreateCloudTableClient();
                TableServiceContext tableServiceContext = CreateTableServiceContext(tableClient);

                tableServiceContext.AddObject(tableName, entity);
                tableServiceContext.SaveChanges();

                RefreshDetail(tableName);

                ReportSuccess("New entity added to table");
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        public void UpdateEntity(string tableName, GenericEntity originalEntity, GenericEntity updatedEntity)
        {
            if (CloudStorageAccount == null) return;

            try
            {
                bool keyChanged = true;

                if (originalEntity.PartitionKey == updatedEntity.PartitionKey &&
                    originalEntity.RowKey == updatedEntity.RowKey)
                {
                    keyChanged = false;
                }

                ClearStatus();
                CloudTableClient tableClient = CloudStorageAccount.CreateCloudTableClient();
                TableServiceContext tableServiceContext = CreateTableServiceContext(tableClient);

                IQueryable<GenericEntity> entities = 
                    (from entity in tableServiceContext.CreateQuery<GenericEntity>(tableName)
                     where entity.PartitionKey == originalEntity.PartitionKey && entity.RowKey == originalEntity.RowKey 
                     select entity);
                List<GenericEntity> entitiesList = entities.ToList<GenericEntity>();

                if (entitiesList != null && entitiesList.Count() > 0)
                {
                    GenericEntity readEntity = entitiesList[0];

                    if (keyChanged)
                    {
                        // Key change, so add the new then delete the old.
                        tableServiceContext.AddObject(tableName, updatedEntity);
                        tableServiceContext.DeleteObject(readEntity);
                        tableServiceContext.SaveChanges();
                        ReportSuccess("Entity " + originalEntity.Key() + " updated, new key " + updatedEntity.Key());
                    }
                    else
                    {
                        // Key unchanged, so just update the entity.
                        readEntity.Properties = updatedEntity.Properties;
                        tableServiceContext.UpdateObject(readEntity);
                        tableServiceContext.SaveChanges(SaveChangesOptions.ReplaceOnUpdate);
                        ReportSuccess("Entity " + originalEntity.Key() + " updated, key unchanged");
                    }

                    RefreshDetail(tableName);
                }
                else
                {
                    ReportWarning("Entity " + originalEntity.Key() + " was not found");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        public bool DeleteEntity(string tableName, GenericEntity targetEntity)
        {
            try
            {
                DetailSpinnerVisible = Visibility.Visible;
                ClearStatus();

                CloudTableClient tableClient = CloudStorageAccount.CreateCloudTableClient();
                TableServiceContext tableServiceContext = CreateTableServiceContext(tableClient);

                IQueryable<GenericEntity> entities =
                    (from entity in tableServiceContext.CreateQuery<GenericEntity>(tableName)
                     where entity.PartitionKey == targetEntity.PartitionKey &&
                           entity.RowKey == targetEntity.RowKey 
                     select entity);
                    
                GenericEntity entityToDelete = entities.FirstOrDefault();
                if (entityToDelete != null)
                {
                    tableServiceContext.DeleteObject(entityToDelete);
                    tableServiceContext.SaveChanges();
                }

                ReportSuccess("Entity " + targetEntity.Key() + " deleted");

                return true;
            }
            catch (Exception ex)
            {
                ReportException(ex);
                RefreshDetail(tableName);
                return false;
            }
        }

        public void ReportDeleteEntities(string tableName, int deleted, int errors)
        {
            if (errors == 0)
            {
                if (deleted > 1)
                {
                    ReportSuccess(deleted.ToString() + " entities deleted");
                }
            }
            else
            {
                if (deleted > 1)
                {
                    if (errors > 1)
                    {
                        ReportError("1 entity could not be deleted due to error");
                    }
                    else
                    {
                        ReportError(errors.ToString() + "entities could not be deleted due to error");
                    }
                }
            }

            RefreshDetail(tableName);
        }

        #region Download Entities

        private string DownloadTable;
        private string DownloadFile;
        private string DownloadFormat;
        private bool DownloadOutputColumnHeaderRow = false;
        private bool DownloadIncludeColumnTypes = false;
        private bool DownloadIncludeNullValues = false;

        public void DownloadEntities(string tableName, string format /* csv|xml|atom */, 
                                     bool outputcolumnHeaderRow, bool includeColumnTypes, 
                                     bool includeNullValues,
                                     string filename)
        {
            if (CloudStorageAccount == null) return;

            //tableName = NormalizeTableName(tableName);

            DownloadTable = tableName;
            DownloadFormat = format;
            DownloadFile = filename;

            DownloadOutputColumnHeaderRow = outputcolumnHeaderRow;
            DownloadIncludeColumnTypes = includeColumnTypes;
            DownloadIncludeNullValues = includeNullValues;

            DetailSpinnerVisible = Visibility.Visible;

            ReportActive("Downloading Entities...");

            DownloadInProgress = true;

            BackgroundWorker background = new BackgroundWorker();

            switch (format)
            {
                case "csv":
                default:
                    background.DoWork += new DoWorkEventHandler(Background_DownloadEntitiesCSV);
                    break;
                case "xml":
                    background.DoWork += new DoWorkEventHandler(Background_DownloadEntitiesPlainXML);
                    break;
                case "atom":
                    background.DoWork += new DoWorkEventHandler(Background_DownloadEntitiesAtomPub);
                    break;
            }
            
            background.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Background_DownloadEntitiesCompleted);
            background.RunWorkerAsync();
        }

        // Download entities in Comma-Separated Values (CSV) format.

        void Background_DownloadEntitiesCSV(object sender, DoWorkEventArgs e)
        {
            TextWriter tw = File.CreateText(DownloadFile);

            TableColumnNames.Clear();

            List<GenericEntity> entityNodes = new List<GenericEntity>();

            if (!OpenAccount()) return;

            try
            {
                CloudTableClient tableClient = CloudStorageAccount.CreateCloudTableClient();
                TableServiceContext tableServiceContext = CreateTableServiceContext(tableClient);

                IQueryable<GenericEntity> entities = null;
                entities = (from entity in tableServiceContext.CreateQuery<GenericEntity>(DownloadTable) select entity);

                int col = 0;
                int row = 0;
                foreach (GenericEntity entity in entities)
                {
                    // Write header row.

                    if (DownloadOutputColumnHeaderRow && row == 0)
                    {
                        tw.Write("PartitionKey,RowKey,Timestamp");

                        col = 0;
                        foreach (KeyValuePair<string, Column> column in TableColumnNames)
                        {
                            tw.Write(",");
                            tw.Write(q(column.Key));

                            if (DownloadIncludeColumnTypes &&
                                column.Value.Type != "string" && 
                                !String.IsNullOrEmpty(column.Value.Type))
                            {
                                // If the column is not of type string, append <name> with :<type>.
                                tw.Write(":" + column.Value.Type);
                            }

                            col++;
                        }
                        tw.WriteLine();
                    }

                    // Write data row.

                    col = 0;
                    object obj;
                    tw.Write(entity.PartitionKey);
                    tw.Write(",");
                    tw.Write(entity.RowKey);
                    tw.Write(",");
                    tw.Write(entity.Timestamp);

                    foreach (KeyValuePair<string, Column> column in TableColumnNames)
                    {
                        tw.Write(",");
                        if (entity.Properties.TryGetValue(column.Key, out obj) && obj != null)
                        {
                            tw.Write(q(obj.ToString()));
                        }
                        else
                        {
                            if (DownloadIncludeNullValues)
                            {
                                tw.Write("null");
                            }
                        }
                        col++;
                    }

                    tw.WriteLine();
                    row++;
                }

                tw.Close();

                ReportSuccess("Download Complete");
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        // Download entities in Plain XML format.

        void Background_DownloadEntitiesPlainXML(object sender, DoWorkEventArgs e)
        {
            XmlWriter xw = XmlWriter.Create(File.CreateText(DownloadFile));
            
            xw.WriteStartDocument();
            
            xw.WriteStartElement(DownloadTable);

            TableColumnNames.Clear();

            List<GenericEntity> entityNodes = new List<GenericEntity>();

            if (!OpenAccount()) return;

            try
            {
                CloudTableClient tableClient = CloudStorageAccount.CreateCloudTableClient();
                TableServiceContext tableServiceContext = CreateTableServiceContext(tableClient);

                IQueryable<GenericEntity> entities = null;
                entities = (from entity in tableServiceContext.CreateQuery<GenericEntity>(DownloadTable) select entity);

                int col = 0;
                int row = 0;
                foreach (GenericEntity entity in entities)
                {
                    col = 0;
                    object obj;

                    xw.WriteStartElement("entity");

                    xw.WriteStartElement("PartitionKey");
                    xw.WriteValue(entity.PartitionKey);
                    xw.WriteEndElement();

                    xw.WriteStartElement("RowKey");
                    xw.WriteValue(entity.RowKey);
                    xw.WriteEndElement();

                    xw.WriteStartElement("Timestamp");
                    xw.WriteValue(entity.Timestamp);
                    xw.WriteEndElement();

                    foreach (KeyValuePair<string, Column> column in TableColumnNames)
                    {
                        if (entity.Properties.TryGetValue(column.Key, out obj) && obj != null)
                        {
                            xw.WriteStartElement(column.Value.Name);

                            if (column.Value.Type != "string")
                            {
                                xw.WriteStartAttribute("type");
                                xw.WriteValue(column.Value.Type);
                                xw.WriteEndAttribute();
                            }

                            xw.WriteValue(obj.ToString());

                            xw.WriteEndElement();
                        }
                        else
                        {
                            if (DownloadIncludeNullValues)
                            {
                                xw.WriteStartElement(column.Value.Name);

                                if (column.Value.Type != "string")
                                {
                                    xw.WriteStartAttribute("type");
                                    xw.WriteValue(column.Value.Type);
                                    xw.WriteEndAttribute();
                                }

                                xw.WriteStartAttribute("null");
                                xw.WriteValue(true);
                                xw.WriteEndAttribute();

                                xw.WriteEndElement();
                            }
                        }

                        col++;
                    }

                    xw.WriteEndElement();

                    row++;
                }

                xw.WriteEndElement();

                xw.WriteEndDocument();
                xw.Close();

                ReportSuccess("Download Complete");
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        // Download entities in AtomPub XML format.

        //<?xml version="1.0" encoding="utf-8" standalone="yes"?>
        //<entry xmlns:d="http://schemas.microsoft.com/ado/2007/08/dataservices" xmlns:m="http://schemas.microsoft.com/ado/2007/08/dataservices/metadata" xmlns="http://www.w3.org/2005/Atom">
        //  <title />
        //  <updated>2008-09-18T23:46:19.3857256Z<updated/>
        //  <author>
        //    <name />
        //  </author>
        //  <id />
        //  <content type="application/xml">
        //    <m:properties>
        //      <d:Address>Mountain View</d:Address>
        //      <d:Age m:type="Edm.Int32">23</d:Age>
        //      <d:AmountDue m:type="Edm.Double">200.23</d:AmountDue>
        //      <d:BinaryData m:type="Edm.Binary" m:null="true" />
        //      <d:CustomerCode m:type="Edm.Guid">c9da6455-213d-42c9-9a79-3e9149a57833</d:CustomerCode>
        //      <d:CustomerSince m:type="Edm.DateTime">2008-07-10T00:00:00</d:CustomerSince>
        //      <d:IsActive m:type="Edm.Boolean">true</d:IsActive>
        //      <d:NumOfOrders m:type="Edm.Int64">255</d:NumOfOrders>
        //      <d:PartitionKey>mypartitionkey</d:PartitionKey>
        //      <d:RowKey>myrowkey1</d:RowKey>
        //      <d:Timestamp m:type="Edm.DateTime">0001-01-01T00:00:00</d:Timestamp>
        //    </m:properties>
        //  </content>
        //</entry>

        void Background_DownloadEntitiesAtomPub(object sender, DoWorkEventArgs e)
        {
            string def = "http://www.w3.org/2005/Atom";
            string b = "http://myaccount.tables.core.windows.net/";
            string m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
            string d = "http://schemas.microsoft.com/ado/2007/08/dataservices";

            XmlWriter xw = XmlWriter.Create(File.CreateText(DownloadFile));

            xw.WriteStartDocument();

            xw.WriteStartElement("feed", def);
            xw.WriteAttributeString("xmlns", "base", null, b);
            xw.WriteAttributeString("xmlns", "d", null, d);
            xw.WriteAttributeString("xmlns", "m", null, m);

            TableColumnNames.Clear();

            List<GenericEntity> entityNodes = new List<GenericEntity>();

            if (!OpenAccount()) return;

            try
            {
                CloudTableClient tableClient = CloudStorageAccount.CreateCloudTableClient();
                TableServiceContext tableServiceContext = CreateTableServiceContext(tableClient);

                IQueryable<GenericEntity> entities = null;
                entities = (from entity in tableServiceContext.CreateQuery<GenericEntity>(DownloadTable) select entity);

                int col = 0;
                int row = 0;
                foreach (GenericEntity entity in entities)
                {
                    col = 0;
                    object obj;

                    xw.WriteStartElement("entry");

                    xw.WriteStartElement("content");
                    xw.WriteAttributeString("type", "application/xml");

                    xw.WriteStartElement("properties", m);

                    xw.WriteStartElement("PartitionKey", d);
                    xw.WriteValue(entity.PartitionKey);
                    xw.WriteEndElement();

                    xw.WriteStartElement("RowKey", d);
                    xw.WriteValue(entity.RowKey);
                    xw.WriteEndElement();

                    xw.WriteStartElement("Timestamp", d);
                    xw.WriteValue(entity.Timestamp);
                    xw.WriteEndElement();

                    foreach (KeyValuePair<string, Column> column in TableColumnNames)
                    {
                        if (entity.Properties.TryGetValue(column.Key, out obj) && obj != null)
                        {
                            xw.WriteStartElement(column.Value.Name, d);

                            xw.WriteStartAttribute("type", m);
                            xw.WriteValue(Column.EdmTypeName(column.Value.Type));
                            xw.WriteEndAttribute();

                            xw.WriteValue(obj.ToString());
                            xw.WriteEndElement();
                        }
                        else
                        {
                            if (DownloadIncludeNullValues)
                            {
                                xw.WriteStartElement(column.Value.Name, d);

                                xw.WriteStartAttribute("null", m);
                                xw.WriteValue(true);
                                xw.WriteEndAttribute();

                                xw.WriteEndElement();
                            }
                        }

                        col++;
                    }

                    xw.WriteEndElement();
                    xw.WriteEndElement();
                    xw.WriteEndElement();

                    row++;
                }

                xw.WriteEndElement();

                xw.WriteEndDocument();
                xw.Close();

                ReportSuccess("Download Complete");
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        void Background_DownloadEntitiesCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DownloadInProgress = false;
            DetailSpinnerVisible = Visibility.Collapsed;
        }

        #endregion

        #region Upload Entities

        private string UploadTable;
        private string UploadFormat;
        private string UploadFile;
        private bool UploadColumnHeaderRow = true;

        public void UploadEntities(string filename, string format, bool optionColumnHeaderRow, string tableName)
        {
            if (CloudStorageAccount == null) return;

            //tableName = NormalizeTableName(tableName);

            UploadTable = tableName;
            UploadFormat = format;
            UploadFile = filename;

            UploadColumnHeaderRow = optionColumnHeaderRow;

            DetailSpinnerVisible = Visibility.Visible;

            ReportActive("Uploading Entities...");

            UploadInProgress = true;

            BackgroundWorker background = new BackgroundWorker();

            switch (format)
            {
                case "csv":
                default:
                    background.DoWork += new DoWorkEventHandler(Background_UploadEntitiesCSV);
                    break;
                case "xml":
                    background.DoWork += new DoWorkEventHandler(Background_UploadEntitiesPlainXML);
                    break;
                case "atom":
                    background.DoWork += new DoWorkEventHandler(Background_UploadEntitiesAtomPub);
                    break;
            }

            background.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Background_UploadEntitiesCompleted);
            background.RunWorkerAsync();
        }

        // Upload entities from a Comma-Separated Values (CSV) file.

        void Background_UploadEntitiesCSV(object sender, DoWorkEventArgs e)
        {
            try
            {
                int count = 0;
                int errors = 0;

                CloudTableClient tableClient = CloudStorageAccount.CreateCloudTableClient();
                TableServiceContext tableServiceContext = CreateTableServiceContext(tableClient);
                var table = tableClient.GetTableReference(UploadTable);
                table.CreateIfNotExists();

                System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

                TextReader tr = File.OpenText(UploadFile);

                List<Column> columns = new List<Column>();
                string line;

                if (UploadColumnHeaderRow)
                {
                    line = tr.ReadLine();

                    string[] columnItems;

                    foreach (string columnDef in r.Split(line))
                    {
                        columnItems = columnDef.Split(':');
                        switch (columnItems.Length)
                        {
                            case 1:
                                columns.Add(new Column(columnItems[0]));
                                break;
                            case 2:
                                columns.Add(new Column(columnItems[0], columnItems[1], null));
                                break;
                        }
                    }
                }

                int col;
                int row = 1;
                string value;
                string[] values;

                GenericEntity entity;
                while ((line = tr.ReadLine()) != null)
                {
                    try
                    {
                        values = r.Split(line);

                        entity = new GenericEntity();

                        if (UploadColumnHeaderRow)
                        {
                            col = 0;
                            foreach (Column columnDef in columns)
                            {
                                if (col < values.Length)
                                {
                                    switch (columnDef.Name)
                                    {
                                        case "PartitionKey":
                                            entity.PartitionKey = values[col];
                                            break;
                                        case "RowKey":
                                            entity.RowKey = values[col];
                                            break;
                                        //case "Timestamp":
                                        //    break;
                                        default:
                                            value = values[col];
                                            if (value == "null")
                                            {
                                                entity.Properties[columnDef.Name] = null;
                                            }
                                            else
                                            {
                                                if (value.StartsWith("\"") && value.EndsWith("\""))
                                                {
                                                    if (value.Length <= 2)
                                                    {
                                                        value = String.Empty;
                                                    }
                                                    else
                                                    {
                                                        value = value.Substring(1, value.Length - 2);
                                                    }
                                                }
                                                entity.Properties[columnDef.Name] = Column.ConvertToStandardType(value, columnDef.Type);
                                            }
                                            break;
                                    }
                                }
                                col++;
                            }
                        }
                        else
                        {
                            // No defined columns - generate field names. 

                            entity.PartitionKey = String.Empty;
                            entity.RowKey = row.ToString();
                            row++;

                            col = 1;
                            string fieldName;
                            foreach (string fieldValue in values)
                            {
                                fieldName = "Field" + col.ToString();
                                entity[fieldName] = fieldValue;
                                col++;
                            }
                        }

                        tableServiceContext.AddObject(UploadTable, entity);
                        tableServiceContext.SaveChanges();
                        count++;
                    }
                    catch (Exception ex)
                    {
                        if (errors == 0)
                        {
                            ReportException(ex);
                        }

                        tableServiceContext = CreateTableServiceContext(tableClient);

                        errors++;
                    }
                }

                RefreshDetail(UploadTable);

                if (errors > 0)
                {
                    if (count > 0)
                    {
                        ReportWarning("Upload complete, " + numberof(count, "entity", "entities") + " added, " + numberof(errors, "error", "errors"));
                    }
                    else
                    {
                        ReportError("Upload failed, " + numberof(count, "entity", "entities") + " added, " + numberof(errors, "error", "errors"));
                    }
                }
                else
                {
                    ReportSuccess("Upload complete, " + numberof(count, "entity", "entities") + " added");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        // Upload entities from a plain XML file. Take all children of the root to be entities.

        void Background_UploadEntitiesPlainXML(object sender, DoWorkEventArgs e)
        {
            try
            {
                int count = 0;
                int errors = 0;

                bool havePartitionKey = false;
                bool haveRowKey = false;

                CloudTableClient tableClient = CloudStorageAccount.CreateCloudTableClient();
                TableServiceContext tableServiceContext = CreateTableServiceContext(tableClient);
                var table = tableClient.GetTableReference(UploadTable);
                table.CreateIfNotExists();

                XDocument doc = XDocument.Parse(File.ReadAllText(UploadFile));

                XElement rootNode = doc.Root;

                List<XElement> entities = new List<XElement>(from XElement ent in rootNode.Elements() select ent);

                GenericEntity entity;
                string name;
                XAttribute type;
                object value;
                int row = 1;

                foreach (XElement en in entities)
                {
                    havePartitionKey = false;
                    haveRowKey = false;

                    entity = new GenericEntity();

                    foreach (XElement prop in en.Elements())
                    {
                        switch (prop.Name.LocalName)
                        {
                            case "PartitionKey":
                                entity.PartitionKey = prop.Value;
                                havePartitionKey = true;
                                break;
                            case "RowKey":
                                entity.RowKey = prop.Value;
                                haveRowKey = true;
                                break;
                            default:
                                name = prop.Name.LocalName;
                                value = prop.Value;
                                type = prop.Attribute("type");
                                if (prop.Attribute("null") != null && Convert.ToBoolean(prop.Attribute("null").Value))
                                {
                                    value = null;
                                }
                                else
                                {
                                    if (type != null)
                                    {
                                        value = Column.ConvertToStandardType(prop.Value, type.Value);   
                                    }
                                }
                                entity[name] = value;
                                break;
                        }
                    }

                    if (!havePartitionKey)
                    {
                        entity.PartitionKey = string.Empty;
                    }

                    if (!haveRowKey)
                    {
                        entity.RowKey = row.ToString();
                    }

                    try
                    {
                        tableServiceContext.AddObject(UploadTable, entity);
                        tableServiceContext.SaveChanges();
                        count++;
                    }
                    catch (Exception ex)
                    {
                        if (errors == 0)
                        {
                            ReportException(ex);
                        }

                        tableServiceContext = CreateTableServiceContext(tableClient);

                        errors++;
                    }

                    row++;
                }

                RefreshDetail(UploadTable);

                if (errors > 0)
                {
                    if (count > 0)
                    {
                        ReportWarning("Upload complete, " + numberof(count, "entity", "entities") + " added, " + numberof(errors, "error", "errors"));
                    }
                    else
                    {
                        ReportError("Upload failed, " + numberof(count, "entity", "entities") + " added, " + numberof(errors, "error", "errors"));
                    }
                }
                else
                {
                    ReportSuccess("Upload complete, " + numberof(count, "entity", "entities") + " added");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        // Upload entities from an AtomPub XML file. Take all children of the root to be entities.

        void Background_UploadEntitiesAtomPub(object sender, DoWorkEventArgs e)
        {
            try
            {
                int count = 0;
                int errors = 0;

                bool havePartitionKey = false;
                bool haveRowKey = false;

                XName typeAttrName = XName.Get("type", AstoriaMetadataNamespace.NamespaceName);
                XName nullAttrName = XName.Get("null", AstoriaMetadataNamespace.NamespaceName);

                CloudTableClient tableClient = CloudStorageAccount.CreateCloudTableClient();
                TableServiceContext tableServiceContext = CreateTableServiceContext(tableClient);
                var table = tableClient.GetTableReference(UploadTable);
                table.CreateIfNotExists();

                XDocument doc = XDocument.Parse(File.ReadAllText(UploadFile));

                XElement rootNode = doc.Root;

                List<XElement> entities = new List<XElement>(from XElement ent in rootNode.Elements() select ent);

                GenericEntity entity;
                string name;
                XAttribute typeAttr, nullAttr;
                object value;
                int row = 1;
                XElement content, properties;

                foreach (XElement entry in entities)
                {
                    havePartitionKey = false;
                    haveRowKey = false;

                    entity = new GenericEntity();

                    if (entry != null && entry.Name.LocalName == "entry")
                    {
                        content = entry.Element(AtomNamespace + "content");
                        if (content != null && content.Name.LocalName == "content")
                        {
                            properties = content.Element(AstoriaMetadataNamespace + "properties");
                            if (properties != null)
                            {
                                foreach (XElement prop in properties.Elements())
                                {
                                    switch (prop.Name.LocalName)
                                    {
                                        case "PartitionKey":
                                            entity.PartitionKey = prop.Value;
                                            havePartitionKey = true;
                                            break;
                                        case "RowKey":
                                            entity.RowKey = prop.Value;
                                            haveRowKey = true;
                                            break;
                                        default:
                                            name = prop.Name.LocalName;
                                            value = prop.Value;
                                            typeAttr = prop.Attribute(typeAttrName);
                                            nullAttr = prop.Attribute(nullAttrName);
                                            if (nullAttr != null &&
                                                Convert.ToBoolean(prop.Attribute(nullAttrName).Value))
                                            {
                                                value = null;
                                            }
                                            else
                                            {
                                                if (typeAttr != null)
                                                {
                                                    value = Column.ConvertToStandardType(prop.Value, typeAttr.Value);
                                                }
                                            }
                                            entity[name] = value;
                                            break;
                                    }
                                }

                                if (!havePartitionKey)
                                {
                                    entity.PartitionKey = string.Empty;
                                }

                                if (!haveRowKey)
                                {
                                    entity.RowKey = row.ToString();
                                }

                                try
                                {
                                    tableServiceContext.AddObject(UploadTable, entity);
                                    tableServiceContext.SaveChanges();
                                    count++;
                                }
                                catch (Exception ex)
                                {
                                    if (errors == 0)
                                    {
                                        ReportException(ex);
                                    }

                                    tableServiceContext = CreateTableServiceContext(tableClient);

                                    errors++;
                                }

                                row++;
                            }
                        }
                    }
                }

                RefreshDetail(UploadTable);

                if (errors > 0)
                {
                    if (count > 0)
                    {
                        ReportWarning("Upload complete, " + numberof(count, "entity", "entities") + " added, " + numberof(errors, "error", "errors"));
                    }
                    else
                    {
                        ReportError("Upload failed, " + numberof(count, "entity", "entities") + " added, " + numberof(errors, "error", "errors"));
                    }
                }
                else
                {
                    ReportSuccess("Upload complete, " + numberof(count, "entity", "entities") + " added");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }
        }

        void Background_UploadEntitiesCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UploadInProgress = false;
            DetailSpinnerVisible = Visibility.Collapsed;
        }

        #endregion

        #region Delete Entities

        public void DeleteEntities(string selectedTableName, string query)
        {
            if (!TablesSelected) return;

            if (string.IsNullOrEmpty(query))
            {
                ReportActive("Deleting all entities...");
            }
            else
            {
                ReportActive("Deleting matching entities...");
            }

            SelectedTableName = selectedTableName;

            if (query != null)
            {
                query = query.Replace("\"", "'");
                query = query.Replace(" = ", " eq ");
                query = query.Replace(" == ", " eq ");
                query = query.Replace(" <> ", " ne ");
                query = query.Replace(" != ", " ne ");
                query = query.Replace(" <= ", " le ");
                query = query.Replace(" < ", " lt ");
                query = query.Replace(" >= ", " ge ");
                query = query.Replace(" > ", " gt ");
            }
            TableQuery = query;

            TableMaxRecords = -1;
            DetailSpinnerVisible = Visibility.Visible;
            BackgroundWorker background = new BackgroundWorker();
            background.DoWork += new DoWorkEventHandler(Background_DeleteEntities);
            background.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Background_DeleteEntitiesCompleted);
            background.RunWorkerAsync();
        }

        private void Background_DeleteEntities(object sender, DoWorkEventArgs e)
        {
            ClearItemSummary();

            TableColumnNames.Clear();

            if (!OpenAccount()) return;

            try
            {
                CloudTableClient tableClient = CloudStorageAccount.CreateCloudTableClient();
                TableServiceContext tableServiceContext = CreateTableServiceContext(tableClient);

                IQueryable<GenericEntity> entities = null;
                entities = (from entity in tableServiceContext.CreateQuery<GenericEntity>(SelectedTableName) select entity);

                if (!String.IsNullOrEmpty(TableQuery))
                {
                    entities = (from entity in tableServiceContext.CreateQuery<GenericEntity>(SelectedTableName)
                                    .AddQueryOption("$filter", TableQuery)
                                select entity);
                }

                int deleteCount = 0;
                foreach (GenericEntity entity in entities)
                {
                    tableServiceContext.DeleteObject(entity);
                    tableServiceContext.SaveChanges();
                    deleteCount++;
                }

                if (deleteCount == 1)
                {
                    ReportSuccess("1 entity deleted");
                }
                else
                {
                    ReportSuccess(deleteCount.ToString() + " entities deleted");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }

        }

        void Background_DeleteEntitiesCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DetailSpinnerVisible = Visibility.Collapsed;
        }

        #endregion

        #endregion

        #region Storage Support Methods

        #region Storage Account Methods

        // Open storage account.

        public bool OpenAccount()
        {
            if (CloudStorageAccount == null)
            {
                try
                {
                    if (Account.Name == "DevStorage")
                    {
                        CloudStorageAccount = CloudStorageAccount.DevelopmentStorageAccount;
                    }
                    else
                    {
                        CloudStorageAccount = new CloudStorageAccount(
                            new StorageCredentials(Account.Name, Account.Key), Account.UseHttps);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    ReportException(ex);
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region Blob Support Methods

        // Retrieve and display blob containers,

        private void LoadContainers()
        {
            if (!OpenAccount()) return;

            if (BlobsLoaded) return;
            
            Containers.Clear();

            try
            {
                CloudBlobClient blobClient = CloudStorageAccount.CreateCloudBlobClient();
                blobClient.RetryPolicy = new LinearRetry(TimeSpan.Zero, 20);
                //blobClient.RetryPolicy = RetryPolicies.Retry(20, TimeSpan.Zero);
                
                Containers = new List<CloudBlobContainer>(blobClient.ListContainers());

                BlobsLoaded = true;
            }
            catch (Exception ex)
            {
                ViewBlobErrors++;
                ReportException(ex);
            }
        }

        // Retrieve and display blob containers,

        private void LoadBlobItems(string containerName)
        {
            List<BlobDescriptor> blobItemNodes = new List<BlobDescriptor>();

            ClearItemSummary();

            if (!OpenAccount()) return;

            try
            {
                containerName = NormalizeContainerName(containerName);

                CloudBlobClient blobClient = CloudStorageAccount.CreateCloudBlobClient();
                blobClient.RetryPolicy = new LinearRetry(TimeSpan.Zero, 20);
                //blobClient.RetryPolicy = RetryPolicies.Retry(20, TimeSpan.Zero);

                CloudBlobContainer container = blobClient.GetContainerReference(containerName);

                blobItemNodes.Clear();
                foreach (IListBlobItem blob in container.ListBlobs(useFlatBlobListing: true, blobListingDetails: BlobListingDetails.All))
                {
                    if (blob is CloudBlockBlob)
                    {
                         blobItemNodes.Add(new BlobDescriptor(blob as CloudBlockBlob));
                    }
                }

                if (blobItemNodes.Count == 1)
                {
                    ReportItemSummary("1 blob");
                }
                else
                {
                    ReportItemSummary(blobItemNodes.Count.ToString() + " blobs");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }

            lock (BlobItemNodes)
            {
                BlobItemNodes.Clear();
                foreach (BlobDescriptor bd in blobItemNodes)
                {
                    BlobItemNodes.Add(bd);
                }
            }
        }

        #endregion

        #region Queue Support Methods

        // Retrieve and display queues

        private void LoadQueues()
        {
            if (!OpenAccount()) return;

            if (QueuesLoaded) return;

            Queues.Clear();

            try
            {
                CloudQueueClient queueClient = CloudStorageAccount.CreateCloudQueueClient();
                Queues = new List<CloudQueue>(queueClient.ListQueues());
                QueuesLoaded = true;
            }
            catch (Exception ex)
            {
                ViewQueuesErrors++;
                ReportException(ex);
            }
        }

        private void LoadMessages(string queueName)
        {
            const int MaxMessages = 32;

            List<CloudQueueMessage> messageNodes = new List<CloudQueueMessage>();

            if (!OpenAccount()) return;

            try
            {
                ClearItemSummary();

                CloudQueueClient queueClient = CloudStorageAccount.CreateCloudQueueClient();
                CloudQueue queue = queueClient.GetQueueReference(queueName);

                messageNodes.Clear();
                foreach (CloudQueueMessage message in queue.PeekMessages(MaxMessages))
                {
                    messageNodes.Add(message);
                }

                if (messageNodes.Count == 1)
                {
                    ReportItemSummary("1 message");
                }
                else
                {
                    ReportItemSummary(messageNodes.Count.ToString() + " messages");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }

            lock (MessageNodes)
            {
                MessageNodes.Clear();
                foreach (CloudQueueMessage messageItem in messageNodes)
                {
                    MessageNodes.Add(messageItem);
                }
            }
        }

        #endregion

        #region Table Support Methods

        // Retrieve and display tables

        private void LoadTables()
        {
            if (!OpenAccount()) return;

            if (TablesLoaded) return;

            Tables.Clear();

            try
            {
                CloudTableClient tableClient = CloudStorageAccount.CreateCloudTableClient();
                Tables = new List<string>(tableClient.ListTables().Select(t => t.Name));
                TablesLoaded = true;
            }
            catch (Exception ex)
            {
                ViewTablesErrors++;
                ReportException(ex);
            }
        }

        // Retrieve and display entities.

        private void LoadEntities(string tableName)
        {
            ClearItemSummary();

            TableColumnNames.Clear();

            List<GenericEntity> entityNodes = new List<GenericEntity>();

            if (!OpenAccount()) return;

            try
            {
                CloudTableClient tableClient = CloudStorageAccount.CreateCloudTableClient();
                TableServiceContext tableServiceContext = CreateTableServiceContext(tableClient);
                tableServiceContext.IgnoreMissingProperties = true; 
                TableServiceQuery<GenericEntity> cloudTableQuery = null;
                if (!String.IsNullOrEmpty(TableQuery))
                {
                    cloudTableQuery =
                    (from entity in tableServiceContext.CreateQuery<GenericEntity>(tableName)
                     .AddQueryOption("$filter", TableQuery)
                     select entity).AsTableServiceQuery<GenericEntity>(tableServiceContext);
                }
                else
                {
                    cloudTableQuery =
                    (from entity in tableServiceContext.CreateQuery<GenericEntity>(tableName)
                     select entity).AsTableServiceQuery<GenericEntity>(tableServiceContext);
                }
                IEnumerable<GenericEntity> entities = cloudTableQuery.Execute() as IEnumerable<GenericEntity>;

                int records = 0;
                entityNodes.Clear();
                foreach (GenericEntity entity in entities)
                {
                    if (TableMaxRecords > -1 && records >= TableMaxRecords) break;
                    entityNodes.Add(entity);
                    records++;
                }

                if (entityNodes.Count == 1)
                {
                    ReportItemSummary("1 entity");
                }
                else
                {
                    ReportItemSummary(entityNodes.Count.ToString() + " entities");
                }
            }
            catch (Exception ex)
            {
                ReportException(ex);
            }

            lock (EntityNodes)
            {
                EntityNodes.Clear();
                foreach (GenericEntity ge in entityNodes)
                {
                    EntityNodes.Add(ge);
                }
            }
        }

        public Type ResolveEntityType(String name)
        {
            return typeof(GenericEntity);
        }

        // Credit goes to Pablo from ADO.NET Data Service team 

        public void OnReadingEntity(object sender, ReadingWritingEntityEventArgs args)
        {
            XNamespace AtomNamespace = "http://www.w3.org/2005/Atom";
            XNamespace AstoriaDataNamespace = "http://schemas.microsoft.com/ado/2007/08/dataservices";
            XNamespace AstoriaMetadataNamespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

            GenericEntity entity = args.Entity as GenericEntity;
            if (entity == null)
            {
                return;
            }

            // read each property, type and value in the payload   
            var properties = args.Entity.GetType().GetProperties();
            var q = from p in args.Data.Element(AtomNamespace + "content")
                                    .Element(AstoriaMetadataNamespace + "properties")
                                    .Elements()
                    where properties.All(pp => pp.Name != p.Name.LocalName)
                    select new
                    {
                        Name = p.Name.LocalName,
                        IsNull = string.Equals("true", p.Attribute(AstoriaMetadataNamespace + "null") == null ? null : p.Attribute(AstoriaMetadataNamespace + "null").Value, StringComparison.OrdinalIgnoreCase),
                        TypeName = p.Attribute(AstoriaMetadataNamespace + "type") == null ? null : p.Attribute(AstoriaMetadataNamespace + "type").Value,
                        p.Value
                    };

            foreach (var dp in q)
            {
                entity[dp.Name] = GetTypedEdmValue(dp.TypeName, dp.Value, dp.IsNull);

                if (!TableColumnNames.ContainsKey(dp.Name))
                {
                    TableColumnNames.Add(dp.Name, new Column(dp.Name, dp.TypeName, dp.Value));
                }
            }
        }

        void OnWritingEntity(object sender, System.Data.Services.Client.ReadingWritingEntityEventArgs e)
        {

            XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

            XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";

            XElement properties = e.Data.Descendants(m + "properties").First();

            GenericEntity entity = e.Entity as GenericEntity;

            if (entity != null)
            {

                foreach (string key in entity.Properties.Keys)
                {

                    object value = entity.Properties[key];

                    if (value != null)
                    {
                        Type type = value.GetType();
                        XElement property = new XElement(d + key, value);
                        this.MapType(m, type, property);
                        properties.Add(property);
                    }
                    else
                    {
                        XElement property = new XElement(d + key);
                        property.Add(new XAttribute(m + "null", "true"));
                        properties.Add(property);
                    }
                }

            }

        }

        private void MapType(XNamespace m, Type propertyType, XElement property)
        {

            if (typesMapping.ContainsKey(propertyType))
            {

                property.Add(new XAttribute(m + "type", typesMapping[propertyType]));

            }

        }

        private static object GetTypedEdmValue(string type, string value, bool isnull)
        {
            if (isnull) return null;

            if (string.IsNullOrEmpty(type)) return value;

            switch (type)
            {
                case "Edm.String": return value;
                case "Edm.Byte": return Convert.ChangeType(value, typeof(byte));
                case "Edm.SByte": return Convert.ChangeType(value, typeof(sbyte));
                case "Edm.Int16": return Convert.ChangeType(value, typeof(short));
                case "Edm.Int32": return Convert.ChangeType(value, typeof(int));
                case "Edm.Int64": return Convert.ChangeType(value, typeof(long));
                case "Edm.Double": return Convert.ChangeType(value, typeof(double));
                case "Edm.Single": return Convert.ChangeType(value, typeof(float));
                case "Edm.Boolean": return Convert.ChangeType(value, typeof(bool));
                case "Edm.Decimal": return Convert.ChangeType(value, typeof(decimal));
                case "Edm.DateTime": return XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.RoundtripKind);
                case "Edm.Binary": return Convert.FromBase64String(value);
                case "Edm.Guid": return new Guid(value);

                default: throw new NotSupportedException("Not supported type " + type);
            }
        }

        #endregion

        #endregion

        #endregion

        #region Validation

        public static bool ValidContainerName(string containerName)
        {
            if (containerName.Equals("$root")) return true;

            // Container names are 3-63 characters in length and may contain lower-case alphanumeric characters 
            // and dashes. Dashes must be preceded and followed by an alphanumeric character. 

            if (containerName.Length < 3 || containerName.Length > 63) return false;

            if (!Regex.IsMatch(containerName, "^[a-z0-9](([a-z0-9])|(\\-[a-z0-9]))+$")) return false;

            return true;
        }

        public static bool ValidBlobName(string blobName)
        {
            // A blob name can contain any combination of characters, but reserved URL characters must be properly escaped. 
            // A blob name must be at least one character long and cannot be more than 1,024 characters long.

            if (blobName.Length < 1 || blobName.Length > 1024) return false;

            return true;
        }

        public static bool ValidQueueName(string queueName)
        {
            // Queue names are 3-63 characters in length and may contain lower-case alphanumeric characters 
            // and dashes. Dashes must be preceded and followed by an alphanumeric character. 

            if (queueName.Length < 3 || queueName.Length > 63) return false;

            if (!Regex.IsMatch(queueName, "^[a-z0-9](([a-z0-9])|(\\-[a-z0-9]))+$")) return false;

            return true;
        }

        public static bool ValidTableName(string tableName)
        {
            // Table names must be valid DNS names, 3-63 characters in length, 
            // beginning with a letter and containing only alphanumeric characters. 
            // Table names are case-sensitive.

            if (tableName.Length < 3 || tableName.Length > 63) return false;

            if (!Regex.IsMatch(tableName, "^[a-zA-Z][a-zA-Z0-9]([a-zA-Z0-9])+$")) return false;

            return true;
        }

        #endregion

        public static string NormalizeContainerName(string containerName)
        {
            if (containerName.StartsWith("devstoreaccount1/"))
            {
                containerName = containerName.Substring(17);
            }
            return containerName;
        }

        public static string NormalizeQueueName(string containerName)
        {
            if (containerName.StartsWith("devstoreaccount1/"))
            {
                containerName = containerName.Substring(17);
            }
            return containerName;
        }

        public static string NormalizeBlobName(string blobName)
        {
            if (blobName.StartsWith("/devstoreaccount1/"))
            {
                blobName = blobName.Substring(18);
            }
            return blobName;
        }

        // Return a string, quote-enclosed if it contains spaces.

        public static string q(string text)
        {
            if (text.Contains(' '))
            {
                return "\"" + text + "\"";
            }
            else
            {
                return text;
            }
        }

        private string numberof(int count, string singular, string plural)
        {
            if (count == 1)
            {
                return "1 " + singular;
            }
            else
            {
                return count.ToString() + " " + plural;
            }
        }

        private TableServiceContext CreateTableServiceContext(CloudTableClient tableClient)
        {
            TableServiceContext tableServiceContext = tableClient.GetTableServiceContext();
            tableServiceContext.ResolveType = ResolveEntityType;
            tableServiceContext.ReadingEntity += new EventHandler<ReadingWritingEntityEventArgs>(OnReadingEntity);
            tableServiceContext.WritingEntity += new EventHandler<ReadingWritingEntityEventArgs>(OnWritingEntity);
            return tableServiceContext;
        }
    }
}