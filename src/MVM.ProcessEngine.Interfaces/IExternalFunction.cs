using MVM.ProcessEngine.TO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Interfaces
{
    public interface IExternalFunction
    {
        decimal Execute(string tenant, List<object> parametersProcess, List<object> buffer );
    }
}
