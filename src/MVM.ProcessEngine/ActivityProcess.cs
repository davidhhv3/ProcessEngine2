using MVM.ProcessEngine.Interfaces;
using MVM.ProcessEngine.Common.Helpers;
using MVM.ProcessEngine.Common;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using MVM.ProcessEngine.TO;
using System.Data;
using System.Threading.Tasks;

namespace MVM.ProcessEngine
{
    public class ActivityProcess
    {

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="accountStorageConnection"></param>
        public ActivityProcess(string tenant,string accountStorageConnection)
        {
            // Set Metadata
            AzureStorageHelper.SetAppSettingForTenant(tenant, accountStorageConnection);

        }


        /// <summary>
        /// Run new Activity ()
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="accountStorageConnection"></param>
        /// <param name="xmlFileName"></param>
        /// <param name="parametters"></param>
        /// <returns></returns>
        public string Run(
            string tenant,
            string xmlFileName,
            params object[] parameters)
        {

            //// Call 'EjecutarCalculo'
            return GestorCalculosServiceLocator.GetService<ICalculoService>("CalculoTarget").EjecutarCalculo
                (tenant,
                xmlFileName,
                null,  // All Groups 
                null,  // All Calc 
                true,  // Async
                false, // No Paralell Calc
                false, // No stop on exception
                parameters);

        }

        
        /// <summary>
        /// Get Data (datatable) from Repository 
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="accountStorageConnection"></param>
        /// <param name="repository"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public string GetDataRepository(
           string tenant,
           RepositorioTO repository,
           ConfiguracionTO configuration)
        {

            DataTable dataTable = GestorCalculosServiceLocator.GetService<ICalculoService>("CalculoTarget").ObtenerDataRepositorio(
                tenant,
                repository,
                configuration,
                null // no need buffer
                )
                ;

            return JsonConvert.SerializeObject(dataTable);
        }

        public async Task<string> GetDataRepositoryLink(string tenant,
            string activityName,
           RepositorioTO repository,
           ConfiguracionTO configuration)
        {
            var link = await GestorCalculosServiceLocator.GetService<ICalculoService>("CalculoTarget").GuardarDataRepositorio(
                tenant,
                activityName,
                repository,
                configuration,
                null // no need buffer
                );

            return link;
        }

    }

}
