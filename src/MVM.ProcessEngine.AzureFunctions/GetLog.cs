using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MVM.ProcessEngine.AzureFunctions
{
    public static class GetLog
    {
        [FunctionName("GetLog")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {

            log.Info($"ProcessEngine - GetLog function processed a request. {req.RequestUri}");

            var queryParamms = req.GetQueryNameValuePairs()
              .ToDictionary(p => p.Key, p => p.Value, StringComparer.OrdinalIgnoreCase);

            // Valide Header Parameters
            if (req.Headers.TryGetValues("tenant", out IEnumerable<string> tenantParameter) &&
                req.Headers.TryGetValues("idProcess", out IEnumerable<string> idProcessParameter) &&
                req.Headers.TryGetValues("dayProcess", out IEnumerable<string> dayProcessParameter)
                )
            {

                var baseUrl = System.Environment.GetEnvironmentVariable("TenantMetadataUrl", EnvironmentVariableTarget.Process);
                var tenant = tenantParameter.FirstOrDefault();
                var idProcess = idProcessParameter.FirstOrDefault();
                var dayProcess = dayProcessParameter.FirstOrDefault();

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

                // Get Azure Storage Table
                var tableClient = storageAccount.CreateCloudTableClient();
                var table = tableClient.GetTableReference("ALogProcessEngine" + dayProcess);

                // Get All Log 
                var logFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, idProcess);
                var logQuery = new TableQuery().Where(logFilter);

                try
                {
                    var logResults = table.ExecuteQuery(logQuery);
                    logResults = logResults.OrderBy(p => p.Timestamp);
                    var message = string.Join(Environment.NewLine,
                        (from result in logResults select result["Message"].StringValue));

                    var response = req.CreateResponse(HttpStatusCode.OK);
                    response.Content = new StringContent(message, Encoding.UTF8, "text/plain");
                    return response;

                }
                catch
                {
                    return req.CreateResponse(HttpStatusCode.InternalServerError, "Log couldn't be read.");
                }

            }

            return req.CreateResponse(HttpStatusCode.NotAcceptable, "Invalid Parameters");
        }
    }
}
