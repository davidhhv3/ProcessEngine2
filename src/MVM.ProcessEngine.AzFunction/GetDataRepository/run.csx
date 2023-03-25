#r "Microsoft.WindowsAzure.Storage"
#r "..\Shared\bin\MVM.ProcessEngine.dll"
#r "..\Shared\bin\MVM.ProcessEngine.TO.dll"
#r "Newtonsoft.Json"

using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.Specialized;
using MVM.ProcessEngine.TO;

// GET DATA REPOSITORY
public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{

    log.Info($"C# HTTP trigger function processed a request. {req.RequestUri}");

    // Header Parameters
    IEnumerable<string> tenant = null;

    if (req.Headers.TryGetValues("tenant", out tenant))
    {
        // Get header Parameters
        var sTenant = WebUtility.UrlDecode(tenant.FirstOrDefault());

        // Get body message
        var requestBody = req.Content.ReadAsStringAsync().Result;

        // Account Storage Connection for AppSetting from Tenant
        string accountStorageConnection = System.Environment.GetEnvironmentVariable("AccountStorageConnection", EnvironmentVariableTarget.Process);

        // Deserialize body
        var jsonBody = JsonConvert.DeserializeObject<IDictionary<string, object>>(requestBody);
        RepositorioTO repository = JsonConvert.DeserializeObject<RepositorioTO>(jsonBody["repository"].ToString());
        ConfiguracionTO configuration = JsonConvert.DeserializeObject<ConfiguracionTO>(jsonBody["configuration"].ToString());

        // Call Dll Method
        var data = new MVM.ProcessEngine.ActivityProcess(sTenant, accountStorageConnection)
                                .GetDataRepository(sTenant,repository, configuration);

        // Response
        HttpResponseMessage response = req.CreateResponse(HttpStatusCode.OK);
        response.Content = new StringContent(data, Encoding.UTF8, "application/json");
        return response;

    }

    return req.CreateResponse(HttpStatusCode.NotAcceptable, "Invalid Parameters");
}