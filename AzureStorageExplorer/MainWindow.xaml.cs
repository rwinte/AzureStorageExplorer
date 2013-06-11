using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Neudesic.AzureStorageExplorer;
using Neudesic.AzureStorageExplorer.Data;
using Neudesic.AzureStorageExplorer.ViewModel;
using Neudesic.AzureStorageExplorer.Dialogs;

namespace Neudesic.AzureStorageExplorer
{
    public partial class MainWindow : System.Windows.Window
    {
        #region Properties

        public static System.Windows.Window Window { get; set; }
        public static MainWindow This { get; set; }

        public static List<Exception> Exceptions = new List<Exception>();

        private bool NewVersionAvailable = false;
        private string NewVersionNumber, NewVersionProductName, NewVersionReleaseName;
        private string NewVersionReleaseDate, NewVersionRecommended, NewVersionDownloadUrl;
        private string CurrentVersion;

        public MainWindowViewModel ViewModel
        {
            get
            {
                return DataContext as MainWindowViewModel;
            }
        }

        #endregion

        #region Initialization

        public MainWindow()
        {
            InitializeComponent();

            This = this;
            Window = this as System.Windows.Window;
            this.Loaded += new System.Windows.RoutedEventHandler(MainWindow_Loaded);
        }

        void MainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (StorageAccountsComboBox.ItemsSource != null)
            {
                StorageAccountsComboBox.SelectedIndex = 0;
            }

            CheckNewVersionAvailable(false);

            if (NewVersionAvailable)
            {
                if (OfferNewVersion())
                {
                    Environment.Exit(0);
                }
            }

            if (ViewModel.ShowWelcomeOnStartup)
            {
                ShowWelcome();
            }

            if (ViewModel.UseHardcodedAccount)
            {
                if (ViewModel.Accounts.Count <= 1)
                {
                    throw new ConfigurationErrorsException("Hardcoded Account enabled no account is configured.");
                }

                var avm = ViewModel.Accounts[1];
                RelayCommand command = avm.Command as RelayCommand;
                StorageAccount account = avm.Account;

                if (command != null)
                {
                    command.Execute(account);
                }
            }
        }

        #endregion

        #region Updated Version Check

