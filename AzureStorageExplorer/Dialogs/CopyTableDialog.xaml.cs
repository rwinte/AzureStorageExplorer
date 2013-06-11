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
    public partial class CopyTableDialog : Window
    {
        public CopyTableDialog()
        {
            InitializeComponent();

            DestTableName.Focus();
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
            if (String.IsNullOrEmpty(SourceTableName.Text))
            {
                MessageBox.Show("A source table name is required", "Source Table Name Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            else if (String.IsNullOrEmpty(DestTableName.Text))
            {
                MessageBox.Show("A destination table name is required", "Destination Table Name Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            string rules = "Table names must be valid DNS names, 3-63 characters in length, beginning with a letter and containing only alphanumeric characters. Table names are case-sensitive.";

            if (!StorageAccountViewModel.ValidTableName(SourceTableName.Text))
            {
                MessageBox.Show("The source table name is invalid.\r\n\r\n" + rules, "Invalid Source Table Name", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            if (!StorageAccountViewModel.ValidTableName(DestTableName.Text))
            {
                MessageBox.Show("The destination table name is invalid.\r\n\r\n" + rules, "Invalid Source Table Name", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            if (String.Compare(SourceTableName.Text, DestTableName.Text) == 0)
            {
                MessageBox.Show("The destination table name must be different from the original table name", "Destination Table Name Not Unique", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            return true;
        }
    }
}
