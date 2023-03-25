using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Extension.SIOIndicator.Domain
{
   public class CneZone
    {
        public Guid Id { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ElementId { get; set; }
        public string ElementName { get; set; }
        public string ZoneName { get; set; }
        public string State { get; set; }
    }
}
