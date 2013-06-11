using System;
using System.Windows.Input;
using Neudesic.AzureStorageExplorer;
using Neudesic.AzureStorageExplorer.Data;
using Neudesic.AzureStorageExplorer.ViewModel;

namespace Neudesic.AzureStorageExplorer.ViewModel
{
    /// <summary>
    /// Represents an actionable item displayed by a View.
    /// </summary>
    public class AccountViewModel : ViewModelBase, IComparable
    {
        private StorageAccount _account;
        public StorageAccount Account
        {
            get
            {
                return _account;
            }
        }

        public AccountViewModel(string displayName, string key, bool useHttps, bool blobContainersUpgraded, ICommand command)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            _account = new StorageAccount(displayName, key, useHttps, blobContainersUpgraded);

            base.DisplayName = displayName;
            this.Key = key;
            this.Command = command;
        }

        public ICommand Command { get; private set; }

        public string AccountName 
        { 
            get
            {
                return base.DisplayName;
            }
            set
            {
                base.DisplayName = value;
                _account.Name = value;
            }
        }

        public string Key
        {
            get
            {
                return _account.Key;
            }
            set
            {
                _account.Key = value;
            }
        }

        public bool UseHttps
        {
            get
            {
                return _account.UseHttps;
            }
            set
            {
                _account.UseHttps = value;
            }
        }

        public bool BlobContainersUpgraded
        {
            get
            {
                return _account.BlobContainersUpgraded;
            }
            set
            {
                _account.BlobContainersUpgraded = value;
            }
        }

        public override string ToString()
        {
            return AccountName;
        }

        public int CompareTo(object obj)
        {
            if (obj is AccountViewModel)
            {
                AccountViewModel avm = obj as AccountViewModel;
                return string.Compare(AccountName, avm.AccountName);
            }
            else
            {
                return 0;
            }
        }
    }
}