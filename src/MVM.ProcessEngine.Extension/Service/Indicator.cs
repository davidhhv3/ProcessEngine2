using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Extension.Service
{
    public class Indicator
    {
        public string Tenant;

        public DateTime StartDate;
        public DateTime EndDate;
        public string IndicatorType;
        public List<string> ActiveCodes = new List<string>();

        public Indicator(string tenant, List<object> parametersProcess)
        {
            Tenant = tenant;
            SetParameters(parametersProcess);
        }
        private void SetParameters(List<object> parametersProcess)
        {
            if (parametersProcess!=null)
            {
                try
                {
                    if (parametersProcess.Count >= 1)
                    {
                        var listTmp = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(parametersProcess[0].ToString());
                        if (!listTmp.Where(a => a.Contains("%")).Any())
                            ActiveCodes = listTmp;
                    }
                }
                catch (Exception ex)
                {

                    throw new Exception("Error En repMHAIASTR  parametersProcess?[0]" + parametersProcess?[0] + ex.Message);
                }

                try
                {
                    if (parametersProcess.Count >= 3 && !string.IsNullOrEmpty(parametersProcess[2]?.ToString()))
                        DateTime.TryParse(parametersProcess[2].ToString(), out EndDate);

                }
                catch (Exception ex)
                {

                    throw new Exception("Error En repMHAIASTR  parametersProcess?[2]" + parametersProcess?[2] + ex.Message);
                }
                try
                {
                    if (parametersProcess[4] != null)
                    {
                        if (parametersProcess.Count >= 5 && !string.IsNullOrEmpty(parametersProcess[4]?.ToString()))
                            DateTime.TryParse(parametersProcess[4].ToString(), out StartDate);
                    }
                }
                catch (Exception ex)
                {

                    throw new Exception("Error En repMHAIASTR  parametersProcess?[4]" + parametersProcess?[4] + ex.Message);
                }
            }
        }
    }
}
