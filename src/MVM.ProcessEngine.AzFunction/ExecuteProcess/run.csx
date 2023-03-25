#r "Microsoft.WindowsAzure.Storage"
#r "..\Shared\bin\MVM.ProcessEngine.dll"
#r "Newtonsoft.Json"

using System.Text;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{

    log.Info($"C# HTTP trigger function processed a request. {req.RequestUri}");

    // Header Parameters
    IEnumerable<string> tenant = null;
    IEnumerable<string> xmlFileName = null;

    if (req.Headers.TryGetValues("tenant", out tenant) &&
        req.Headers.TryGetValues("xmlFileName", out xmlFileName))
    {
        // Get Parameters of body message
        var sTenant = WebUtility.UrlDecode(tenant.FirstOrDefault());
        var requestParams = req.Content.ReadAsStringAsync().Result;
        var jsonParams = JsonConvert.DeserializeObject<IDictionary<string, object>>(requestParams);
        var parameters = ((JArray)jsonParams.FirstOrDefault(x => x.Key == "parameters").Value).ToObject<List<string>>();

        // Account Storage Connection for AppSetting from Tenant
        string accountStorageConnection = System.Environment.GetEnvironmentVariable("AccountStorageConnection", EnvironmentVariableTarget.Process);

        // Add .xml extension
        string sXmlFileName = WebUtility.UrlDecode(xmlFileName.FirstOrDefault());
        sXmlFileName = sXmlFileName.ToUpper().EndsWith(".XML") ? sXmlFileName : string.Format("{0}.xml", sXmlFileName);

        // Call Dll Method
        var idCalculation = new MVM.ProcessEngine.ActivityProcess(sTenant, accountStorageConnection)
                                            .Run(sTenant,sXmlFileName, parameters.ToArray());

        // Response
        HttpResponseMessage response = req.CreateResponse(HttpStatusCode.OK);
        response.Content = new StringContent(idCalculation, Encoding.UTF8, "text/plain");
        return response;
    }
    return req.CreateResponse(HttpStatusCode.NotAcceptable, "Invalid Parameters");
}