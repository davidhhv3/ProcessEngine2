using MVM.ProcessEngine.Interfaces;
using Spring.Context.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Common.Helpers
{
    public class ExternalFunctionFactory
    {
        IExternalFunction ExternalFunction;

        public ExternalFunctionFactory() {
            ExternalFunction = ContextRegistry.GetContext()["externalFunction"] as IExternalFunction;
        }

        public decimal Execute(string tenant, List<object> parametersProcess, List<object> buffer)
        {
            return ExternalFunction.Execute(tenant, parametersProcess, buffer);
        }
    }
}
