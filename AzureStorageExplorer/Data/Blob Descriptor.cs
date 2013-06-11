using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure;
//using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Blob;
//using Microsoft.WindowsAzure.StorageClient;
using Neudesic.AzureStorageExplorer.ViewModel;

namespace Neudesic.AzureStorageExplorer.Data
{
    // This class defines a blob description used for data binding blob items to the right pane
    // grid view.

    public class BlobDescriptor
    {
        public CloudBlockBlob CloudBlob { get; set; }

        public string Name
        {
            get
            {
                string name = BlobName(CloudBlob); 
                name = StorageAccountViewModel.NormalizeBlobName(name);
                return name;
            }
        }

        public static string BlobName(CloudBlockBlob blob)
        {
            string name = blob.Uri.LocalPath;

            name = StorageAccountViewModel.NormalizeBlobName(name);
            if (!name.StartsWith("/")) name = "/" + name;

            string containerName = StorageAccountViewModel.NormalizeContainerName(blob.Container.Name) + "/";
            if (!containerName.StartsWith("/")) containerName = "/" + containerName;

            if (name.StartsWith(containerName))
            {
                name = name.Substring(containerName.Length);
            }
            return name;
        }

        public string LastModifiedText
        {
            get
            {
                return CloudBlob.Properties.LastModified.ToString();
            }
        }

        public long Length
        {
            get
            {
                return CloudBlob.Properties.Length;
            }
        }

        public string LengthText
        {
            get
            {
                const long KB = 1024;
                const long MB = 1024 * 1024;
                const long GB = 1024 * 1024 * 1024;

                long length = CloudBlob.Properties.Length;
                if (length < KB)
                {
                    return length.ToString() + " bytes";
                }
                else if (length < MB)
                {
                    return (length / KB).ToString() + " KB";
                }
                else if (length < GB)
                {
                    return (length / MB).ToString() + " MB";
                }
                else
                {
                    return (length / GB).ToString() + " GB";
                }
            }
        }

        public string ContentType
        {
            get
            {
                return CloudBlob.Properties.ContentType;
            }
        }

        public string ContentEncoding
        {
            get
            {
                return CloudBlob.Properties.ContentEncoding;
            }
        }

        public string ContentLanguage
        {
            get
            {
                return CloudBlob.Properties.ContentLanguage;
            }
        }

        public BlobDescriptor(CloudBlockBlob cloudBlob)
        {
            this.CloudBlob = cloudBlob;
        }
    }
}
