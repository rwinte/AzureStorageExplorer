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
    /// Interaction logic for CopyContainerDialog.xaml
    /// </summary>
    public partial class AddAccountDialog : Window
    {
        public bool Editing;  // If true, we are editing not adding.

        public AddAccountDialog(bool editing)
        {
            InitializeComponent();

            this.Editing = editing;

            if (editing)
            {
                this.Title = "Edit Storage Account";
                this.Save.Content = "Update Storage Account";
            }

            AccountName.Focus();
        }

        private void AddAccount_Click(object sender, RoutedEventArgs e)
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
            if (String.IsNullOrEmpty(AccountName.Text))
            {
                MessageBox.Show("A storage account name is required", "Storage Account Name Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            if (!DevStorage.IsChecked.Value && String.IsNullOrEmpty(AccountKey.Text))
            {
                MessageBox.Show("A storage account key is required", "Storage Account Key Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            return true;
        }

        private string SaveAccountName, SaveAccountKey;

        private void DevStorage_Checked(object sender, RoutedEventArgs e)
        {
            SaveAccountName = AccountName.Text;
            SaveAccountKey = AccountKey.Text;
            AccountName.Text = "DevStorage";
            AccountName.IsReadOnly = true;
            AccountKey.Text = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";
            AccountKeyLabel.Opacity = 0;
            AccountKey.Opacity = 0;
        }

        private void DevStorage_Unchecked(object sender, RoutedEventArgs e)
        {
            AccountName.Text = SaveAccountName;
            AccountKey.Text = SaveAccountKey;
            AccountName.IsReadOnly = false;
            AccountKeyLabel.Opacity = 1;
            AccountKey.Opacity = 1;
        }

        private void AccountName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AccountName.Text.Length == 0)
            {
                AccountNameGhost.Opacity = 1;
            }
            else
            {
                AccountNameGhost.Opacity = 0;
            }
        }

        private void AccountKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AccountKey.Text.Length == 0)
            {
                AccountKeyGhost.Opacity = 1;
            }
            else
            {
                AccountKeyGhost.Opacity = 0;
            }
        }

        private void AccountName_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

    }
}
