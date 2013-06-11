using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Neudesic.AzureStorageExplorer.Data;
using Neudesic.AzureStorageExplorer.Dialogs;
using Neudesic.AzureStorageExplorer.Properties;

namespace Neudesic.AzureStorageExplorer.ViewModel
{
    /// <summary>
    /// The ViewModel for the application's main window.
    /// </summary>
    public class MainWindowViewModel : WorkspaceViewModel
    {
        private const string ConfigFilename = "AzureStorageExplorer.config";

        #region Fields

        ObservableCollection<AccountViewModel> _accounts;
        ReadOnlyCollection<CommandViewModel> _commands;
        ObservableCollection<WorkspaceViewModel> _workspaces;

        #endregion

        #region Constructor

        public MainWindowViewModel()
        {
            base.DisplayName = Strings.MainWindowViewModel_DisplayName;

            LoadConfiguration();
        }

        #endregion

        #region Commands

        /// <summary>
        /// Returns a read-only list of commands 
        /// that the UI can display and execute.
        /// </summary>
        public ReadOnlyCollection<CommandViewModel> Commands
        {
            get
            {
                if (_commands == null)
                {
                    List<CommandViewModel> cmds = this.CreateCommands();
                    _commands = new ReadOnlyCollection<CommandViewModel>(cmds);
                }
                return _commands;
            }
        }

        List<CommandViewModel> CreateCommands()
        {

            return new List<CommandViewModel>
            {
                new CommandViewModel(
                    Strings.MainWindowViewModel_Command_ViewStorageAccount,
                    new RelayCommand(param => this.ViewStorageAccount(param as StorageAccount)))
            };
        }

        #endregion

        #region Accounts

        /// <summary>
        /// Returns a read-only list of commands 
        /// that the UI can display and execute.
        /// </summary>
        public ObservableCollection<AccountViewModel> Accounts
        {
            get
            {
                return _accounts;
            }
            set
            {
                _accounts = value;
                OnPropertyChanged("Accounts");
            }
        }

        #endregion // Commands

        #region Workspaces

        /// <summary>
        /// Returns the collection of available workspaces to display.
        /// A 'workspace' is a ViewModel that can request to be closed.
        /// </summary>
        public ObservableCollection<WorkspaceViewModel> Workspaces
        {
            get
            {
                if (_workspaces == null)
                {
                    _workspaces = new ObservableCollection<WorkspaceViewModel>();
                    _workspaces.CollectionChanged += this.OnWorkspacesChanged;
                }
                return _workspaces;
            }
        }

        void OnWorkspacesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (WorkspaceViewModel workspace in e.NewItems)
                    workspace.RequestClose += this.OnWorkspaceRequestClose;

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (WorkspaceViewModel workspace in e.OldItems)
                    workspace.RequestClose -= this.OnWorkspaceRequestClose;
        }

        void OnWorkspaceRequestClose(object sender, EventArgs e)
        {
            WorkspaceViewModel workspace = sender as WorkspaceViewModel;
            workspace.Dispose();
            this.Workspaces.Remove(workspace);
        }

        #endregion // Workspaces

        #region Private Helpers

        void ViewStorageAccount(StorageAccount account)
        {
            // See if the storage account is already displayed in a tab. If it is, select it.

            foreach (WorkspaceViewModel wvm in this.Workspaces)
            {
                if (wvm.DisplayName == account.Name)
                {
                    this.SetActiveWorkspace(wvm);
                    return;
                }
            }

            // Add a new workspace tab for the selected storage account.

            StorageAccountViewModel workspace = new StorageAccountViewModel(account);

            this.Workspaces.Add(workspace);

            this.SetActiveWorkspace(workspace);
        }

        void SetActiveWorkspace(WorkspaceViewModel workspace)
        {
            Debug.Assert(this.Workspaces.Contains(workspace));

            ICollectionView collectionView = CollectionViewSource.GetDefaultView(this.Workspaces);
            if (collectionView != null)
                collectionView.MoveCurrentTo(workspace);
        }

        #endregion

        #region Storage Accounts Load/Store

        // Load configuration from AzureStorageExplorer.config

