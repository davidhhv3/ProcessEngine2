using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using MVM.ProcessEngine.Common.Helpers;
using MVM.ProcessEngine.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Plugin
{
    public class StorageQueueMessagePublisher : IMessagePublisher
    {
        public Guid ConversationHandleId { get; set; }

        public void Publish(string tenant, object message)
        {
            var queueName = message.GetValueFromProperty("IdProcesoGestor")?.ToString();

            if (queueName.IsNullOrEmptyTrim())
            {
                throw new Exception("No ha específicado el identificador del proceso para definir nombre de la cola.");
            }

            var accountName = GestorCalculosHelper.GetMetadataValue(tenant, "AzureStorageAccountName", true);
            var accountKey = GestorCalculosHelper.GetMetadataValue(tenant, "AzureStorageAccountKey", true);
      
            var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};");

            CloudQueueClient client = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = client.GetQueueReference(queueName);
            queue.CreateIfNotExists();
            
            var json = JsonConvert.SerializeObject(message, Formatting.None);
            

            CloudQueueMessage queueMessage = new CloudQueueMessage(json);
            queue.AddMessage(queueMessage);
        
        }

        public Task PublishAsync(string tenant, object message)
        {
            throw new NotImplementedException();
        }
    }
}
