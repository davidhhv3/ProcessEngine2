#r "Microsoft.WindowsAzure.Storage"
using System.Net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

    var queryParamms = req.GetQueryNameValuePairs()
       .ToDictionary(p => p.Key, p => p.Value, StringComparer.OrdinalIgnoreCase);

    string client;
    string blobName;

    if (queryParamms.TryGetValue("client", out client) && queryParamms.TryGetValue("blobName", out blobName))
    {

        var accountName = "mvmcomercial";
        var accountKey = "J/b/Km+f08YNi7XybwH/doZCCWNJ8HH6WDMyR3GfUo0AJrKt0WjFksYF0dGTgM/RDvnJGdAMaCfMd5zu7onohg==";
        
        //var accountName = "datada01";
        //var accountKey = "FzcPU9dJuoIOL/x22siqCOk1+kXblfNbxEIH56vM6uB4sSMDm3InMWO1xxZipMlAEZbKxOZXsQ0me5P0dErx7A==";
        var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};");
        var blobClient = storageAccount.CreateCloudBlobClient();
        var container = blobClient.GetContainerReference(client);
        var blob = container.GetBlockBlobReference(blobName);

        if (blob.Exists())
        {
            //var blobByteLength = blob.Properties.Length;
            //byte[] blobBytes = new byte[blobByteLength];
            //await blob.DownloadToByteArrayAsync(blobBytes, 0);
            //var xmlContent = System.Text.Encoding.Default.GetString(blobBytes);

            //xmlContent = xmlContent.Replace(@"\", "");

            //var xmlContent = blob.DownloadText();
            //var xmlContent = "";

            //using (var memoryStream = new MemoryStream())
            //{
            //    blob.DownloadToStream(memoryStream);
            //    xmlContent = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
            //}

            var xmlContent = blob.DownloadText();

            return req.CreateResponse(HttpStatusCode.OK, xmlContent);
            
        }
        else
        {
            return req.CreateResponse(HttpStatusCode.InternalServerError, "Blob not being able to be read");
        }
    }
    else
    {
        return req.CreateResponse(HttpStatusCode.NotAcceptable, "Invalid Parameters");
    }
}