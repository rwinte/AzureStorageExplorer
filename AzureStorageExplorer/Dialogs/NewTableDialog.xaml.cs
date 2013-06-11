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
    public partial class NewTableDialog : Window
    {
        public NewTableDialog()
        {
            InitializeComponent();

            TableName.Focus();
        }

        private void CreateTable_Click(object sender, RoutedEventArgs e)
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
            if (String.IsNullOrEmpty(TableName.Text))
            {
                MessageBox.Show("A table name is required", "Table Name Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            if (!StorageAccountViewModel.ValidTableName(TableName.Text))
            {
                MessageBox.Show("The table name is invalid.\r\n\r\nTable names must be valid DNS names, 3-63 characters in length, beginning with a letter and containing only alphanumeric characters. Table names are case-sensitive.", "Invalid Table Name", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            return true;
        }
    }
}
