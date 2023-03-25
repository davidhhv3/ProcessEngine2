#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"

using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Collections.Generic;
using System.Collections.Specialized;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{

    log.Info($"C# HTTP trigger function processed a request. {req.RequestUri}");

    // Header Parameters
    IEnumerable<string> tenantParameter = null;
    IEnumerable<string> idProcessParameter = null;

    if (req.Headers.TryGetValues("tenant", out tenantParameter) &&
        req.Headers.TryGetValues("idProcess", out idProcessParameter))
    {
        var baseUrl = System.Environment.GetEnvironmentVariable("TenantMetadataUrl", EnvironmentVariableTarget.Process);
        var tenant = WebUtility.UrlDecode(tenantParameter.FirstOrDefault());
        var id = idProcessParameter.FirstOrDefault();

        // Url Tenant Metadata
        var serviceUrl = string.Format(baseUrl, tenant);
        serviceUrl += "&settingName=AzureStorageAccountConnString";

        // Get Acoount Storage from Metadata
        WebClient client = new WebClient();
        var content = client.DownloadString(serviceUrl);
        var jsonContent = JsonConvert.DeserializeObject<dynamic>(content);
        var tenantMetadata = (((JArray)jsonContent.settings)).ToDictionary<dynamic, string, string>(x => x.name, x => x.value);
        var accountStorageConnection = string.Empty;
        tenantMetadata?.TryGetValue("AzureStorageAccountConnString", out accountStorageConnection);

        // Tenant Account Storage Connection 
        var storageAccount = CloudStorageAccount.Parse(accountStorageConnection);

        // Queue
        CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
        CloudQueue queue = queueClient.GetQueueReference(id);
        queue.CreateIfNotExists();

        // Message
        var message = new { IdProcesoGestor = id, Cancelado = true, Mensaje = "Proceso Cancelado..." };
        var jsonMessage = JsonConvert.SerializeObject(message, Formatting.None);

        CloudQueueMessage queueMessage = new CloudQueueMessage(jsonMessage);
        queue.AddMessage(queueMessage);

        return req.CreateResponse(HttpStatusCode.OK);

    }

    return req.CreateResponse(HttpStatusCode.NotAcceptable, "Invalid Parameters");
}