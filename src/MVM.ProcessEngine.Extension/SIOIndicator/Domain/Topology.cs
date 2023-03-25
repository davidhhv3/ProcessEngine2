using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Extension.SIOIndicator.Domain
{
    public class Topology
    {
        public Guid TopologyId { get; set; }
        public string TopologyName { get; set; }
        public string ElementId { get; set; }
        public decimal CPCC { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Value { get; set; }

    }
}
