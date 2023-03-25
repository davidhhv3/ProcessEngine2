#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"
using System.Text;
using System.IO;
using System.Net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    var queryParamms = req.GetQueryNameValuePairs()
        .ToDictionary(p => p.Key, p => p.Value, StringComparer.OrdinalIgnoreCase);

    log.Info($"C# HTTP trigger function processed a request. {req.RequestUri}");

    // Header Parameters
    IEnumerable<string> tenantParameter = null;
    IEnumerable<string> idProcessParameter = null;
    IEnumerable<string> dayProcessParameter = null; //YYYYMMDD

    if (req.Headers.TryGetValues("tenant", out tenantParameter) &&
        req.Headers.TryGetValues("idProcess", out idProcessParameter) &&
        req.Headers.TryGetValues("dayProcess", out dayProcessParameter)
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

        // Get Id Log from Id Execution
        //var executionFilter = TableQuery.GenerateFilterCondition(
        //    "Execution", QueryComparisons.Equal, idProcess);

        //var executionQuery = new TableQuery().Where(executionFilter);
        //var executionRecord = table.ExecuteQuery(executionQuery);
        //var idLog = executionRecord.FirstOrDefault()?.PartitionKey;


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

        return req.CreateResponse(HttpStatusCode.OK);

    }
    return req.CreateResponse(HttpStatusCode.NotAcceptable, "Invalid Parameters");
}