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
using Neudesic.AzureStorageExplorer.ViewModel;

namespace Neudesic.AzureStorageExplorer.Dialogs
{
    /// <summary>
    /// Interaction logic for RenameContainerDialog.xaml
    /// </summary>
    public partial class RenameContainerDialog : Window
    {
        public RenameContainerDialog()
        {
            InitializeComponent();

            DestContainerName.Focus();
        }

        private void CreateContainer_Click(object sender, RoutedEventArgs e)
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
            if (String.IsNullOrEmpty(SourceContainerName.Text))
            {
                MessageBox.Show("A source container name is required", "Source Container Name Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            else if (String.IsNullOrEmpty(DestContainerName.Text))
            {
                MessageBox.Show("A destination container name is required", "Destination Container Name Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            string rules = "Container names must be 3-63 characters in length and may contain lower-case alphanumeric characters and dashes. Dashes must be preceded and followed by an alphanumeric character.";

            if (!StorageAccountViewModel.ValidContainerName(SourceContainerName.Text))
            {
                MessageBox.Show("The source container name is invalid.\r\n\r\n" + rules, "Invalid Source Container Name", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            if (!StorageAccountViewModel.ValidContainerName(DestContainerName.Text))
            {
                MessageBox.Show("The source queue name is invalid.\r\n\r\n" + rules, "Invalid Source Container Name", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            if (String.Compare(SourceContainerName.Text, DestContainerName.Text) == 0)
            {
                MessageBox.Show("The destination container name must be different from the original container name", "Destination Container Name Not Unique", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }


            return true;
        }
    }
}
