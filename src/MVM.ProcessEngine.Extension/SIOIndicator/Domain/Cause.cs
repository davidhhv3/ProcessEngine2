using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Extension.SIOIndicator.Domain
{
    public class Cause
    {
        public Guid Id { get; set; }
        public string CauseName { get; set; }
        public bool IsExcluded { get; set; }
        public string System { get; set; }

    }
}
