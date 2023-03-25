using Microsoft.WindowsAzure.Storage;
using MVM.ProcessEngine.Common.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Xsl;

namespace MVM.ProcessEngine.TestConsole
{
    public class TestAzureFunctions
    {
        public string DownloadandParseXMLBlob()
        {
            string messRet = "";
            string client = "bidenergy";
            string blobName = "01_ResistenciaRealOHM.xml";
            var accountName = "mvmcomercial";
            var accountKey = "J/b/Km+f08YNi7XybwH/doZCCWNJ8HH6WDMyR3GfUo0AJrKt0WjFksYF0dGTgM/RDvnJGdAMaCfMd5zu7onohg==";

            ////var accountName = "datada01";
            ////var accountKey = "FzcPU9dJuoIOL/x22siqCOk1+kXblfNbxEIH56vM6uB4sSMDm3InMWO1xxZipMlAEZbKxOZXsQ0me5P0dErx7A==";
            var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};");
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(client);
            var blob = container.GetBlockBlobReference(blobName);

            string xml = blob.DownloadText();
            var sb = new StringBuilder();

            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                using (var streamReader = new StreamReader(memoryStream))
                {
                    // var x = XDocument.Load(streamReader);

                    var settings = new XmlReaderSettings();
                    settings.ValidationType = ValidationType.Schema;
                    settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                    settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
                    settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

                    Assembly myAssembly = Assembly.GetExecutingAssembly();
                    using (Stream schemaStream = myAssembly.GetManifestResourceStream("MVM.ProcessEngine.TestConsole.Template.xsd"))
                    {
                        XmlSchema schema = XmlSchema.Read(schemaStream, null);
                        settings.Schemas.Add(schema);
                    }

                    settings.ValidationEventHandler += delegate (object sender, ValidationEventArgs args)
                    {

                        if (args.Severity == XmlSeverityType.Warning)
                        {
                            messRet +=  args.Message;
                        }
                        else
                        {
                            messRet += args.Message;
                        }
                    };

                    using (var reader = XmlReader.Create(streamReader, settings))
                    {
                        // Parse the file and get xml content into the string builder. 
                        while (reader.Read())
                        {
                            sb.Append(reader.ReadOuterXml());
                        }
                    }
                }
            }

            //XDocument doc = XDocument.Parse(streamReader);

            return "ok:" + sb.ToString();

        }





    }
}
