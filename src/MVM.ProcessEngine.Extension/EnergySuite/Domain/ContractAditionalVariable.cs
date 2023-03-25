using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Extension.EnergySuite.Domain
{
    public class ContractAditionalVariable
    {
        public int ContractId { get; set; }
        public DateTime Date { get; set; }
        public byte Period { get; set; }
        public byte Version { get; set; }
        public string ProductType { get; set; }
        public string ConceptId { get; set; }
        public string ElementId { get; set; }
        public decimal? Value { get; set; }
        public DateTime? DateValue { get; set; }
        public string TextValue { get; set; }

    }
}
