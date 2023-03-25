using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Extension.SIOIndicator.Domain
{
   public class Affectation
    {
        public Guid Id { get; set; }
        public string ConsignmentId { get; set; }
        public string IncomeType { get; set; }
        public string Origin { get; set; }
        public string AffectationTypeId { get; set; }
        public string MainElementId { get; set; }
        public string MainElementName { get; set; }
        public string AffectationElementId { get; set; }
        public string AffectationElementName { get; set; }
        public DateTime? ProgrammedStartedDate { get; set; }
        public DateTime? ProgrammedFinishedDate { get; set; }
        public string FinalConsignmentStatus { get; set; }
        
    }
}
