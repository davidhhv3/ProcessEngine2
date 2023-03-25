using System;

namespace MVM.ProcessEngine.Extension.SIOIndicator.Domain
{
    public class Suspension
    {
        public Guid Id { get; set; }
        public string ConsignmentId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CauseValue { get; set; }
        
    }
}
