using System;
using System.Collections.Generic;


namespace MVM.ProcessEngine.Extension.EnergySuite.Domain
{

    public partial class ContractCondition
    {
        public int ContractId { get; set; }
        public string ProductType { get; set; }
        public string ElementId { get; set; }
        public System.DateTime Date { get; set; }
        public int Period { get; set; }
        public string ConditionType { get; set; }
        public System.Guid CalculationId { get; set; }
        public System.Guid ConditionId { get; set; }
    }
}
