using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using MVM.ProcessEngine.Plugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MVM.ProcessEngine.AzureFunctions
{
    public static class CancelProcess
    {
        [FunctionName("CancelProcess")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info($"ProcessEngine - CancelProcess function processed a request. {req.RequestUri}");

            // Header Parameters
            IEnumerable<string> tenantParameter = null;
            IEnumerable<string> idProcessParameter = null;

            if (req.Headers.TryGetValues("tenant", out tenantParameter) &&
                req.Headers.TryGetValues("idProcess", out idProcessParameter))
            {
                //var baseUrl = System.Environment.GetEnvironmentVariable("TenantMetadataUrl", EnvironmentVariableTarget.Process);
                var tenant = WebUtility.UrlDecode(tenantParameter.FirstOrDefault());
                var id = idProcessParameter.FirstOrDefault();

                // Url Tenant Metadata
                //var serviceUrl = string.Format(baseUrl, tenant);
                //serviceUrl += "&settingName=AzureStorageAccountConnString";

                // Get Acoount Storage from Metadata
                //WebClient client = new WebClient();
                //var content = client.DownloadString(serviceUrl);
                //var jsonContent = JsonConvert.DeserializeObject<dynamic>(content);
                //var tenantMetadata = (((JArray)jsonContent.settings)).ToDictionary<dynamic, string, string>(x => x.name, x => x.value);
                //var accountStorageConnection = string.Empty;
                //tenantMetadata?.TryGetValue("AzureStorageAccountConnString", out accountStorageConnection);

                //// Tenant Account Storage Connection 
                //var storageAccount = CloudStorageAccount.Parse(accountStorageConnection);

                //// Queue
                //CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                //CloudQueue queue = queueClient.GetQueueReference(id);
                //queue.CreateIfNotExists();

                // Message
                var message = new { IdProcesoGestor = id, Cancelado = true, Mensaje = "Proceso Cancelado..." };

                var publisher = new ServicesBusQueueMessagePublisher();

                await publisher.PublishAsync(tenant, message);
                //var jsonMessage = JsonConvert.SerializeObject(message, Formatting.None);

                //CloudQueueMessage queueMessage = new CloudQueueMessage(jsonMessage);
                //await Task.Run(() => queue.AddMessage(queueMessage));

                return req.CreateResponse(HttpStatusCode.OK);

            }

            return req.CreateResponse(HttpStatusCode.NotAcceptable, "Invalid Parameters");
        }
    }
}
