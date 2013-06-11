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
    /// Interaction logic for NewVersionDialog.xaml
    /// </summary>
    public partial class NewVersionDialog : Window
    {
        public string DownloadUrl { get; set; }

        public NewVersionDialog()
        {
            InitializeComponent();

            this.DataContext = this;

            this.Loaded += new RoutedEventHandler(NewVersionDialog_Loaded);
        }

        void NewVersionDialog_Loaded(object sender, RoutedEventArgs e)
        {
            System.Media.SystemSounds.Exclamation.Play();
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(DownloadUrl);
            DialogResult = true;
            Close();
        }

        private void NoThanks_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            CheckForNewVersionCheckbox.IsChecked = true;
            DialogResult = false;
            Close();
        }

    }
}
