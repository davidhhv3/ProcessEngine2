#r "Newtonsoft.Json"
#r "System.Data"

using System.Text;
using System.Data.SqlClient;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

    // Header Parameters
    IEnumerable<string> tenantParameter = null;

    if (req.Headers.TryGetValues("tenant", out tenantParameter))
    {
        var successful = true;
        var results = new Dictionary<string, string>();

        try
        {
            var baseUrl = System.Environment.GetEnvironmentVariable("TenantMetadataUrl", EnvironmentVariableTarget.Process);
            var sqlQuery = System.Environment.GetEnvironmentVariable("Query", EnvironmentVariableTarget.Process);
            var tenant = WebUtility.UrlDecode(tenantParameter.FirstOrDefault());

            // Url Tenant Metadata
            var serviceUrl = string.Format(baseUrl, tenant);
            serviceUrl += "&settingName=DBConnectionString";

            // Get Account Storage from Metadata
            WebClient client = new WebClient();
            var content = client.DownloadString(serviceUrl);
            var jsonContent = JsonConvert.DeserializeObject<dynamic>(content);
            var tenantMetadata = (((JArray)jsonContent.settings)).ToDictionary<dynamic, string, string>(x => x.name, x => x.value);
            var cnnString = string.Empty;
            tenantMetadata?.TryGetValue("DBConnectionString", out cnnString);
            
            // Go to DB
            using (var connection = new SqlConnection(cnnString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(reader.GetString(0), reader.GetString(1));
                    }
                }
            }
        }
        catch
        {
            successful = false;
        }

        // Response
        if (successful)
        {
            var data = JsonConvert.SerializeObject(results);
            HttpResponseMessage response = req.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(data, Encoding.UTF8, "application/json");
            return response;
        }
        else
        {
            req.CreateResponse(HttpStatusCode.BadRequest, "Unable to process your request!");
        }

    }

    return req.CreateResponse(HttpStatusCode.NotAcceptable, "Invalid Parameters");

}