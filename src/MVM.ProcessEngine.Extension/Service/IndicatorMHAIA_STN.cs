using MVM.ProcessEngine.Extension.SIOIndicator.Constants;
using MVM.ProcessEngine.Extension.SIOIndicator.Domain.Enumerations;
using MVM.ProcessEngine.Extension.SIOIndicator.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Extension.Service
{
    public class IndicatorMHAIA_STN: Indicator
    {
        private MHAIA_STNRepository _sqlRepository;

        public IndicatorMHAIA_STN(string tenant, List<object> parametersProcess) : base(tenant, parametersProcess)
        {
            _sqlRepository = new MHAIA_STNRepository(tenant);
        }

        public async Task SetElements()
        {
            var elements = _sqlRepository.GetElements(EndDate, ActiveCodes);
            var batteries = await _sqlRepository.GetBatteries(EndDate, ActiveCodes);
            _sqlRepository.DeleteData(Tables.ElementsTableName, $"WHERE IndicatorType = '{IndicatorConstants.STN}'");
            _sqlRepository.ElementsInsert(elements, Tables.ElementsTableName);
            _sqlRepository.ElementsInsert(batteries, Tables.ElementsTableName);
        }

        public void SetConsignments()
        {
            StartDate = EndDate.AddSeconds(1).AddMonths(-1);

            var consignments = _sqlRepository.GetConsignments(StartDate, EndDate, ActiveCodes);
            _sqlRepository.DeleteData(Tables.ConsignmentsTableName, $"WHERE IndicatorType = '{IndicatorConstants.STN}'");
            _sqlRepository.ConsignmentsInsert(consignments, Tables.ConsignmentsTableName);
        }

        public void SetActions()
        {
            StartDate = EndDate.AddSeconds(1).AddMonths(-1);

            var actions = _sqlRepository.GetActions(StartDate, EndDate, ActiveCodes);
            _sqlRepository.DeleteData(Tables.ActionsTableName, $"WHERE ActionType = '{CalculationConstants.MHAIA_STN}'");
            _sqlRepository.ActionsInsert(actions, Tables.ActionsTableName);
        }
    }
}
