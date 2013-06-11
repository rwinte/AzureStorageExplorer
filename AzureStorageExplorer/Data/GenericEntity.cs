﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Text;
using System.Net.Mime;
using System.Configuration;
using System.IO;
using System.Net;
using System.Collections.Specialized;
using System.Data.Services.Common;
using System.Data.Services.Client;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Microsoft.WindowsAzure.Storage;
﻿using Microsoft.WindowsAzure.Storage.Table.DataServices;

namespace Neudesic.AzureStorageExplorer.Data
{
    // This class allows table entities to be queried without knowing the schema in advance.

    [DataServiceKey("PartitionKey", "RowKey")]
    public class GenericEntity : TableServiceEntity
    {
        string tableName;
        public string GetTableName()
        {
            return tableName;
        }

        public void SetTableName(string tableName)
        {
            this.tableName = tableName;
        }

        Dictionary<string, object> properties = new Dictionary<string, object>();
        public Dictionary<string, object> Properties
        {
            get
            {
                return properties;
            }
            set
            {
                properties = value;
            }
        }

        public GenericEntity() { }

        public GenericEntity(GenericEntity e)
        {
            this.PartitionKey = e.PartitionKey;
            this.RowKey = e.RowKey;
            this.Timestamp = e.Timestamp;
            this.properties = e.properties;
        }

        internal object this[string key]
        {
            get
            {
                return this.properties[key];
            }

            set
            {
                this.properties[key] = value;
            }
        }

        public string Key()
        {
            string key = string.Empty;
            if (RowKey != null)
            {
                key = RowKey;
            }
            if (PartitionKey != null)
            {
                key = PartitionKey + "|" + key;
            }
            return key;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string value;
            int count = 0;
            if (properties != null)
            {
                foreach (KeyValuePair<string, object> kvp in properties)
                {
                    if (count > 0)
                        sb.Append("|");
                    if (kvp.Value == null)
                        value = string.Empty;
                    else
                        value = kvp.Value.ToString();
                    sb.Append(kvp.Key + "=" + value);
                    count++;
                }
            }
            return sb.ToString();
        }

        public string ToXml()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<entity>");
            sb.AppendLine("  <PartitionKey>" + this.PartitionKey + "</PartitionKey>");
            sb.AppendLine("  <RowKey>" + this.RowKey + "</RowKey>");

            string value;
            if (properties != null)
            {
                foreach (KeyValuePair<string, object> kvp in properties)
                {
                    if (kvp.Value == null)
                        value = string.Empty;
                    else
                        value = kvp.Value.ToString();
                    sb.AppendLine("  <" + kvp.Key + ">" + value + "</" + kvp.Key + ">");
                }
            }

            sb.Append("</entity>");
            return sb.ToString();
        }

        public string ToXmlBinaryValues()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<entity>"); 
            sb.AppendLine("  <PartitionKey>" + this.PartitionKey + "</PartitionKey>");
            sb.AppendLine("  <RowKey>" + this.RowKey + "</RowKey>");

            string value;
            if (properties != null)
            {
                foreach (KeyValuePair<string, object> kvp in properties)
                {
                    if (kvp.Value == null)
                        value = string.Empty;
                    else
                        value = kvp.Value.ToString();

                    value = DisplayCharsAsBytes(value.ToCharArray());

                    sb.AppendLine("  <" + kvp.Key + ">" + value + "</" + kvp.Key + ">");
                }
            }

            sb.AppendLine("</entity>");
            return sb.ToString();
        }

        /// Convert a byte sequence into a displayable multi-line string showing the values.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>

        private string DisplayBytes(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();

            for (int b = 0; b < bytes.Length; b++)
                sb.Append(String.Format("{0:X2}", bytes[b]) + " ");
            return sb.ToString();
        }

        private string DisplayCharsAsBytes(char[] chars)
        {
            StringBuilder sb = new StringBuilder();

            for (int b = 0; b < chars.Length; b++)
                sb.Append(String.Format("{0:X4}", Convert.ToInt64(char.GetNumericValue(chars[b]))) + " ");
            return sb.ToString();
        }
    }

}

