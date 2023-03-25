using MVM.ProcessEngine.Extension.SIOIndicator.Domain.Enumerations;
using MVM.ProcessEngine.Extension.SIOIndicator.Repositories;
using System.Collections.Generic;

namespace MVM.ProcessEngine.Extension.Service
{
    class IndicatorSuspensions : Indicator
    {
        private SuspensionsRepository _sqlRepository;

        public IndicatorSuspensions(string tenant, List<object> parametersProcess) : base(tenant, parametersProcess)
        {
            _sqlRepository = new SuspensionsRepository(tenant);
        }

        public void GetSuspensions()
        {
            var suspensions = _sqlRepository.GetSuspensions(EndDate);
            _sqlRepository.DeleteData(Tables.SuspensionsTableName);
            _sqlRepository.SuspensionsInsert(suspensions, Tables.SuspensionsTableName);
        }

    }
}
