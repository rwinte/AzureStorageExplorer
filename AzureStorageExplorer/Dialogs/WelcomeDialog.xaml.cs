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
    public partial class WelcomeDialog : Window
    {
        public bool ShowWelcomeOnStartup { get; set; }

        public WelcomeDialog()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Feedback_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GiveFeedbackOnCodeplex();
        }

    }
}
