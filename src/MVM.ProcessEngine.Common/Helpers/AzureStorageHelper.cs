using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MVM.ProcessEngine.Common.Helpers
{
    public static class AzureStorageHelper
    {

        /// <summary>
        /// Set AppSetting (Medatada) For Tenant
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="accountStorageConnection"></param>
        public static void SetAppSettingForTenant(string tenant, string accountStorageConnection)
        {

            var storageAccount = CloudStorageAccount.Parse(accountStorageConnection);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("AppSettings");

            var customerFilter = TableQuery.GenerateFilterCondition(
                "PartitionKey",
                QueryComparisons.Equal,
                tenant);

            var combinedFilter = customerFilter;
            var query = new TableQuery().Where(combinedFilter);

            var keys = table.ExecuteQuery(query).Select(k => new Key { Name = k.RowKey, Value = k["value"].StringValue });
            GestorCalculosServiceLocator.GetService<AppSetting>("appSetting").Metadata[tenant] = keys;

        }



        /// <summary>
        /// Obtiene un Archivo almacenado en Azure Blob Storage y retorna el contenido como un string.
        /// Usado para obener configuracions XML
        /// </summary>
        /// <param name="blobName">Nombre del archivo</param>
        /// <returns></returns>
        public static string GetBlockBlobAsText(string tenant , string blobName)
        {

            string blobAsText = "";

            var accountName = GestorCalculosHelper.GetMetadataValue(tenant,"AzureStorageAccountName", true);
            var accountKey = GestorCalculosHelper.GetMetadataValue(tenant,"AzureStorageAccountKey", true);
            var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};");
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("xml");
            var blob = container.GetBlockBlobReference(blobName);

            if (blob.Exists())
            {
                blobAsText = blob.DownloadText();

            }

            return blobAsText;
        }


        /// <summary>
        /// Get or Create a table Log () from Azure Table Storage 
        /// </summary>
        /// <returns></returns>
        public static CloudTable GetLogTable(string tenant) {

            var accountName = GestorCalculosHelper.GetMetadataValue(tenant,"AzureStorageAccountName", true);
            var accountKey = GestorCalculosHelper.GetMetadataValue(tenant,"AzureStorageAccountKey", true);
            var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};");
            var tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference("ALogProcessEngine" + DateTime.Now.ToString("yyyyMMdd"));

            table.CreateIfNotExists();

            return table;
        }

        public async static Task<string> CreateBlob(string tenant, string filename, string blobName, byte[] fileBytes)
        {
            var accountName = GestorCalculosHelper.GetMetadataValue(tenant, "AzureStorageAccountName", true);
            var accountKey = GestorCalculosHelper.GetMetadataValue(tenant, "AzureStorageAccountKey", true);
            var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};");
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(blobName);

            var blockBlob = container.GetBlockBlobReference(filename);

            await blockBlob.UploadFromByteArrayAsync(fileBytes, 0, fileBytes.Length);

            return blockBlob.Uri.AbsoluteUri;
        }
    }
}