        private void LoadConfiguration()
        {
            string name, value;
            int pos;
            List<AccountViewModel> accounts = new List<AccountViewModel>();
            AccountViewModel avm = null;

            accounts.Add(
                new AccountViewModel(
                        "-- Select a Storage Account --", String.Empty, false, false, 
                        new RelayCommand(param => this.ViewStorageAccount(param as StorageAccount)))
                        );

            var useHardcodedAccount = ConfigurationManager.AppSettings.Get("UseHardcodedAccount");
            var hardcodedAccountName = ConfigurationManager.AppSettings.Get("HardcodedAccountName");
            var hardCodedKey = ConfigurationManager.AppSettings.Get("HardcodedKey");
            var hardcodedUseHttps = ConfigurationManager.AppSettings.Get("HardcodedUseHttps");
            if (useHardcodedAccount != null && useHardcodedAccount.ToLower().Equals("true") &&
                !string.IsNullOrWhiteSpace(hardcodedAccountName) &&
                !string.IsNullOrWhiteSpace(hardCodedKey))
            {
                this.UseHardcodedAccount = true;
                var https = hardcodedUseHttps != null && hardcodedUseHttps.ToLower().Equals("true");
                accounts.Add(new AccountViewModel(hardcodedAccountName, hardCodedKey, https, true,
                                                  new RelayCommand(
                                                      param =>
                                                      this.ViewStorageAccount(param as StorageAccount))));
            }

            String folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\AzureStorageExplorer\\";
            Directory.CreateDirectory(folder);
            string filename = folder + ConfigFilename;

            if (File.Exists(filename))
            {
                string item = String.Empty;
                string line;
                using (TextReader tr = File.OpenText(filename))
                {
                    while ((line = tr.ReadLine()) != null)
                    {
                        if (line.StartsWith("["))
                        {
                            if (avm != null && !string.IsNullOrEmpty(avm.DisplayName) && !UseHardcodedAccount)
                            {
                                accounts.Add(avm);
                            }
                            switch (line)
                            {
                                case "[Account]":
                                    if (!UseHardcodedAccount)
                                    {
                                        item = line;
                                        avm = new AccountViewModel(null, null, false, false,
                                                                   new RelayCommand(
                                                                       param =>
                                                                       this.ViewStorageAccount(param as StorageAccount)));
                                    }
                                    else
                                    {
                                        item = "[SkipAccounts]";
                                    }
                                    break;
                                case "[Options]":
                                    item = line;
                                    break;
                                case "[ContentTypes]":
                                    item = line;
                                    break;
                            }
                        }
                        else
                        {
                            pos = line.IndexOf('=');
                            if (pos != -1)
                            {
                                name = line.Substring(0, pos);
                                value = line.Substring(pos + 1);
                                double doubleValue;
                                switch (item)
                                {
                                    case "[Options]":
                                        switch (name)
                                        {
                                            case "Culture":
                                                culture = value;
                                                break;
                                            case "ShowWelcomeOnStartup":
                                                showWelcomeOnStartup = (value == "1");
                                                break;
                                            case "PreserveWindowPosition":
                                                preserveWindowPosition = (value == "1");
                                                break;
                                            case "WindowTop":
                                                if (PreserveWindowPosition && Double.TryParse(value, out doubleValue))
                                                {
                                                    MainWindow.WindowTop = doubleValue;
                                                }
                                                break;
                                            case "WindowLeft":
                                                if (PreserveWindowPosition && Double.TryParse(value, out doubleValue))
                                                {
                                                    MainWindow.WindowLeft = doubleValue;
                                                }
                                                break;
                                            case "WindowHeight":
                                                if (PreserveWindowPosition && Double.TryParse(value, out doubleValue))
                                                {
                                                    MainWindow.WindowHeight = doubleValue;
                                                }
                                                break;
                                            case "WindowWidth":
                                                if (PreserveWindowPosition && Double.TryParse(value, out doubleValue))
                                                {
                                                    MainWindow.WindowWidth = doubleValue;
                                                }
                                                break;
                                            case "CheckForNewerVersion":
                                                checkForNewerVersion = (value == "1");
                                                break;
                                            case "LastVersionOffered":
                                                lastVersionOffered = value;
                                                break;
                                        }
                                        break;
                                    case "[ContentTypes]":
                                        switch (name)
                                        {
                                            case "SetContentTypeAutomatically":
                                                ContentTypeMapping.SetContentTypeAutomatically = (value == "1");
                                                break;
                                            case "Mapping":
                                                if (ContentTypeMapping.Values == null)
                                                {
                                                    ContentTypeMapping.Values = new List<ContentTypeMapping>();
                                                }
                                                ContentTypeMapping.Values.Add(new ContentTypeMapping(value));
                                                break;
                                        }
                                        break;
                                    case "[Account]":
                                        switch (name)
                                        {
                                            case "Name":
                                                avm.AccountName = value;
                                                break;
                                            case "ConnectionString":
                                                avm.Key = value;
                                                break;
                                            case "BlobContainersUpgraded":
                                                avm.BlobContainersUpgraded = (value == "1");
                                                break;
                                            case "AutoOpen":
                                                //account.AutoOpen = Boolean.Parse(value);
                                                break;
                                            case "UseHttps":
                                                avm.UseHttps = (value == "1");
                                                break;
                                        }
                                        break;
                                    case "[SkipAccounts]":
                                        break;
                                }
                            }
                        }
                    }
                    tr.Close();

                    if (avm != null)
                    {
                        if (avm != null && !string.IsNullOrEmpty(avm.AccountName))
                        {
                            accounts.Add(avm);
                        }
                        avm = null;
                    }
                }
            }

            SetCulture();

            Accounts = new ObservableCollection<AccountViewModel>(accounts);
        }

