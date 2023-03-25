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
using MVM.ProcessEngine.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MVM.ProcessEngine.AzureFunctions
{
    public static class ExecuteProcess
    {
        [FunctionName("ExecuteProcess")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info($"Processengine - ExecuteProcess function processed a request. {req.RequestUri}");

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
                var parameters = ((JArray)jsonParams.FirstOrDefault(x => x.Key == "parameters").Value).ToObject<List<object>>();

                // Account Storage Connection for AppSetting from Tenant
                string accountStorageConnection = System.Environment.GetEnvironmentVariable("AccountStorageConnection", EnvironmentVariableTarget.Process);

                // Add .xml extension
                string sXmlFileName = WebUtility.UrlDecode(GestorCalculosHelper.GetString(Convert.FromBase64String(xmlFileName.FirstOrDefault())));
                sXmlFileName = sXmlFileName.ToUpper().EndsWith(".XML") ? sXmlFileName : string.Format("{0}.xml", sXmlFileName);

                // Call Dll Method
                var activityProcess = new ActivityProcess(sTenant, accountStorageConnection);
                var idCalculation = await Task.Run(()=> activityProcess.Run(sTenant, sXmlFileName, parameters.ToArray()));

                // Response
                HttpResponseMessage response = req.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(idCalculation, Encoding.UTF8, "text/plain");
                return response;
            }
            return req.CreateResponse(HttpStatusCode.NotAcceptable, "Invalid Parameters");
        }

    }
}
