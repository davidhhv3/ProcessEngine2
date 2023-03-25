using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Extension.SIOIndicator.Domain
{
    public class Consignment
    {
        public string Consecutive { get; set; }
        public string ElementId { get; set; }
        public string MaintenanceOriginName { get; set; }
        public string CauseName { get; set; }
        public DateTime? ScheduledStartDate { get; set; }
        public DateTime? ScheduledEndDate { get; set; }
        public DateTime? RealStartDate { get; set; }
        public DateTime? RealEndDate { get; set; }
        public DateTime? PenalDate { get; set; }
        public bool? ActivePenal { get; set; }
        public string IndicatorType { get; set; }
        public string ElementCompanyShortName { get; set; }
        public string EntryType { get; set; }
        public string AffectationTypeId { get; set; }
        public string CauseRealStartDateChange { get; set; }
        public string CauseRealEndDateChange { get; set; }


    }
}
