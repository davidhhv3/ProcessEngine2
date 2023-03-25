using System;

namespace MVM.ProcessEngine.Extension.SIOIndicator.Domain
{
    public class GenUnit
    {
        public Guid Id { get; set; }
        public string GenerationGroupId { get; set; }
        public string UnitId { get; set; }
        public string GroupType { get; set; }
        public string GenerationGroupName { get; set; }
        public string UnitName { get; set; }
        public string CEN_Unit { get; set; }
        public string Fuel_Unit { get; set; }
        public string CEN_Group { get; set; }
        public DateTime? OperationStartDate { get; set; }
    }
}
