using MVM.ProcessEngine.Extension.SIOIndicator.Domain.Enumerations;
using MVM.ProcessEngine.Extension.SIOIndicator.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Extension.Service
{
    class IndicatorCause:Indicator
    {
        private CausesRepository _sqlRepository;

        public IndicatorCause(string tenant, List<object> parametersProcess) : base(tenant, parametersProcess)
        {
            _sqlRepository = new CausesRepository(tenant);
        }

        public void GetCauses()
        {
            var causes = _sqlRepository.GetCauses();
            _sqlRepository.DeleteData(Tables.CausesTableName);
            _sqlRepository.CausesInsert(causes, Tables.CausesTableName);
        }
    }
}
