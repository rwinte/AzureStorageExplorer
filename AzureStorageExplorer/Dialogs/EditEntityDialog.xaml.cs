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
using Neudesic.AzureStorageExplorer.Data;
using Neudesic.AzureStorageExplorer.ViewModel;

namespace Neudesic.AzureStorageExplorer.Dialogs
{
    /// <summary>
    /// Interaction logic for NewContainerDialog.xaml
    /// </summary>
    public partial class EditEntityDialog : Window
    {
        private bool CopyEntity = false;

        private EntityViewModel ViewModel
        {
            get
            {
                return DataContext as EntityViewModel;
            }
        }

        public EditEntityDialog(string mode)
        {
            InitializeComponent();

            switch(mode)
            {
                case "new":
                    this.Title = "New Entity";
                    this.SaveEntity.Content = "Create Entity";
                    break;
                case "edit":
                    this.Title = "Edit Entity";
                    this.SaveEntity.Content = "Update Entity";
                    break;
                case "copy":
                    this.CopyEntity = true;
                    this.Title = "Copy Entity";
                    this.SaveEntity.Content = "Copy Entity";
                    break;
            }
        }

        private void SaveEntity_Click(object sender, RoutedEventArgs e)
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
            bool haveValidRowKey = false;
            foreach (Property property in ViewModel.Properties)
            {
                if (property.PropertyName == "RowKey" && !String.IsNullOrEmpty(property.PropertyValue))
                {
                    haveValidRowKey = true;
                }
            }
            
            if (!haveValidRowKey)
            {
                MessageBox.Show("A non-blank row key (RowKey property) is required.", "Row Key Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            if (CopyEntity)
            {
                if (ViewModel.Entity.PartitionKey == ViewModel.UpdatedEntity.PartitionKey &&
                    ViewModel.Entity.RowKey == ViewModel.UpdatedEntity.RowKey)
                {
                MessageBox.Show("The copy must have a different key than the original record. Change the partition key and/or row key and try again.", "Key Change Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
                }
            }

            return true;
        }

    }
}
