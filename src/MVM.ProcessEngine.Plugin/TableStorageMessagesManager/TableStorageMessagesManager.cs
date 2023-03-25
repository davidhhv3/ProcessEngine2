using MVM.ProcessEngine.Interfaces;
using System;
using MVM.ProcessEngine.Common.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace MVM.ProcessEngine.Plugin.TableStorageMessagesManager
{
    /// <summary>
    /// Implementación concreta de manejador de mensajes usando Azure Table Storage
    /// </summary>
    public class TableStorageMessagesManager : IMensajesManager
    {

        /// <summary>
        /// implement method from interface 
        /// <see cref="MVM.ProcessEngine.Interfaces.RegistrarProceso"/>
        /// </summary>
        public void RegistrarProceso(string tenant, string idLog ,string sistema, string nombre)
        {
            // Log
            var log = new Log()
            {
                PartitionKey = idLog,
                RowKey = Guid.NewGuid().ToString(),
                Process = sistema,
                Activity = nombre,
                Message = "Starting...",
            };

            InsertLog(tenant, log);
        }

        /// <summary>
        /// implement method from interface 
        /// <see cref="MVM.ProcessEngine.Interfaces.ActualizarEstadoProceso"/>
        /// </summary>
        public void ActualizarEstadoProceso(string tenant, string idProceso, string estado, string mensajeFinal, DateTime? fechaFinal)
        {
            var log = new Log()
            {
                PartitionKey = idProceso,
                RowKey = Guid.NewGuid().ToString(),
                Message = estado + ":" + mensajeFinal,
            };

          
            InsertLog(tenant , log);
        }

        /// <summary>
        /// implement method from interface 
        /// <see cref="MVM.ProcessEngine.Interfaces.ObtenerMensajesProceso"/>
        /// </summary>
        public string ObtenerMensajesProceso(string tenant, string idProceso)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// implement method from interface 
        /// <see cref="MVM.ProcessEngine.Interfaces.RegistrarMensaje"/>
        /// </summary>
        public void RegistrarMensaje(string tenant, string idProceso, string mensaje)
        {
            var log = new Log()
            {
                PartitionKey = idProceso,
                RowKey = Guid.NewGuid().ToString(),
                Message = mensaje,
            };

            InsertLog(tenant,log);
        }

        /// <summary>
        /// Insert Log on Azure Table Storage
        /// </summary>
        /// <param name="log"></param>
        private void InsertLog(string tenant, Log log)
        {
            var table = AzureStorageHelper.GetLogTable(tenant);

            // Create the InsertOrReplace  TableOperation
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(log);

            // Execute the operation.
            TableResult result = table.Execute(insertOrMergeOperation);

        }

    }
}
