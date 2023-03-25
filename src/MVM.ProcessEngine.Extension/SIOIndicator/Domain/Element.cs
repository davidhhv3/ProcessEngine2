using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Extension.SIOIndicator.Domain
{
    public class Element
    {
        public string SubsystemId { get; set; }
        public string AggregatorId { get; set; }
        public string ElementId { get; set; }
        public string ElementType { get; set; }
        public string UCtype { get; set; }
        public string Compensate { get; set; }
        public double? MHAI { get; set; }
        public string MHAIAUnit { get; set; }
        public double? ManeuverTime { get; set; }
        public string ManeuverUnit { get; set; }
        public double? MaintenanceTime { get; set; }
        public string MaintenanceUnit { get; set; }
        public DateTime? MaintenanceStartDate { get; set; }
        public DateTime? MaintenanceEndDate { get; set; }
        public bool? IsCenterCut { get; set; }
        public double? SegmentLength { get; set; }
        public string SubsystemName { get; set; }
        public string AggregatorName { get; set; }
        public string ElementName { get; set; }
        public string IndicatorType { get; set; }
        public string OperatorCompanyId { get; set; }
        public string OperatorCompanyName { get; set; }
        public string OperatorCompanyShortName { get; set; }
        public string DefaultCapacity { get; set; }

    }
}
