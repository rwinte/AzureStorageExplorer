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
    /// Interaction logic for CopyContainerDialog.xaml
    /// </summary>
    public partial class CopyQueueDialog : Window
    {
        public CopyQueueDialog()
        {
            InitializeComponent();

            DestQueueName.Focus();
        }

        private void CreateQueue_Click(object sender, RoutedEventArgs e)
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
            if (String.IsNullOrEmpty(SourceQueueName.Text))
            {
                MessageBox.Show("A source queue name is required", "Source Queue Name Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            else if (String.IsNullOrEmpty(DestQueueName.Text))
            {
                MessageBox.Show("A destination queue name is required", "Destination Queue Name Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            string rules = "Queue names must be 3-63 characters in length and may contain lower-case alphanumeric characters and dashes. Dashes must be preceded and followed by an alphanumeric character.";

            if (!StorageAccountViewModel.ValidQueueName(SourceQueueName.Text))
            {
                MessageBox.Show("The source queue name is invalid.\r\n\r\n" + rules, "Invalid Source Queue Name", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            if (!StorageAccountViewModel.ValidQueueName(DestQueueName.Text))
            {
                MessageBox.Show("The destination queue name is invalid.\r\n\r\n" + rules, "Invalid Source Queue Name", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            if (String.Compare(SourceQueueName.Text, DestQueueName.Text) == 0)
            {
                MessageBox.Show("The destination queue name must be different from the original queue name", "Destination Table Name Not Unique", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }


            return true;
        }
    }
}
