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
    /// Interaction logic for NewContainerDialog.xaml
    /// </summary>
    public partial class ViewErrorsDialog : Window
    {
        public List<Exception> Exceptions { get; set; }

        public ViewErrorsDialog(List<Exception> exceptions)
        {
            Exceptions = exceptions;

            InitializeComponent();

            DataContext = this;
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Exceptions.Clear();
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ExceptionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ExceptionListBox.SelectedIndex == -1) return;

            MessageBox.Show((ExceptionListBox.SelectedItem as Exception).ToString(), "Exception Detail", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
