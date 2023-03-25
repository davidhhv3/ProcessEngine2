using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Extension.EnergySuite.Domain
{
    public class TimeSlot
    {
        public DateTime Date { get; set; }
        public string Region { get; set; }
        public int Hour { get; set; }
        public LoadType? SlotType { get; set; }
    }
}
