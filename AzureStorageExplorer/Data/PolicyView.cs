using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Windows;
using System.Windows.Resources;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Neudesic.AzureStorageExplorer.Data
{
    // This class represents a mapping of file type to content type.

    public class PolicyView : INotifyPropertyChanged 
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string policyName;
        public string PolicyName 
        { 
            get
            {
                return policyName;
            }
            set
            {
                policyName = value;
                NotifyPropertyChanged("PolicyName");
            }
        }

        public bool AllowRead 
        { 
            get
            {
                return ((Policy.Permissions & SharedAccessBlobPermissions.Read) != 0);
            }
            set
            {
                if (value)
                {
                    Policy.Permissions |= SharedAccessBlobPermissions.Read;
                }
                else
                {
                    Policy.Permissions &= ~SharedAccessBlobPermissions.Read;
                }
                NotifyPropertyChanged("AllowRead");
            }
        }

        public bool AllowWrite
        {
            get
            {
                return ((Policy.Permissions & SharedAccessBlobPermissions.Write) != 0);
            }
            set
            {
                if (value)
                {
                    Policy.Permissions |= SharedAccessBlobPermissions.Write;
                }
                else
                {
                    Policy.Permissions &= ~SharedAccessBlobPermissions.Write;
                }
                NotifyPropertyChanged("AllowWrite");
            }
        }

        public bool AllowDelete
        {
            get
            {
                return ((Policy.Permissions & SharedAccessBlobPermissions.Delete) != 0);
            }
            set
            {
                if (value)
                {
                    Policy.Permissions |= SharedAccessBlobPermissions.Delete;
                }
                else
                {
                    Policy.Permissions &= ~SharedAccessBlobPermissions.Delete;
                }
                NotifyPropertyChanged("AllowDelete");
            }
        }

        public bool AllowList
        {
            get
            {
                return ((Policy.Permissions & SharedAccessBlobPermissions.List) != 0);
            }
            set
            {
                if (value)
                {
                    Policy.Permissions |= SharedAccessBlobPermissions.List;
                }
                else
                {
                    Policy.Permissions &= ~SharedAccessBlobPermissions.List;
                }
                NotifyPropertyChanged("AllowList");
            }
        }

        public string startTime;
        public string StartTime 
        {
            get
            {
                return startTime;
            }
            set
            {
                startTime = value;
                NotifyPropertyChanged("StartTime");
            }
        }

        public string expiryTime;
        public string ExpiryTime
        {
            get
            {
                return expiryTime;
            }
            set
            {
                expiryTime = value;
                NotifyPropertyChanged("ExpiryTime");
            }
        }

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy();
        public SharedAccessBlobPolicy Policy
        { 
            get
            {
                return policy;    
            }
            set
            {
                policy = value;
            }
        }

        public PolicyView() { }

        public PolicyView(string name, SharedAccessBlobPolicy policy)
        {
            PolicyName = name;
            Policy = policy;
            StartTime = Policy.SharedAccessStartTime.ToString();
            ExpiryTime = Policy.SharedAccessExpiryTime.ToString();
        }

        public override string ToString()
        {
            return PolicyName;
        }

        private static List<ContentTypeMapping> values;
        public static List<ContentTypeMapping> Values
        {
            get
            {
                return values;
            }
            set
            {
                values = value;
            }
        }

        public bool Validate()
        {
            try
            {
                Policy.SharedAccessStartTime = DateTime.Parse(StartTime + " Z");
                Policy.SharedAccessExpiryTime = DateTime.Parse(ExpiryTime + " Z");
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

    }
}
