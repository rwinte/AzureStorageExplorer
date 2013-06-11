using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Windows;
using System.Windows.Resources;

namespace Neudesic.AzureStorageExplorer.Data
{
    // This class represents a mapping of file type to content type.

    public class ContentTypeMapping
    {
        public string FileType { get; set; }
        public string ContentType { get; set; }

        public static bool SetContentTypeAutomatically { get; set; }

        static ContentTypeMapping()
        {
            SetContentTypeAutomatically = true;
        }

        public ContentTypeMapping() { }

        public ContentTypeMapping(string fileType, string contentType)
        {
            this.FileType = fileType.Replace(".", String.Empty).ToLower();
            this.ContentType = contentType;
        }

        public ContentTypeMapping(string value)
        {
            string[] parts = value.Split(',');
            if (parts.Length > 0) this.FileType = parts[0].Replace(".", String.Empty).ToLower(); ;
            if (parts.Length > 1) this.ContentType = parts[1];
        }

        public override string ToString()
        {
            return FileType + "," + ContentType;
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

        public static List<ContentTypeMapping> DefaultValues()
        {
            List<ContentTypeMapping> defaultValues = new List<ContentTypeMapping>();

            try
            {
                StreamResourceInfo sri = Application.GetResourceStream(new Uri("/Data/MimeTypes.xml", UriKind.Relative));
                StreamReader reader = new StreamReader(sri.Stream);
                XDocument mimeTypeXml = XDocument.Load(reader);

                foreach (XElement mimeType in mimeTypeXml.Root.Elements())
                {
                    defaultValues.Add(
                        new ContentTypeMapping(
                            mimeType.Attribute("extension").Value, 
                            mimeType.Attribute("mimetype").Value
                        )
                    );
                }

            }
            catch (IOException)
            {
                defaultValues.Add(new ContentTypeMapping("htm", "text/html"));
                defaultValues.Add(new ContentTypeMapping("html", "text/html"));
                defaultValues.Add(new ContentTypeMapping("jpg", "image/jpeg"));
                defaultValues.Add(new ContentTypeMapping("jpeg", "image/jpeg"));
                defaultValues.Add(new ContentTypeMapping("png", "image/png"));
            }
            return defaultValues;
        }

        public static string GetFileContentType(string filetype)
        {
            if (String.IsNullOrEmpty(filetype)) return null;

            int pos = filetype.LastIndexOf('.');

            if (pos != -1)
            {
                filetype = filetype.Substring(pos + 1);
            }

            if (Values != null)
            {
                if (filetype.StartsWith("."))
                {
                    filetype = filetype.Substring(1);
                }

                foreach (ContentTypeMapping mapping in Values)
                {
                    if (string.Compare(mapping.FileType, filetype, true) == 0)
                    {
                        return mapping.ContentType;
                    }
                }
            }
            return null;
        }

    }
}
