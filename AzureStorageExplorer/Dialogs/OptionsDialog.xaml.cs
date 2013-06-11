using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Neudesic.AzureStorageExplorer.Data;

namespace Neudesic.AzureStorageExplorer.Dialogs
{
    /// <summary>
    /// Interaction logic for OptionsDialog.xaml
    /// </summary>
    public partial class OptionsDialog : Window
    {
        public string Culture { get; set; }
        public bool ShowWelcomeOnStartup { get; set; }
        public bool PreserveWindowPosition { get; set; }
        public bool CheckForNewerVersion { get; set; }

        public bool SetContentTypeAutomtically { get; set; }
        public ObservableCollection<ContentTypeMapping> ContentTypes { get; set; }

        public OptionsDialog()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void DefaultContentTypes_Click(object sender, RoutedEventArgs e)
        {
            ContentTypes = new ObservableCollection<ContentTypeMapping>(ContentTypeMapping.DefaultValues());
            ContentTypesGrid.ItemsSource = null;
            ContentTypesGrid.ItemsSource = ContentTypes;
        }

    }
}

