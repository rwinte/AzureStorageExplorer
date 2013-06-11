using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.WindowsAzure.Storage.Blob;
using Neudesic.AzureStorageExplorer.Data;
using Neudesic.AzureStorageExplorer.ViewModel;
using Microsoft.WindowsAzure.Storage;

namespace Neudesic.AzureStorageExplorer.Dialogs
{
    /// <summary>
    /// Interaction logic for BlobSecurityDialog.xaml
    /// </summary>
    public partial class BlobSecurityDialog : Window
    {
        private string containerName;
        public string ContainerName
        {
            get
            {
                return containerName;
            }
            set
            {
                containerName = value;
                ContainerNameTextBox.Text = value;
            }
        }

        private string blobName;
        public string BlobName
        {
            get
            {
                return blobName;
            }
            set
            {
                blobName = value;
                BlobNameTextBox.Text = value;
            }
        }

        public StorageAccountViewModel ViewModel { get; set; }

        public ObservableCollection<PolicyView> PolicyViews { get; set; }

        public BlobSecurityDialog()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(BlobSecurityDialog_Loaded);
            this.DataContext = this;
        }

        void BlobSecurityDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            SetDefaultTimes();

            SharedAccessBlobPolicies policies = ViewModel.GetContainerAccessPolicies(containerName);

            switch(ViewModel.GetContainerAccessLevel(containerName))
            {
                default:
                case BlobContainerPublicAccessType.Off:
                    AccessPrivate.IsChecked = true;
                    break;
                case BlobContainerPublicAccessType.Blob:
                    AccessPublicBlob.IsChecked = true;
                    break;
                case BlobContainerPublicAccessType.Container:
                    AccessPublicContainer.IsChecked = true;
                    break;
            }

            PolicyViews = new ObservableCollection<PolicyView>();

            if (policies != null)
            {
                PolicyComboBox.Items.Add("--- no policy selected ---");

                foreach (KeyValuePair<string, SharedAccessBlobPolicy> policy in policies)
                {
                    PolicyViews.Add(new PolicyView(policy.Key, policy.Value));
                    PolicyComboBox.Items.Add(policy.Key);
                }
            }
            else
            {
                PolicyComboBox.Items.Add("--- no policies ---");
            }

            PolicyComboBox.SelectedIndex = 0;

            PoliciesGrid.ItemsSource = null;
            PoliciesGrid.ItemsSource = PolicyViews;

            Cursor = Cursors.Arrow;
        }

        private void SetDefaultTimes()
        {
            if (string.IsNullOrEmpty(StartTimeTextBox.Text))
            {
                StartTimeTextBox.Text = DateTime.Now.ToUniversalTime().ToString();
            }
            if (string.IsNullOrEmpty(ExpiryTimeTextBox.Text))
            {
                ExpiryTimeTextBox.Text = DateTime.Parse(StartTimeTextBox.Text).AddMinutes(60).ToString();
            }
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

        private void GenerateSignature_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CopyToClipboard.Visibility = System.Windows.Visibility.Collapsed;
                TestInBrowser.Visibility = System.Windows.Visibility.Collapsed;
                SignatureTextBlock.Visibility = System.Windows.Visibility.Collapsed;
                SignatureTextBox.Visibility = System.Windows.Visibility.Collapsed;
                SignatureTextBox.Text = String.Empty;

                SetDefaultTimes();

                string signature = String.Empty;
                if (PolicyComboBox.SelectedIndex > 0)
                {
                    signature = ViewModel.GenerateSharedAccessSignatureFromPolicy(
                                    ContainerNameTextBox.Text, 
                                    BlobNameTextBox.Text, 
                                    PolicyComboBox.SelectedItem as String);

                }
                else
                {
                    signature = ViewModel.GenerateSharedAccessSignature(
                        ContainerNameTextBox.Text, BlobNameTextBox.Text,
                        AllowRead.IsChecked.Value, AllowWrite.IsChecked.Value,
                        AllowDelete.IsChecked.Value, AllowList.IsChecked.Value,
                        DateTime.Parse(StartTimeTextBox.Text + " Z"),
                        DateTime.Parse(ExpiryTimeTextBox.Text + " Z")
                        );
                }
                SignatureTextBox.Text = signature;

                CopyToClipboard.Visibility = System.Windows.Visibility.Visible;
                TestInBrowser.Visibility = System.Windows.Visibility.Visible;
                SignatureTextBlock.Visibility = System.Windows.Visibility.Visible;
                SignatureTextBox.Visibility = System.Windows.Visibility.Visible;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to generate signature due to error:\r\n\r\n" + ex.ToString(),
                    "Error Generating Signature", MessageBoxButton.OK);
            }
        }

        private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(SignatureTextBox.Text);
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(SignatureTextBox.Text);
        }

        private void PolicyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PolicyComboBox != null && PolicyComboBox.SelectedIndex > 0)
            {
                PermissionsLabel.Visibility = Visibility.Collapsed;
                PermissionsControls.Visibility = Visibility.Collapsed;
                TimeLabel.Visibility = Visibility.Collapsed;
                TimeControls.Visibility = Visibility.Collapsed;
            }
            else
            {
                PermissionsLabel.Visibility = Visibility.Visible;
                PermissionsControls.Visibility = Visibility.Visible;
                TimeLabel.Visibility = Visibility.Visible;
                TimeControls.Visibility = Visibility.Visible;
            }
        }

        private void SavePolicies_Click(object sender, RoutedEventArgs e)
        {
            SharedAccessBlobPolicies policies = new SharedAccessBlobPolicies();

            if (PolicyViews != null)
            {
                foreach (PolicyView policyView in PolicyViews)
                {
                    if (!String.IsNullOrEmpty(policyView.PolicyName))
                    {
                        if (!policyView.Validate())
                        {
                            MessageBox.Show("Policy '" + policyView.PolicyName + "' has one or more fields that are not formatted properly.", "Validation Error", MessageBoxButton.OK);
                            return;
                        }
                        policies.Add(policyView.PolicyName, policyView.Policy);
                    }
                }
            }

            try
            {
                Cursor = Cursors.Wait;
                ViewModel.SetContainerAccessPolicies(containerName, policies);
                MessageBox.Show("Container policies have been saved.", "Save Complete", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Saving policies failed due to an error.\r\n\r\n" + ex.ToString(), "Save Error", MessageBoxButton.OK);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void NewPolicy_Click(object sender, RoutedEventArgs e)
        {
            PolicyViews.Add(new PolicyView("new", new SharedAccessBlobPolicy()));
        }


        private void DeletePolicy_Click(object sender, RoutedEventArgs e)
        {
            if (PolicyViews != null &&
               this.PoliciesGrid != null && this.PoliciesGrid.SelectedIndex != -1)
            {
                PolicyViews.Remove(this.PoliciesGrid.SelectedItem as PolicyView);
            }
        }


        private void DeleteAllPolicies_Click(object sender, RoutedEventArgs e)
        {
            if (PolicyViews != null)
            {
                PolicyViews.Clear();
            }
        }

        private void SetAccessLevel_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            BlobContainerPublicAccessType accessType;

            if (AccessPrivate.IsChecked.Value)
            {
                accessType = BlobContainerPublicAccessType.Off;
            }
            else if (AccessPublicBlob.IsChecked.Value)
            {
                accessType = BlobContainerPublicAccessType.Blob;
            }
            else 
            {
                accessType = BlobContainerPublicAccessType.Container;
            }
            ViewModel.SetContainerAccessLevel(containerName, accessType);
            Cursor = Cursors.Arrow;
        }

    }
}

