using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using MVM.ProcessEngine.Common;
using MVM.ProcessEngine.Common.Helpers;
using MVM.ProcessEngine.Core;
using MVM.ProcessEngine.Plugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.TestConsole
{
    public class Program
    {

        static public IConfigurationRoot Configuration { get; set; }



        static void Main(string[] args)
        {
            HttpRequestMessage req = new HttpRequestMessage();



            string accountStorageName = "mvmcomercial";
            string accountStorageKey = "J/b/Km+f08YNi7XybwH/doZCCWNJ8HH6WDMyR3GfUo0AJrKt0WjFksYF0dGTgM/RDvnJGdAMaCfMd5zu7onohg==";


            var processEngine = new MVM.ProcessEngine.ActivityProcess("BidEnergy",
                    accountStorageName);

            System.Console.WriteLine("-----------------------------------------------------------");
            System.Console.WriteLine("EVALUACIÓN DE CÁLCULOS");
            System.Console.WriteLine("------------------------------------------------------------");

            System.Console.ReadLine();

            // Test Azure Blob Storage
            //var testReadBlog = new TestAzureFunctions();
            //System.Console.WriteLine(testReadBlog.DownloadandParseXMLBlob());


            var cont = "Y";
            while (cont.ToUpper().Equals("Y"))
            {

                var result = processEngine.Run
                    ("BidEnergy",
                    "DeterminacionCostoMarginalBarraPotencia.xml",
                    new object[] {
                        new DateTime(2016, 04, 1),                //P0
                        1,                                        //P1
                        1,                                        //P2
                        "TranEcon",                               //P3
                        1,                                        //P4
                        1,                                        //P5
                        "RESREOHM",                               //P6
                        //"",                                       //P7
                        //new DateTime(2016, 04, 1),                //P8
                        //19,                                       //P9
                        //22,                                       //P10
                    });


                System.Console.WriteLine("FIN PROCESO:" + result);
                System.Console.WriteLine("Ejecutar de nuevo (Y)?");
                cont = System.Console.ReadLine();
            }

            System.Console.ReadLine();

        }

        public void CancelProcess()
        {

            string tenant = "BidEnergy";
            string accountStorageConnection = "DefaultEndpointsProtocol=https;AccountName=mvmcomercial;AccountKey=J/b/Km+f08YNi7XybwH/doZCCWNJ8HH6WDMyR3GfUo0AJrKt0WjFksYF0dGTgM/RDvnJGdAMaCfMd5zu7onohg==";
            string id = "89812881-d2ee-4219-9b86-67bdbe96fd1a";
            {

                var storageAccount = CloudStorageAccount.Parse(accountStorageConnection);

                CloudQueueClient client = storageAccount.CreateCloudQueueClient();
                CloudQueue queue = client.GetQueueReference(id);
                queue.CreateIfNotExists();

                var mensaje = new { IdProcesoGestor = id, Cancelado = true, Mensaje = "Proceso Cancelado..." };
                var json = JsonConvert.SerializeObject(mensaje, Formatting.None);

                CloudQueueMessage queueMessage = new CloudQueueMessage(json);
                queue.AddMessage(queueMessage);
            }


        }

    }
}
