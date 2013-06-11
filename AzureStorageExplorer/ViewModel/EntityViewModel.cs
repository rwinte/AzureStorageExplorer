using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Neudesic.AzureStorageExplorer;
using Neudesic.AzureStorageExplorer.Data;
using Neudesic.AzureStorageExplorer.ViewModel;
using Microsoft.WindowsAzure.Storage;

namespace Neudesic.AzureStorageExplorer.ViewModel
{
    /// <summary>
    /// Represents an actionable item displayed by a View.
    /// </summary>
    public class EntityViewModel : ViewModelBase
    {
        private List<Column> ColumnNames = null;

        private GenericEntity entity;
        public GenericEntity Entity 
        {
            get
            {
                return entity;
            }
            set
            {
                entity = value;
                Properties.Clear();
                Properties.Add(new Property("PartitionKey", Entity.PartitionKey));
                Properties.Add(new Property("RowKey", Entity.RowKey));
                Properties.Add(new Property("Timestamp", Entity.Timestamp.ToShortDateString()));
                if (entity.Properties != null)
                {
                    string propertyValue;
                    foreach(KeyValuePair<string, object> kvp in entity.Properties)
                    {
                        propertyValue = (kvp.Value == null) ? String.Empty : kvp.Value.ToString();
                        Properties.Add(new Property(kvp.Key, Column.StandardType(kvp.Value), propertyValue));
                    }

                    if (ColumnNames != null)
                    {
                        bool haveProperty;
                        foreach (Column column in ColumnNames)
                        {
                            haveProperty = false;
                            foreach (Property property in Properties)
                            {
                                if (property.PropertyName == column.Name)
                                {
                                    haveProperty = true;
                                    break;
                                }
                            }
                            if (!haveProperty)
                            {
                                Properties.Add(new Property(column.Name, column.Type, "null"));
                            }
                        }
                    }
                }
            }
        }

        // Return an updated entity that reflects user property editing.

        public GenericEntity UpdatedEntity
        {
            get
            {
                GenericEntity entity = new GenericEntity();
                string value;
                foreach (Property prop in Properties)
                {
                    switch (prop.PropertyName)
                    {
                        case "PartitionKey":
                            value = prop.PropertyValue;
                            if (value == null) value = String.Empty;
                            entity.PartitionKey = value;
                            break;
                        case "RowKey":
                            value = prop.PropertyValue;
                            if (value == null) value = String.Empty;
                            entity.RowKey = value;
                            break;
                        case "Timestamp":
                            value = prop.PropertyValue;
                            if (value == null) value = String.Empty;
                            //entity.Timestamp = value;
                            break;
                        default:
                            if (!string.IsNullOrEmpty(prop.PropertyName))
                            {
                                value = prop.PropertyValue;
                                if (value == null || value == "null")
                                {
                                    entity[prop.PropertyName] = null;
                                }
                                else
                                {
                                    entity[prop.PropertyName] = Column.ConvertToStandardType(value, prop.PropertyType);
                                }
                            }
                            break;
                    }
                }
                return entity;
            }
        }

        public ObservableCollection<Property> Properties { get; set; }

        public EntityViewModel(GenericEntity entity, Dictionary<string, Column> columnNames)
        {
            Properties = new ObservableCollection<Property>();
            ColumnNames = new List<Column>();
            if (columnNames != null)
            {
                foreach (KeyValuePair<string, Column> kvp in columnNames)
                {
                    if (!String.IsNullOrEmpty(kvp.Key))
                    {
                        ColumnNames.Add(kvp.Value);
                    }
                }
            }
            
            Entity = entity;

            base.DisplayName = entity.Key();
        }

        public override string ToString()
        {
            return Entity.Key();
        }

    }
}