        private void CheckNewVersionAvailable(bool recheckEvenIfAlreadyOffered)
        {
            try
            {
                if (!ViewModel.CheckForNewerVersion) return;

                WebClient webClient = new WebClient();
                string latestVersionData = webClient.DownloadString("http://neudesic.blob.core.windows.net/versioning/azurestorageexplorer.version");
                if (!String.IsNullOrEmpty(latestVersionData))
                {
                    string[] parts = latestVersionData.Split(',');

                    if (parts.Length >= 5)
                    {
                        // <version>,<product-name>,<release-name>,<date>,<recommended>,<download-url>
                        // 4.0.0.4,Azure Storage Explorer,Beta 1 Refresh 4 (4.0.0.4),10/26/2010,1,http://azurestorageexplorer.codeplex.com

                        NewVersionNumber = parts[0];
                        NewVersionProductName = parts[1];
                        NewVersionReleaseName = parts[2];
                        NewVersionReleaseDate = parts[3];
                        NewVersionRecommended = parts[4];
                        NewVersionDownloadUrl = parts[5];

                        Assembly oAssembly = Assembly.GetExecutingAssembly();
                        FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(oAssembly.Location);

                        CurrentVersion = versionInfo.FileMajorPart.ToString() + "." +
                                                 versionInfo.FileMinorPart.ToString() + "." +
                                                 versionInfo.FileBuildPart.ToString() + "." + 
                                                 versionInfo.FilePrivatePart.ToString();

                        string currentVersion = versionInfo.FileMajorPart.ToString() +
                                 versionInfo.FileMinorPart.ToString() +
                                 versionInfo.FileBuildPart.ToString() +
                                 versionInfo.FilePrivatePart.ToString();

                        string newVersion = NewVersionNumber.Replace(".", String.Empty);

                        if (Int32.Parse(currentVersion) < Int32.Parse(newVersion))
                        {
                            if (recheckEvenIfAlreadyOffered ||
                                NewVersionNumber != ViewModel.LastVersionOffered)
                            {
                                NewVersionAvailable = true;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private bool OfferNewVersion()
        {
            if (NewVersionAvailable)
            {
                NewVersionDialog dlg = new NewVersionDialog();
                dlg.Owner = MainWindow.Window;
                dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                dlg.DownloadUrl = NewVersionDownloadUrl;
                dlg.CheckForNewVersionCheckbox.IsChecked = ViewModel.CheckForNewerVersion;
                dlg.CurrentVersion.Text = "Current version: " + CurrentVersion;
                dlg.LatestVersion.Text = "Latest version: " + NewVersionNumber + " - " + NewVersionReleaseName;

                switch (NewVersionRecommended)
                {
                    default:
                    case "0":
                        dlg.Recommended.Text = "This update is optional.";
                        break;
                    case "1":
                        dlg.Recommended.Text = "This update is recommended.";
                        break;
                    case "2":
                        dlg.Recommended.Text = "This update is strongly recommended.";
                        break;
                }

                bool performedAction = dlg.ShowDialog().Value;
                ViewModel.CheckForNewerVersion = dlg.CheckForNewVersionCheckbox.IsChecked.Value;

                if (performedAction)
                {
                    // Download or No Thanks

                    ViewModel.LastVersionOffered = NewVersionNumber;
                    return true;
                }
                else
                {
                    // Ask me Later or Close

                    if (dlg.CheckForNewVersionCheckbox.IsChecked.Value)
                    {
                        ViewModel.AutoSaveConfiguration = false;
                        ViewModel.CheckForNewerVersion = true;
                        ViewModel.LastVersionOffered = string.Empty;
                        ViewModel.SaveConfiguration();
                        ViewModel.AutoSaveConfiguration = true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Set / Get Window Position and Size

        public static double WindowTop
        {
            get
            {
                return This.Top;
            }
            set
            {
                This.Top = value;
            }
        }

        public static double WindowLeft
        {
            get
            {
                return This.Left;
            }
            set
            {
                This.Left = value;
            }
        }

        public static double WindowHeight
        {
            get
            {
                return This.Height;
            }
            set
            {
                This.Height = value;
            }
        }

        public static double WindowWidth
        {
            get
            {
                return This.Width;
            }
            set
            {
                This.Width = value;
            }
        }

        #endregion

        #region Storage Account Navigation and Administration

        // A storage account has been selected. Execute the ViewStorageAccount command for the selected account,
        // which will open a workspace tab.

        private void StorageAccountsComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (StorageAccountsComboBox.SelectedIndex > 0)
            {
                AccountViewModel avm = StorageAccountsComboBox.SelectedItem as AccountViewModel;
                RelayCommand command = avm.Command as RelayCommand;
                StorageAccount account = avm.Account;

                if (avm.AccountName == "DevStorage" && !DeveloperStorageRunning())
                {
                    MessageBox.Show("Windows Azure Developer Storage is not running.\r\n\r\nThe process DSService.exe is not detected", "Developer Storage Not Detected", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                
                if (command != null)
                {
                    command.Execute(account);
                }
            }
        }

        // Return true if Developer Storage is running by scanning for the DSService.exe process.

        private bool DeveloperStorageRunning()
        {
            const string name = "DSService";

            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.Contains(name))
                {
                    return true;
                }
            }
            return false;
        }

        private void AddStorageAccount_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AddAccountDialog dlg = new AddAccountDialog(false);
            dlg.Owner = MainWindow.Window;
            dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            if (dlg.ShowDialog().Value)
            {
                try
                {
                    string name = dlg.AccountName.Text;
                    string key = dlg.AccountKey.Text;
                    bool useHttps = dlg.UseHttps.IsChecked.Value;
                    bool proceed = false;

                    if (name == "DevStorage")
                    {
                        if (!DeveloperStorageRunning())
                        {
                            MessageBox.Show("Windows Azure Developer Storage is not running.\r\n\r\nThe process DSService.exe is not detected", "Developer Storage Not Detected", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return;
                        }
                        proceed = true;
                    }
                    else
                    {
                        if (MessageBox.Show("Please note, the first time your blob containers are scanned there may be a wait of several minutes if any old format containers are encountered. They will be automatically upgraded to current Azure standards.\r\n\r\nFor more information, see http://social.msdn.microsoft.com/Forums/en-US/windowsazure/thread/5926f6dc-2e46-4654-a3c9-b397d0598a16",
                            "New Storage Account", MessageBoxButton.OKCancel, MessageBoxImage.Information)
                            == MessageBoxResult.OK)
                        {
                            proceed = true;
                        }
                    }

                    if (proceed)
                    {
                        StorageAccountsComboBox.SelectedItem = ViewModel.AddAccount(name, key, useHttps, false);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred saving the configuration.\r\n\r\n" + ex.ToString(),
                        "Could Not Save Configuration", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        private void RemoveStorageAccount_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (StorageAccountsComboBox.SelectedIndex <= 0 || StorageAccountsComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a storage account to remove.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);               
                return;
            }

            AccountViewModel avm = StorageAccountsComboBox.SelectedItem as AccountViewModel;

            if (MessageBox.Show("Are you SURE you want to remove the account '" + avm.AccountName + 
                "'?\r\n\r\nThis will remove the account name and key from Azure Storage Explorer's saved configuration.\r\n\r\nCloud storage is not affected by this action.",
                "Confirm Account Delete", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                try
                {
                    StorageAccountsComboBox.SelectedIndex = 0;
                    ViewModel.RemoveAccount(avm);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred saving the configuration.\r\n\r\n" + ex.ToString(),
                        "Could Not Save Configuration", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        #endregion

        #region Menu Navigation

        #region View Menu

        #region View Errors

        private void ViewErrorsCanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ViewErrorsExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            ViewErrorsDialog dlg = new ViewErrorsDialog(MainWindow.Exceptions);
            dlg.Owner = MainWindow.Window;
            dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            dlg.ShowDialog();
        }

        #endregion

        #endregion

        #region Tools Menu

        #region Tools / Options

        private void ToolsOptionsCanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ToolsOptionsExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            OptionsDialog dlg = new OptionsDialog();
            dlg.Owner = MainWindow.Window;
            dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            
            dlg.Culture = ViewModel.Culture;
            dlg.ShowWelcomeOnStartup = ViewModel.ShowWelcomeOnStartup;
            dlg.CheckForNewerVersion = ViewModel.CheckForNewerVersion;
            dlg.PreserveWindowPosition = ViewModel.PreserveWindowPosition;

            dlg.SetContentTypeAutomtically = ViewModel.SetContentTypeAutomatically;
            dlg.ContentTypes = ViewModel.ContentTypes;

            if (dlg.ShowDialog().Value)
            {
                if (ViewModel.Culture != dlg.Culture)
                {
                    MessageBox.Show("Culture changes will apply the next time you launch Azure Storage Explorer",
                        "Culture Changed", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
                Cursor = Cursors.Wait;

                ViewModel.AutoSaveConfiguration = false;

                ViewModel.Culture = dlg.Culture;
                ViewModel.ShowWelcomeOnStartup = dlg.ShowWelcomeOnStartup;
                ViewModel.PreserveWindowPosition = dlg.PreserveWindowPosition;

                ViewModel.SetContentTypeAutomatically = dlg.SetContentTypeAutomtically;
                ViewModel.CheckForNewerVersion = dlg.CheckForNewerVersion;
                ViewModel.ContentTypes = dlg.ContentTypes;
                
                ViewModel.SaveConfiguration();

                ViewModel.AutoSaveConfiguration = true;

                Cursor = Cursors.Arrow;
            }
        }

        #endregion

        #region Tools / Check For New Version

        private void ToolsCheckForNewVersionCanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ToolsCheckForNewVersionExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            CheckNewVersionAvailable(true);
            if (OfferNewVersion())
            {
                Environment.Exit(0);
            }
        }

        #endregion


        #endregion

        #region Help Menu

        #region Show Welcome Screen

        private void HelpWelcomeCanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void HelpWelcomeExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            ShowWelcome();
        }

        private void ShowWelcome()
        {
            WelcomeDialog dlg = new WelcomeDialog();
            dlg.Owner = MainWindow.Window;
            dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            dlg.ShowWelcomeOnStartup = ViewModel.ShowWelcomeOnStartup;
            dlg.ShowDialog();
            ViewModel.ShowWelcomeOnStartup = dlg.ShowWelcomeOnStartup;
        }

        #endregion

        #region Give Feedback

        private void HelpFeedbackCanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void HelpFeedbackExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            GiveFeedbackOnCodeplex();
        }

        public static void GiveFeedbackOnCodeplex()
        {
            string url = "http://azurestorageexplorer.codeplex.com/discussions";
            System.Diagnostics.Process.Start(url);
        }

        #endregion

        #region Help About

        private void HelpAboutCanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void HelpAboutExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Azure Storage Explorer version 4.0.0.10 (05.07.2011).\r\n\r\nA community donation of Neudesic.", "About Azure Storage Explorer", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        #endregion

        #region Help (F1) - User Guide

        private void HelpCanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void HelpExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            MainWindow.OpenUserGuide();
        }

        public static void OpenUserGuide()
        {
            try
            {
                string url = "\"Docs\\Azure Storage Explorer 4 User Guide.pdf\"";
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception)
            {
                MessageBox.Show("The user guide could not be opened.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ViewModel.PreserveWindowPosition)
            {
                Cursor = Cursors.Wait;

                ViewModel.SaveConfiguration();
            }
        }

        #endregion

        #endregion

        public static void SaveConfiguration()
        {
            This.ViewModel.SaveConfiguration();
        }

        public static AccountViewModel GetAccount(string name)
        {
            foreach (AccountViewModel avm in This.ViewModel.Accounts)
            {
                if (avm.AccountName == name)
                {
                    return avm;
                }
            }
            return null;
        }

        private void EditStorageAccount_Click(object sender, RoutedEventArgs e)
        {
            if (StorageAccountsComboBox.SelectedIndex <= 0 || StorageAccountsComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a storage account to edit.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            AccountViewModel avm = StorageAccountsComboBox.SelectedItem as AccountViewModel;

            AddAccountDialog dlg = new AddAccountDialog(true);
            dlg.Owner = MainWindow.Window;
            dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            dlg.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            string oldName = avm.AccountName;

            dlg.AccountName.Text = avm.AccountName;
            dlg.AccountKey.Text = avm.Key;
            dlg.UseHttps.IsChecked = avm.UseHttps;
            if (avm.AccountName == "DevStorage")
            {
                dlg.DevStorage.IsChecked = true;
            }

            if (dlg.ShowDialog().Value)
            {
                try
                {
                    string name = dlg.AccountName.Text;
                    string key = dlg.AccountKey.Text;
                    bool useHttps = dlg.UseHttps.IsChecked.Value;
                    bool proceed = false;

                    if (name == "DevStorage")
                    {
                        if (!DeveloperStorageRunning())
                        {
                            MessageBox.Show("Windows Azure Developer Storage is not running.\r\n\r\nThe process DSService.exe is not detected", "Developer Storage Not Detected", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return;
                        }
                        proceed = true;
                    }
                    else
                    {
                        proceed = true;
                    }

                    if (proceed)
                    {
                        avm.AccountName = name;
                        avm.Key = key;
                        avm.UseHttps = useHttps;
                        StorageAccountsComboBox.SelectedItem = ViewModel.UpdateAccount(avm);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred saving the configuration.\r\n\r\n" + ex.ToString(),
                        "Could Not Save Configuration", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }

        }
    }
}