using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Neudesic.AzureStorageExplorer;
using Neudesic.AzureStorageExplorer.Data;
using Neudesic.AzureStorageExplorer.ViewModel;
using Microsoft.WindowsAzure.Storage;

namespace Neudesic.AzureStorageExplorer.ViewModel
{
    /// <summary>
    /// Represents an actionable item displayed by a View.
    /// </summary>
    public class MessageViewModel : ViewModelBase
    {
        private CloudQueueMessage Message = null;

        private byte[] Bytes = null;
        private string Text = string.Empty;

        public Visibility TextSpinnerVisibility { get; set; }
        public Visibility PreviewTextVisibility { get; set; }

        public Visibility ImageSpinnerVisibility { get; set; }
        public Visibility PreviewImageVisibility { get; set; }

        private BlobDescriptor _blob;
        public BlobDescriptor Blob
        {
            get
            {
                return _blob;
            }
            set
            {
                _blob = value;
            }
        }

        public string Title
        {
            get
            {
                if (Message != null)
                {
                    return "Message Detail - " + Message.Id;
                }
                else
                {
                    return "Message Detail";
                }
            }
        }

        private bool imageVisible = false;
        public bool ImageVisible 
        { 
            get
            {
                return imageVisible;
            }
            set
            {
                if (value && !imageVisible)
                {
                    Bytes = null;
                    BackgroundWorker background = new BackgroundWorker();
                    background.DoWork += new DoWorkEventHandler(Background_LoadImage);
                    background.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Background_LoadImageCompleted);
                    background.RunWorkerAsync();
                }

                imageVisible = value;
            }
        }

        private bool videoVisible;
        public bool VideoVisible
        {
            get
            {
                return videoVisible;
            }
            set
            {
                videoVisible = value;

                if (value)
                {
                    OnPropertyChanged("PreviewVideo");
                }
            }
        }

        void Background_LoadText(object sender, DoWorkEventArgs e)
        {
            try
            {
                Encoding encoding = Encoding.UTF8;
                MemoryStream stream = new MemoryStream();
                Blob.CloudBlob.DownloadToStream(stream);
                Text = encoding.GetString(stream.ToArray());
            }
            catch (Exception)
            {
                Text = null;
            }
        }

