using MVM.ProcessEngine.Extension.SIOIndicator.Constants;
using MVM.ProcessEngine.Extension.SIOIndicator.Domain.Enumerations;
using MVM.ProcessEngine.Extension.SIOIndicator.Repositories;
using System.Collections.Generic;

namespace MVM.ProcessEngine.Extension.Service
{
    public class IndicatorDispReal: Indicator
    {
        private DispRealRepository _sqlRepository;

        public IndicatorDispReal(string tenant, List<object> parametersProcess) : base(tenant, parametersProcess)
        {
            _sqlRepository = new DispRealRepository(tenant);
        }

        public void SetUnits()
        {
            var units = _sqlRepository.GetUnits(EndDate, ActiveCodes);
            _sqlRepository.DeleteData(Tables.UnitsTableName);
            _sqlRepository.UnitsInsert(units, Tables.UnitsTableName);
        }

        public void SetActions()
        {
            var actions = _sqlRepository.GetActions(StartDate, EndDate, ActiveCodes);
            _sqlRepository.DeleteData(Tables.ActionsTableName, $"WHERE ActionType = '{CalculationConstants.DispRealP}'");
            _sqlRepository.ActionsInsert(actions, Tables.ActionsTableName);
        }

        public void SetConsignments()
        {
            var consignments = _sqlRepository.GetConsignments(StartDate, EndDate, ActiveCodes);
            _sqlRepository.DeleteData(Tables.ConsignmentsTableName);
            _sqlRepository.ConsignmentsInsert(consignments, Tables.ConsignmentsTableName);
        }
    }
}
