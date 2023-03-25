using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Extension.EnergySuite
{
    public class FormulaCondition
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<DayOfWeek> Days { get; set; }
        public List<int> Hours { get; set; }
        public List<LoadType> LoadTypes { get; set; }
        public string Formula { get; set; }
        public bool AllDays { get; set; }
        public bool NormalDay { get; set; }
        public bool Holiday { get; set; }
        public string Region { get; set; }
    }
}
