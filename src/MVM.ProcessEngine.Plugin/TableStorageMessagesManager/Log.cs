using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Plugin.TableStorageMessagesManager
{
    public class Log  : TableEntity
    {
        /// <summary>
        /// Procces/Transaction on Execution
        /// </summary>
        public string Process { get; set; }

        /// <summary>
        /// Activity of Procces/Transaction on Execution
        /// </summary>
        public string Activity { get; set; }

        /// <summary>
        /// Type of Message
        /// </summary>
        public string MessageType { get; set; }

        /// <summary>
        /// Message / Exception
        /// </summary>
        public string Message { get; set; }

    }
}
