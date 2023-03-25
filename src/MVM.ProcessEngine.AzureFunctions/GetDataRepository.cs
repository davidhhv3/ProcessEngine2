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
using MVM.ProcessEngine.TO;
using Newtonsoft.Json;

namespace MVM.ProcessEngine.AzureFunctions
{
    public static class GetDataRepository
    {
        [FunctionName("GetDataRepository")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info($"ProcessEngine - GetDataRepository function processed a request. {req.RequestUri}");

            // Header Parameters
            IEnumerable<string> tenant = null;
            IEnumerable<string> activityNameList = null;
            IEnumerable<string> saveBlobList = null;

            req.Headers.TryGetValues("saveBlob", out saveBlobList);
            req.Headers.TryGetValues("activityName", out activityNameList);

            if (req.Headers.TryGetValues("tenant", out tenant))
            {
                // Get header Parameters
                var sTenant = WebUtility.UrlDecode(tenant.FirstOrDefault());
                var saveBlob = WebUtility.UrlDecode(saveBlobList.FirstOrDefault());
                var activityName = WebUtility.UrlDecode(activityNameList.FirstOrDefault());

                // Get body message
                var requestBody = req.Content.ReadAsStringAsync().Result;

                // Account Storage Connection for AppSetting from Tenant
                string accountStorageConnection = System.Environment.GetEnvironmentVariable("AccountStorageConnection", EnvironmentVariableTarget.Process);

                // Deserialize body
                var jsonBody = JsonConvert.DeserializeObject<IDictionary<string, object>>(requestBody);
                RepositorioTO repository = JsonConvert.DeserializeObject<RepositorioTO>(jsonBody["repository"].ToString());
                ConfiguracionTO configuration = JsonConvert.DeserializeObject<ConfiguracionTO>(jsonBody["configuration"].ToString());

                // Call Dll Method
                var activityProcess = new ActivityProcess(sTenant, accountStorageConnection);
                var data = string.Empty;
                if(saveBlob == "True")
                {
                   data = await activityProcess.GetDataRepositoryLink(sTenant, activityName, repository, configuration);
                } else
                {
                    data = await Task.Run(() => activityProcess.GetDataRepository(sTenant, repository, configuration));
                }

                // Response
                HttpResponseMessage response = req.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(data, Encoding.UTF8, "application/json");
                return response;

            }

            return req.CreateResponse(HttpStatusCode.NotAcceptable, "Invalid Parameters");
        }
    }
}
