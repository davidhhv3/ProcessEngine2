#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"

using System.Text;
using System.Net;
using System.Net.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    var queryParamms = req.GetQueryNameValuePairs()
        .ToDictionary(p => p.Key, p => p.Value, StringComparer.OrdinalIgnoreCase);

    log.Info($"C# HTTP trigger function processed a request. {req.RequestUri}");

    string tenant;
    bool ok = false;
    var message = string.Empty;
    HttpResponseMessage response = null;
    if (queryParamms.TryGetValue("tenant", out tenant))
    {
        var accountName = System.Environment.GetEnvironmentVariable("AccountName", EnvironmentVariableTarget.Process);
        var accountKey = System.Environment.GetEnvironmentVariable("PAK", EnvironmentVariableTarget.Process);
        var tableName = System.Environment.GetEnvironmentVariable("TableName", EnvironmentVariableTarget.Process);
        var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};");
        var tableClient = storageAccount.CreateCloudTableClient();
        var table = tableClient.GetTableReference(tableName);

        var customerFilter = TableQuery.GenerateFilterCondition(
            "PartitionKey",
            QueryComparisons.Equal,
            tenant);

        var combinedFilter = customerFilter;

        string settingName;
        if (queryParamms.TryGetValue("settingName", out settingName))
        {
            var appSettingFilter = TableQuery.GenerateFilterCondition(
                "RowKey",
                QueryComparisons.Equal,
                settingName);

            combinedFilter = TableQuery.CombineFilters(
                customerFilter,
                TableOperators.And,
                appSettingFilter
                );
        }

        var query = new TableQuery().Where(combinedFilter);
        try
        {
            var virtualResults = table.ExecuteQuery(query);
            var settings =
            (from setting in virtualResults
             select new
             {
                 name = setting.RowKey,
                 value = setting["value"].StringValue
             });
            response = req.CreateResponse(HttpStatusCode.OK);
            message = JsonConvert.SerializeObject(new
            {
                settings = settings
            });
        }
        catch
        {
            response = req.CreateResponse(HttpStatusCode.InternalServerError);
            message = JsonConvert.SerializeObject(new
            {
                error = "AppSetting couldn't be read."
            });
        }
    }
    else
    {
        response = req.CreateResponse(HttpStatusCode.NotAcceptable);
        message = JsonConvert.SerializeObject(new
        {
            error = "Invalid Parameters"
        });
    }
    response.Content = new StringContent(message, Encoding.UTF8, "application/json");
    return response;
}