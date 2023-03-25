using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using MVM.ProcessEngine.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MVM.ProcessEngine.AzureFunctions
{
    public static class GetConfigurationByName
    {
        [FunctionName("GetConfigurationByName")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info($"ProcessEngine - GetConfigurationByName function processed a request. {req.RequestUri}");

            // Header Parameters
            IEnumerable<string> tenantParameter = null;
            IEnumerable<string> nameParameter = null;

            if (req.Headers.TryGetValues("tenant", out tenantParameter) &&
                req.Headers.TryGetValues("name", out nameParameter)
                )
            {

                var baseUrl = System.Environment.GetEnvironmentVariable("TenantMetadataUrl", EnvironmentVariableTarget.Process);
                var tenant = WebUtility.UrlDecode(tenantParameter.FirstOrDefault());
                //var nameFile = WebUtility.UrlDecode(GestorCalculosHelper.Base64Decode(nameParameter.FirstOrDefault()));
                var nameFile = WebUtility.UrlDecode(GestorCalculosHelper.GetString(Convert.FromBase64String(nameParameter.FirstOrDefault())));

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
                    var s = await Task.Run(() => blob.DownloadText());
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
    }
}