        void Background_LoadTextCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            TextSpinnerVisibility = Visibility.Collapsed;
            PreviewTextVisibility = Visibility.Visible;
            OnPropertyChanged("TextSpinnerVisibility");
            OnPropertyChanged("PreviewTextVisibility");
            OnPropertyChanged("PreviewText");
        }


        void Background_LoadImage(object sender, DoWorkEventArgs e)
        {
            try
            {
                MemoryStream stream = new MemoryStream();
                Blob.CloudBlob.DownloadToStream(stream);
                Bytes = stream.ToArray();
            }
            catch (Exception)
            {
                Bytes = null;
            }
        }

        void Background_LoadImageCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ImageSpinnerVisibility = Visibility.Collapsed;
            PreviewImageVisibility = Visibility.Visible;
            OnPropertyChanged("ImageSpinnerVisibility");
            OnPropertyChanged("PreviewImageVisibility");
            OnPropertyChanged("PreviewImage");
        }

        public Byte[] PreviewImage
        {
            get
            {
                if (ImageVisible)
                {
                    return Bytes;
                }
                else
                {
                    return null;
                }
            }
        }

        public string PreviewVideo
        {
            get
            {
                if (VideoVisible)
                {
                    return Blob.CloudBlob.Uri.AbsoluteUri;
                }
                else
                {
                    return null;
                }
            }
        }

        private bool textVisible = false;
        public bool TextVisible
        {
            get
            {
                return textVisible;
            }
            set
            {
                if (value && !textVisible)
                {
                    Text = String.Empty;
                    BackgroundWorker background = new BackgroundWorker();
                    background.DoWork += new DoWorkEventHandler(Background_LoadText);
                    background.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Background_LoadTextCompleted);
                    background.RunWorkerAsync();
                }

                textVisible = value;
            }
        }

        public string MessageText
        {
            get
            {
                if (Message != null)
                {
                    return Message.AsString;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        private List<Property> properties = null;
        public ObservableCollection<Property> Properties
        {
            get
            {
                if (properties == null)
                {
                    properties = new List<Property>();
                    properties.Add(new Property("(Id)", Message.Id));
                    properties.Add(new Property("DequeueCount", Message.DequeueCount.ToString()));
                    properties.Add(new Property("ExpirationTime", Message.ExpirationTime==null ? String.Empty : Message.ExpirationTime.ToString()));
                    properties.Add(new Property("InsertionTime", Message.InsertionTime == null ? String.Empty : Message.InsertionTime.ToString()));
                    properties.Add(new Property("NextVisibleTime", Message.NextVisibleTime == null ? String.Empty : Message.InsertionTime.ToString()));
                    properties.Add(new Property("PopReceipt", Message.PopReceipt == null ? String.Empty : Message.InsertionTime.ToString()));
                }
                return new ObservableCollection<Property>(properties);
            }
            set
            {
                properties = new List<Property>(value);
            }
        }

        public MessageViewModel(CloudQueueMessage message)
        {
            Message = message;
            OnPropertyChanged("MessageText");

            base.DisplayName = message.Id;

        }

        public override string ToString()
        {
            return Blob.CloudBlob.Uri.LocalPath;
        }

        #region Blob Actions

        public void SaveText(string text)
        {
            Encoding encoding = Encoding.UTF8;
            MemoryStream stream = new MemoryStream(encoding.GetBytes(text));
            Blob.CloudBlob.UploadFromStream(stream);
            Text = text;
            OnPropertyChanged("PreviewText");
        }

        public void SaveTextFile(string filename)
        {
            SaveText(File.ReadAllText(filename));
        }

        public void SaveImageFile(string filename)
        {
            FileStream stream = File.OpenRead(filename);
            Blob.CloudBlob.UploadFromStream(stream);
            OnPropertyChanged("PreviewImage");
        }

        public void SaveVideoFile(string filename)
        {
            FileStream stream = File.OpenRead(filename);
            Blob.CloudBlob.UploadFromStream(stream);
            OnPropertyChanged("PreviewImage");
        }

        public void SaveProperties()
        {
            CloudBlockBlob blob = Blob.CloudBlob;
            foreach (Property prop in Properties)
            {
                switch (prop.PropertyName)
                {
                    case "(Name)":
                        if (prop.PropertyValue != BlobDescriptor.BlobName(blob))
                        {
                            CloudBlobContainer container = blob.Container;
                            if (container != null)
                            {
                                CloudBlockBlob newBlob = container.GetBlockBlobReference(prop.PropertyValue);
                                Encoding encoding = Encoding.UTF8;
                                MemoryStream stream = new MemoryStream(encoding.GetBytes(string.Empty));
                                Blob.CloudBlob.UploadFromStream(stream);
                                newBlob.StartCopyFromBlob(blob);
                                blob.Delete();
                                blob = newBlob;
                            }
                        }
                        break;
                    case "Blob Type":
                        // Can't be set - blob.Properties.BlobType = prop.PropertyValue;
                        break;
                    case "CacheControl":
                        blob.Properties.CacheControl = prop.PropertyValue;
                        break;
                    case "ContentEncoding":
                        blob.Properties.ContentEncoding = prop.PropertyValue;
                        break;
                    case "ContentLanguage":
                        blob.Properties.ContentLanguage = prop.PropertyValue;
                        break;
                    case "ContentMD5":
                        blob.Properties.ContentMD5 = prop.PropertyValue;
                        break;
                    case "ContentType":
                        blob.Properties.ContentType = prop.PropertyValue;
                        break;
                    case "ETag":
                        //Can't be set - blob.Properties.ETag = prop.PropertyValue;
                        break;
                    case "LastModifiedUtc":
                        //Can't be set
                        break;
                    case "Length":
                        break;
                }
            }
            blob.SetProperties();
        }

        #endregion

    }
}