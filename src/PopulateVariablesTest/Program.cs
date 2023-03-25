using MVM.ProcessEngine.Extension.EnergySuite;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PopulateVariablesTest
{
    class Program
    {
        static void Main(string[] args)
        {

            Stopwatch stopWatch = new Stopwatch();
            Console.WriteLine("Start...");
            stopWatch.Start();


            List<object> parametersProcess = new List<object>() { "2017-01-01", "2017-01-31" };
            List< object > buffer = new List<object>() { "1" };

            //Exc
            new ContractConditionsExternalFunction().Execute("BidEnergy",parametersProcess,buffer);

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds,ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
            Console.WriteLine("End.");
            Console.ReadKey();
        }
    }
}