        private string BoolText(bool value)
        {
            if (value)
            {
                return "1";
            }
            else
            {
                return "0";
            }
        }

        public void SaveConfiguration()
        {
            String folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\AzureStorageExplorer\\";
            Directory.CreateDirectory(folder);
            string filename = folder +  ConfigFilename;

            using (TextWriter tw = File.CreateText(filename))
            {
                tw.WriteLine("[Options]");
                tw.WriteLine("Culture=" + Culture);
                tw.WriteLine("ShowWelcomeOnStartup=" + BoolText(ShowWelcomeOnStartup));
                tw.WriteLine("PreserveWindowPosition=" + BoolText(PreserveWindowPosition));
                tw.WriteLine("WindowTop=" + MainWindow.WindowTop.ToString());
                tw.WriteLine("WindowLeft=" + MainWindow.WindowLeft.ToString());
                tw.WriteLine("WindowHeight=" + MainWindow.WindowHeight.ToString());
                tw.WriteLine("WindowWidth=" + MainWindow.WindowWidth.ToString());
                tw.WriteLine("CheckForNewerVersion=" + BoolText(CheckForNewerVersion));
                tw.WriteLine("LastVersionOffered=" + LastVersionOffered);

                tw.WriteLine("[ContentTypes]");
                tw.WriteLine("SetContentTypeAutomatically=" + BoolText(SetContentTypeAutomatically));
                if (ContentTypeMapping.Values != null)
                {
                    foreach (ContentTypeMapping mapping in ContentTypeMapping.Values)
                    {
                        tw.WriteLine("Mapping=" + mapping.ToString());
                    }
                }

                if (Accounts != null)
                {
                    foreach (AccountViewModel avm in Accounts)
                    {
                        if (avm.AccountName == "DevStorage" || !String.IsNullOrEmpty(avm.Key))
                        {
                            tw.WriteLine("[Account]");
                            tw.WriteLine("Name=" + avm.AccountName);
                            tw.WriteLine("ConnectionString=" + avm.Key);
                            tw.WriteLine("UseHttps=" + BoolText(avm.UseHttps));
                            tw.WriteLine("BlobContainersUpgraded=" + BoolText(avm.BlobContainersUpgraded));
                        }
                    }

                }
                tw.Close();
            }
        }

        #endregion

        public AccountViewModel AddAccount(string name, string key, bool useHttps, bool blobContainersUpgraded)
        {
            AccountViewModel avm = new AccountViewModel(name, key, useHttps, blobContainersUpgraded,
                new RelayCommand(param => this.ViewStorageAccount(param as StorageAccount))
                );

            Accounts.Add(avm);
            
            List<AccountViewModel> list = new List<AccountViewModel>(_accounts.ToArray());
            list.Sort();
            _accounts = new ObservableCollection<AccountViewModel>(list);

            SaveConfiguration();

            OnPropertyChanged("Accounts");

            return avm;
        }

        public AccountViewModel UpdateAccount(AccountViewModel avm)
        {
            List<AccountViewModel> list = new List<AccountViewModel>(_accounts.ToArray());
            list.Sort();
            _accounts = new ObservableCollection<AccountViewModel>(list);

            SaveConfiguration();

            OnPropertyChanged("Accounts");

            return avm;
        }

        public void RemoveAccount(AccountViewModel avm)
        {
            Accounts.Remove(avm);
            SaveConfiguration();
            OnPropertyChanged("Accounts");
        }

