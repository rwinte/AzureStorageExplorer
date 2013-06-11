using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure.StorageClient.Protocol;
using Microsoft.WindowsAzure.StorageClient.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Neudesic.AzureStorageExplorer
{
    public class Security
    {
        private CloudStorageAccount CloudStorageAccount;
        private RSACryptoServiceProvider RSAProvider;
        private static BinaryFormatter bf = new BinaryFormatter();

        // Credit: Jonathon Wiggs MSDN Article http://msdn.microsoft.com/en-us/magazine/ee291586.aspx

        public Security()
        {
            CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) => 
            { 
                configSetter(Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.GetConfigurationSettingValue(configName)); 
            });

            CloudStorageAccount = CloudStorageAccount.FromConfigurationSetting("SaaS");

            CreateRSAProvider();
        }

        public void CreateRSAProvider()
        {
            CspParameters CspParam = new CspParameters();
            CspParam.KeyContainerName = "SampleContainerName";
            CspParam.Flags = CspProviderFlags.UseUserProtectedKey;
            RSAProvider = new RSACryptoServiceProvider(CspParam);

            //Authorization = "[SharedKey|SharedKeyLite] <AccountName>:<Signature>";

            //Signature = "PUT\n\ntext/plain; charset=UTF-8\n\nx-ms-Date:Fri, 12 Sep 2009 22:33:41 GMT\nx-ms-meta-m1:v1\nx-ms-meta-m2:v2\n/exampleaccount/storageclientcontainer/keys.txt";
        }

        // Generate a cryptographically decent random number.

        public static int GenerateRandomNumber()
        {
            byte[] GeneratedBytes = new byte[4];
            RNGCryptoServiceProvider CSP = new RNGCryptoServiceProvider();
            CSP.GetBytes(GeneratedBytes);
            return BitConverter.ToInt32(GeneratedBytes, 0);
        }

        // Encyrpt a buffer using supplied key and initialization vector.

        public static byte[] SampleEncrypt(byte[] dataBuffer, byte[] Key, byte[] IV)
        {
            MemoryStream InMemory = new MemoryStream();
            Rijndael SymetricAlgorithm = Rijndael.Create();
            SymetricAlgorithm.Key = Key;
            SymetricAlgorithm.IV = IV;
            CryptoStream EncryptionStream = new CryptoStream(InMemory,
              SymetricAlgorithm.CreateEncryptor(), CryptoStreamMode.Write);
            EncryptionStream.Write(dataBuffer, 0, dataBuffer.Length);
            EncryptionStream.Close();
            byte[] ReturnBuffer = InMemory.ToArray();
            return ReturnBuffer;
        }

        public static string EncryptString(string data, byte[] Key, byte[] IV)
        {
            byte[] dataBuffer = ObjectToByteArray(data);

            MemoryStream InMemory = new MemoryStream();
            Rijndael SymetricAlgorithm = Rijndael.Create();
            SymetricAlgorithm.Key = Key;
            SymetricAlgorithm.IV = IV;
            CryptoStream EncryptionStream = new CryptoStream(InMemory,
              SymetricAlgorithm.CreateEncryptor(), CryptoStreamMode.Write);
            EncryptionStream.Write(dataBuffer, 0, dataBuffer.Length);
            EncryptionStream.Close();
            byte[] ReturnBuffer = InMemory.ToArray();
            
            return ByteArrayToObject(ReturnBuffer) as string;
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            if ((hex.Length % 2) != 0)
            {
                hex = "0" + hex;
            }

            return Enumerable.Range(0, hex.Length).
                   Where(x => 0 == x % 2).
                   Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).
                   ToArray();
        }

        // Convert an object to a byte array
        public static byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
        // Convert a byte array to an Object
        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)bf.Deserialize(memStream);
            return obj;
        }


    }
}
