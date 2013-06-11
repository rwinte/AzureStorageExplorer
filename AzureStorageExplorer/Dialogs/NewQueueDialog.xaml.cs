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
    /// Interaction logic for NewContainerDialog.xaml
    /// </summary>
    public partial class NewQueueDialog : Window
    {
        public NewQueueDialog()
        {
            InitializeComponent();

            QueueName.Focus();
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
            if (String.IsNullOrEmpty(QueueName.Text))
            {
                MessageBox.Show("A queue name is required", "Queue Name Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            if (!StorageAccountViewModel.ValidQueueName(QueueName.Text))
            {
                MessageBox.Show("The queue name is invalid.\r\n\r\nQueue names must be 3-63 characters in length and may contain lower-case alphanumeric characters and dashes. Dashes must be preceded and followed by an alphanumeric character.", "Invalid Queue Name", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            return true;
        }
    }
}