        #region Configuration / Options

        private bool autoSaveConfiguration = true;
        public bool AutoSaveConfiguration
        {
            get
            {
                return autoSaveConfiguration;
            }
            set
            {
                autoSaveConfiguration = value;
            }
        }

        private bool showWelcomeOnStartup = true;
        public bool ShowWelcomeOnStartup
        {
            get
            {
                return showWelcomeOnStartup;
            }
            set
            {
                if (showWelcomeOnStartup != value)
                {
                    showWelcomeOnStartup = value;
                    if (autoSaveConfiguration) SaveConfiguration();
                }
            }
        }

        private bool preserveWindowPosition = true;
        public bool PreserveWindowPosition
        {
            get
            {
                return preserveWindowPosition;
            }
            set
            {
                if (preserveWindowPosition != value)
                {
                    preserveWindowPosition = value;
                    if (autoSaveConfiguration) SaveConfiguration();
                }
            }
        }

        private bool checkForNewerVersion = true;
        public bool CheckForNewerVersion
        {
            get
            {
                return checkForNewerVersion;
            }
            set
            {
                if (checkForNewerVersion != value)
                {
                    checkForNewerVersion = value;
                    lastVersionOffered = String.Empty;
                    if (autoSaveConfiguration)
                    {
                        SaveConfiguration();
                    }
                }
            }
        }

        private string lastVersionOffered = String.Empty;
        public string LastVersionOffered
        {
            get
            {
                return lastVersionOffered;
            }
            set
            {
                if (lastVersionOffered != value)
                {
                    lastVersionOffered = value;
                    if (autoSaveConfiguration) SaveConfiguration();
                }
            }
        }

        public bool UseHardcodedAccount { get; private set; }

        public bool SetContentTypeAutomatically
        {
            get
            {
                return ContentTypeMapping.SetContentTypeAutomatically;
            }
            set
            {
                if (ContentTypeMapping.SetContentTypeAutomatically != value)
                {
                    ContentTypeMapping.SetContentTypeAutomatically = value;
                    if (autoSaveConfiguration) SaveConfiguration();
                }
            }
        }

        #region Culture

        private string culture = string.Empty;
        public string Culture
        {
            get
            {
                return culture;
            }
            set
            {
                if (culture != value)
                {
                    culture = value;
                    SetCulture();
                    if (autoSaveConfiguration) SaveConfiguration();
                }
            }
        }

        private void SetCulture()
        {
            if (String.IsNullOrEmpty(Culture))
            {
                // Use the OS culture.

                FrameworkElement.LanguageProperty.OverrideMetadata(
                    typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(
                        CultureInfo.CurrentCulture.IetfLanguageTag)));
            }
            else
            {
                // Set a specific culture such as it-IT, en-US, etc.
                
                try
                {
                    System.Threading.Thread.CurrentThread.CurrentCulture =
                        System.Threading.Thread.CurrentThread.CurrentUICulture =
                            new System.Globalization.CultureInfo(culture);
                }
                catch (Exception)
                {
                    // Couldn't set the culture - go with en-US.

                    System.Threading.Thread.CurrentThread.CurrentCulture =
                        System.Threading.Thread.CurrentThread.CurrentUICulture =
                            new System.Globalization.CultureInfo("en-US");
                }
            }
        }

        #endregion

        #region Content Types

        public ObservableCollection<ContentTypeMapping> ContentTypes
        {
            get
            {
                if (ContentTypeMapping.Values == null)
                {
                    ContentTypeMapping.Values = ContentTypeMapping.DefaultValues();
                }

                // Return a copy of the content type values in an observable collection to allow editing and cancellation.

                ObservableCollection<ContentTypeMapping> contentTypes = new ObservableCollection<ContentTypeMapping>();
                if (ContentTypeMapping.Values != null)
                {
                    foreach (ContentTypeMapping mapping in ContentTypeMapping.Values)
                    {
                        contentTypes.Add(mapping);
                    }
                }
                return contentTypes;
            }
            set
            {
                List<ContentTypeMapping> contentTypes = new List<ContentTypeMapping>();
                if (value != null)
                {
                    foreach(ContentTypeMapping mapping in value)
                    {
                        contentTypes.Add(mapping);
                    }
                }
                ContentTypeMapping.Values = new List<ContentTypeMapping>(contentTypes);
            }
        }

        #endregion

        #endregion
    }
}