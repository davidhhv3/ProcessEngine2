#r "MVM.ProcessEngine.dll"

using System.Net;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    var queryParamms = req.GetQueryNameValuePairs()
       .ToDictionary(p => p.Key, p => p.Value, StringComparer.OrdinalIgnoreCase);

    log.Info($"C# HTTP trigger function processed a request. {req.RequestUri}");

    string customerReference;

    if (queryParamms.TryGetValue("customerReference", out customerReference))
    {

        var calculator = new MVM.ProcessEngine.CalculatorManager();

        var idCalculation = calculator.RunCalculation
                   (customerReference,@"01_ResistenciaRealOHM.xml", null, null, true, false, false,
                                                         new object[] {
                                                          new DateTime(2016, 04, 1),                //P0
                                                          1,                                        //P1
                                                          1,                                        //P2
                                                          "TranEcon",                               //P3
                                                          1,                                        //P4
                                                          1,                                        //P5
                                                          "RESREOHM",                               //P6
                                                          "",                                       //P7
                                                          new DateTime(2016, 04, 1),               //P8
                                                          19,                                       //P9
                                                          22,                                       //P10
                                                         });

        return req.CreateResponse(HttpStatusCode.OK, "Id Calculation: " + idCalculation);
    }
    return req.CreateResponse(HttpStatusCode.NotAcceptable, "Invalid Parameters");
}