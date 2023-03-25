using MVM.ProcessEngine.Extension.SIOIndicator.Constants;
using MVM.ProcessEngine.Extension.SIOIndicator.Domain;
using MVM.ProcessEngine.Extension.SIOIndicator.Domain.Enumerations;
using MVM.ProcessEngine.Extension.SIOIndicator.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Extension.Service
{
     public class IndicatorCRPCC : Indicator
     {
        private CRPCCRepository _sqlRepository;

        public IndicatorCRPCC(string tenant, List<object> parametersProcess) : base(tenant, parametersProcess)
        {
            _sqlRepository = new CRPCCRepository(tenant);
        }
        public void SetElements()
        {
            var elements = _sqlRepository.GetElements(EndDate, ActiveCodes);
            _sqlRepository.DeleteData(Tables.ElementsTableName, $"WHERE IndicatorType = '{IndicatorConstants.RACC}'");
            _sqlRepository.ElementsInsert(elements, Tables.ElementsTableName);
        }
        public void SetActions()
        {
            var actions = _sqlRepository.GetActions(EndDate, ActiveCodes);
            _sqlRepository.DeleteData(Tables.ActionsTableName, $"WHERE ActionType = '{CalculationConstants.CRPCC}'");
            _sqlRepository.ActionsInsert(actions, Tables.ActionsTableName);
        }

        public void SetTopologies()
        {
            var topologies = _sqlRepository.GetTopologies(EndDate, ActiveCodes);
            _sqlRepository.DeleteData(Tables.TopologiesTableName, $"WHERE 1 = 1 ");
            _sqlRepository.TopologiesInsert(topologies, Tables.TopologiesTableName);
        }

    }
}
