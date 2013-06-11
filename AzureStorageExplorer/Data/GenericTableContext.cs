using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System.Xml.Linq;

namespace Neudesic.AzureStorageExplorer.Data
{
    // Modified from Jai Haridas: http://social.msdn.microsoft.com/Forums/en-US/windowsazure/thread/481afa1b-03a9-42d9-ae79-9d5dc33b9297/
    public class GenericTableContext : TableServiceContext
    {
        public GenericTableContext(string baseAddress, StorageCredentials credentials)
            : base(baseAddress, credentials)
        {
            this.IgnoreMissingProperties = true;
            this.ReadingEntity += new EventHandler<System.Data.Services.Client.ReadingWritingEntityEventArgs>(GenericTableContext_ReadingEntity);
        }

        public GenericEntity GetFirstOrDefault(string tableName)
        {
            return (from r in this.CreateQuery<GenericEntity>(tableName)
                    select r).FirstOrDefault();
        }

        private void GenericTableContext_ReadingEntity(object sender, System.Data.Services.Client.ReadingWritingEntityEventArgs e)
        {
            // TODO: Make these statics   
            XNamespace AtomNamespace = "http://www.w3.org/2005/Atom";
            XNamespace AstoriaDataNamespace = "http://schemas.microsoft.com/ado/2007/08/dataservices";
            XNamespace AstoriaMetadataNamespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

            GenericEntity entity = e.Entity as GenericEntity;
            if (entity == null)
            {
                return;
            }

            entity.SetTableName(e.Data.Element(AtomNamespace + "link").Attribute("title").Value);

            // read each property, type and value in the payload   
            //var properties = e.Entity.GetType().GetProperties();
            //where properties.All(pp => pp.Name != p.Name.LocalName)
            var q = from p in e.Data.Element(AtomNamespace + "content")
                                    .Element(AstoriaMetadataNamespace + "properties")
                                    .Elements()
                    select new
                    {
                        Name = p.Name.LocalName,
                        IsNull = string.Equals("true", p.Attribute(AstoriaMetadataNamespace + "null") == null ? null : p.Attribute(AstoriaMetadataNamespace + "null").Value, StringComparison.OrdinalIgnoreCase),
                        TypeName = p.Attribute(AstoriaMetadataNamespace + "type") == null ? null : p.Attribute(AstoriaMetadataNamespace + "type").Value,
                        p.Value
                    };

            foreach (var dp in q)
            {
                entity[dp.Name] = GetType(dp.TypeName);
            }
        }


        private static Type GetType(string type)
        {
            if (type == null)
                return typeof(string);

            switch (type)
            {
                case "Edm.String": return typeof(string);
                case "Edm.Byte": return typeof(byte);
                case "Edm.SByte": return typeof(sbyte);
                case "Edm.Int16": return typeof(short);
                case "Edm.Int32": return typeof(int);
                case "Edm.Int64": return typeof(long);
                case "Edm.Double": return typeof(double);
                case "Edm.Single": return typeof(float);
                case "Edm.Boolean": return typeof(bool);
                case "Edm.Decimal": return typeof(decimal);
                case "Edm.DateTime": return typeof(DateTime);
                case "Edm.Binary": return typeof(byte[]);
                case "Edm.Guid": return typeof(Guid);

                default: throw new NotSupportedException("Not supported type " + type);
            }
        }


    }
}

