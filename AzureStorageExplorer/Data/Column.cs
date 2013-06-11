using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neudesic.AzureStorageExplorer.Data
{
    // This class defines a table column, used to collect schema about entities as they are queried.

    public class Column
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public Column() 
        { 
            this.Type = "string";  
        }

        public Column(string name)
        {
            this.Name = name;
            this.Type = "string";
        }

        public Column(string name, object value)
        {
            this.Name = name;
            if (value == null)
            {
                this.Type = "string";
            }
            else
            {
                this.Type = StandardType(value);
            }
        }

        public Column(string name, string typeName, object value)
        {
            this.Name = name;
            this.Type = this.Type = StandardTypeName(typeName);
        }

        // Returns an object's type from a standard set of standard type names ("string" if not recognized).

        public static string StandardType(object obj)
        {
            if (obj == null) return "string";

            switch (obj.GetType().Name.ToLower().Replace("edm.", String.Empty))
            {
                default:
                    return "string";
                case "string":
                case "edm.string":
                    return "string";
                case "byte": 
                    return "byte";
                case "sbyte": 
                    return "sbyte";
                case "int":
                case "int32":
                    return "int";
                case "int16": 
                    return "int16";
                case "int64": 
                    return "int64";
                case "double": 
                    return "double";
                case "single": 
                    return "single";
                case "bool":
                case "boolean": 
                    return "bool";
                case "decimal": 
                    return "decimal";
                case "datetime": 
                    return "datetime";
                case "binary": 
                    return "binary";
                case "guid": 
                    return "guid";
            }
        }

        // Convert a type name to a standard type name we use internally for display and selection.
        // Turn the Edm.<type> names into internal standard names we use in combo boxes ("string", "bool", etc.)

        public static string StandardTypeName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return "string";

            switch (typeName.ToLower().Replace("edm.", String.Empty))
            {
                default:
                    return "string";
                case "string":
                case "edm.string":
                    return "string";
                case "byte": 
                    return "byte";
                case "sbyte": 
                    return "sbyte";
                case "int":
                case "int32":
                    return "int";
                case "int16":  
                    return "int16";
                case "int64":  
                    return "int64";
                case "double":  
                    return "double";
                case "single": 
                    return "single";
                case "bool":
                case "boolean":  
                    return "bool";
                case "decimal": 
                    return "decimal";
                case "datetime": 
                    return "datetime";
                case "binary": 
                    return "binary";
                case "guid":  
                    return "guid";
            }
        }

        // Convert a standard type name to an Edm type name we use in serialized messages.
        // Turn the internal standard names ("string", etc.) into Edm.<type> names.

        public static string EdmTypeName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return "Edm.String";

            switch (typeName.ToLower())
            {
                default:
                    return "Edm.String";
                case "string":
                    return "Edm.String";
                case "byte":
                    return "Edm.Byte";
                case "sbyte":
                    return "Edm.SByte";
                case "int":
                case "int32":
                    return "Edm.Int32";
                case "int16":
                    return "Edm.Int16";
                case "int64":
                    return "Edm.Int64";
                case "double":
                    return "Edm.Double";
                case "single":
                    return "Edm.Single";
                case "bool":
                case "boolean":
                    return "Edm.Boolean";
                case "decimal":
                    return "Edm.Decimal";
                case "datetime":
                    return "Edm.DateTime";
                case "binary":
                    return "Edm.Binary";
                case "guid":
                    return "Edm.Guid";
            }
        }

        // Convert an object to a desired type. 

        public static object ConvertToStandardType(string value, string type)
        {
            try
            {
                switch (StandardTypeName(type))
                {
                    default:
                    case "string":
                        return value;
                    case "byte": 
                        return Convert.ToByte(value);
                    case "sbyte": 
                        return Convert.ToSByte(value);
                    case "int":
                    case "int32": 
                        return Convert.ToInt32(value);
                    case "int16":
                        return Convert.ToInt16(value);
                    case "int64": 
                        return Convert.ToInt64(value);
                    case "double": 
                        return Convert.ToDouble(value);
                    case "single": 
                        return Convert.ToSingle(value);
                    case "bool":
                    case "boolean": 
                        return Convert.ToBoolean(value);
                    case "decimal": 
                        return Convert.ToDecimal(value);
                    case "datetime": 
                        return Convert.ToDateTime(value);
                    case "binary": 
                        {
                            string[] values = value.Split(',');
                            List<byte> bytes = new List<byte>();
                            foreach (string val in values)
                            {
                                bytes.Add(Convert.ToByte(val));
                            }
                            return bytes.ToArray();
                        }
                    case "guid": 
                        return new Guid(value);
                }
            }
            catch (FormatException)
            {
                return value;
            }
            catch (Exception)
            {
                return value;
            }
        }

    }
}
