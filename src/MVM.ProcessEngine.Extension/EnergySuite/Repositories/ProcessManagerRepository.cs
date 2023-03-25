using MVM.ProcessEngine.Common;
using MVM.ProcessEngine.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Extension.EnergySuite.Repositories
{
    public class ProcessManagerRepository
    {
       
        /// <summary>
        /// Get Id Variables / Concepts
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,string> GetConcepts(string tenant)
        {

            string ProcessManagerApiBaseUrl = GestorCalculosHelper.GetMetadataValue(tenant,"ProcessManagerApiBaseUrl", true);
            string ProcessManagerApiGetConceptsMethod = GestorCalculosHelper.GetMetadataValue(tenant,"ProcessManagerApiGetConceptsMethod", true);

            WebClient client = new WebClient();
            var url = ProcessManagerApiBaseUrl + ProcessManagerApiGetConceptsMethod;
            client.Headers.Set("tenant",tenant);
            var content = client.DownloadString(url);
            var jsonContent = JsonConvert.DeserializeObject(content);
            var data = ((JArray)jsonContent).ToDictionary<dynamic, string, string>(x => x.Id, x => x.ConceptId);

            return data;

        }
            
    }
}
