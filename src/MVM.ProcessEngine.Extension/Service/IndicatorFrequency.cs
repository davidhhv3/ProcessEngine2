using MVM.ProcessEngine.Extension.SIOIndicator.Constants;
using MVM.ProcessEngine.Extension.SIOIndicator.Domain.Enumerations;
using MVM.ProcessEngine.Extension.SIOIndicator.Repositories;
using System;
using System.Collections.Generic;

namespace MVM.ProcessEngine.Extension.Service
{
    public class IndicatorFrequency :Indicator
    {
        private FrequencyRepository _sqlRepository;

        public IndicatorFrequency(string tenant, List<object> parametersProcess) : base(tenant, parametersProcess)
        {
            _sqlRepository = new FrequencyRepository(tenant);
        }

        public void SetElementsSTR()
        {
            var elements =  _sqlRepository.GetElementsSTR(EndDate);
            _sqlRepository.DeleteData(Tables.ElementsTableName, $"WHERE IndicatorType = '{IndicatorConstants.FREC_STR}'");
            _sqlRepository.ElementsInsert(elements, Tables.ElementsTableName);
        }
        public void SetElementsSTN()
        {
            var elements = _sqlRepository.GetElementsSTN(EndDate);
            _sqlRepository.DeleteData(Tables.ElementsTableName, $"WHERE IndicatorType = '{IndicatorConstants.FREC_STN}'");
            _sqlRepository.ElementsInsert(elements, Tables.ElementsTableName);
        }
        public void SetActions(string IndicatorType)
        {
            var actions = _sqlRepository.GetActions(EndDate, IndicatorType);
            if(IndicatorType=="STR")
                _sqlRepository.DeleteData(Tables.ActionsTableName, $"WHERE ActionType = '{CalculationConstants.FREC_STR}'");
            else
                _sqlRepository.DeleteData(Tables.ActionsTableName, $"WHERE ActionType = '{CalculationConstants.FREC_STN}'");

            _sqlRepository.ActionsInsert(actions, Tables.ActionsTableName);
        }

    }
}
