using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Extension.SIOIndicator.Domain
{
    public class Action
    {
        public Guid Id { get; set; }
        public DateTime? InstructionTime { get; set; }
        public DateTime? OccurrenceTime { get; set; }
        public DateTime? ConfirmationTime { get; set; }
        public string ElementId { get; set; }
        public string ActionName { get; set; }
        public string Type { get; set; }
        public string ConsignmentId { get; set; }
        public decimal? NewAvailability { get; set; }
        public string Movement { get; set; }
        public string TypeMovement { get; set; }
        public DateTime? EndOccurrenceTime { get; set; }
        public DateTime? ScheduledStartDate { get; set; }
        public DateTime? ScheduledEndDate { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public string ActionType { get; set; }
        public string ElementCausingId { get; set; }
        public bool? CneZone { get; set; }
        public string Fuel { get; set; }
        public string CauseOrigin { get; set; }
        public string FuelCEN { get; set; }
        public string PlantCEN { get; set; }
        public string ElementCompanyShortName { get; set; }
        
    }
}
