using MVM.ProcessEngine.Extension.SIOIndicator.Domain.Enumerations;
using MVM.ProcessEngine.Extension.SIOIndicator.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Extension.Service
{
    class IndicatorCneZone:Indicator
    {
        private CneZonesRepository _sqlRepository;

        public IndicatorCneZone(string tenant, List<object> parametersProcess) : base(tenant, parametersProcess)
        {
            _sqlRepository = new CneZonesRepository(tenant);
        }

        public void GetCneZones()
        {
            var cneZones = _sqlRepository.GetCneZones(EndDate, ActiveCodes);
            _sqlRepository.DeleteData(Tables.CneZonesTableName);
            _sqlRepository.CneZonesInsert(cneZones, Tables.CneZonesTableName);
        }     

    }
}
