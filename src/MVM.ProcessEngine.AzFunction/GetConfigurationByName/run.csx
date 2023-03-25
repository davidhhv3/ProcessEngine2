#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"

using System.Text;
using System.Net;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
  
    log.Info($"C# HTTP trigger function processed a request. {req.RequestUri}");

    // Header Parameters
    IEnumerable<string> tenantParameter = null;
    IEnumerable<string> nameParameter = null;

    if (req.Headers.TryGetValues("tenant", out tenantParameter) &&
        req.Headers.TryGetValues("name", out nameParameter)
        )
    {

        var baseUrl = System.Environment.GetEnvironmentVariable("TenantMetadataUrl", EnvironmentVariableTarget.Process);
        var tenant = WebUtility.UrlDecode(tenantParameter.FirstOrDefault());
        var nameFile = WebUtility.UrlDecode(nameParameter.FirstOrDefault());

        // Url Tenant Metadata
        var serviceUrl = string.Format(baseUrl, tenant);
        serviceUrl += "&settingName=AzureStorageAccountConnString";

        // Get Account Storage from Metadata
        WebClient client = new WebClient();
        var content = client.DownloadString(serviceUrl);
        var jsonContent = JsonConvert.DeserializeObject<dynamic>(content);
        var tenantMetadata = (((JArray)jsonContent.settings)).ToDictionary<dynamic, string, string>(x => x.name, x => x.value);
        var accountStorageConnection = string.Empty;
        tenantMetadata?.TryGetValue("AzureStorageAccountConnString", out accountStorageConnection);
        var storageAccount = CloudStorageAccount.Parse(accountStorageConnection);

        var blobClient = storageAccount.CreateCloudBlobClient();
        var container = blobClient.GetContainerReference("xml");
        var blob = container.GetBlockBlobReference(nameFile);

        if (blob.Exists())
        {
            string text;
            var s = blob.DownloadText();
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(s)))
            using (var streamReader = new StreamReader(memoryStream))
            {
                text = streamReader.ReadToEnd();
            }
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(text, Encoding.UTF8, "text/xml");
            return response;

        }
        return req.CreateResponse(HttpStatusCode.NoContent, "File no found");
    }
    return req.CreateResponse(HttpStatusCode.NotAcceptable, "Invalid Parameters");
}