using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure;
//using Microsoft.WindowsAzure.StorageClient;

namespace Neudesic.AzureStorageExplorer.Data
{
    // This class holds table entity properties for display and editing.

    public class Property : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string propertyName;
        public string PropertyName
        {
            get
            {
                return propertyName;
            }
            set
            {
                propertyName = value;
                NotifyPropertyChanged("PropertyName");
            }
        }

        private string propertyValue;
        public string PropertyValue
        {
            get
            {
                return propertyValue;
            }
            set
            {
                propertyValue = value;
                NotifyPropertyChanged("PropertyValue");
            }
        }

        private string propertyType = "string";
        public string PropertyType
        {
            get
            {
                return propertyType;
            }
            set
            {
                propertyType = value;
                NotifyPropertyChanged("PropertyType");
            }
        }

        private bool isReadOnly;
        public bool IsReadOnly
        {
            get
            {
                return isReadOnly;
            }
            set
            {
                isReadOnly = value;
                NotifyPropertyChanged("IsReadOnly");
            }
        }

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public Property() { }

        public Property(string name, string value)
        {
            this.propertyName = name;
            this.propertyValue = value;
            switch (name)
            {
                case "(Name)":
                case "CacheControl":
                case "ContentEncoding":
                case "ContentLanguage":
                case "ContentMD5":
                case "ContentType":
                    isReadOnly = false;
                    break;
                default:
                    isReadOnly = true;
                    break;
            }
        }

        public Property(string name, string type, string value)
        {
            this.propertyName = name;
            this.propertyType = type;
            this.propertyValue = value;
            switch (name)
            {
                case "(Name)":
                case "CacheControl":
                case "ContentEncoding":
                case "ContentLanguage":
                case "ContentMD5":
                case "ContentType":
                    isReadOnly = false;
                    break;
                default:
                    isReadOnly = true;
                    break;
            }
        }
    }
}
