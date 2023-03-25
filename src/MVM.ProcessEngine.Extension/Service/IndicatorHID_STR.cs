using MVM.ProcessEngine.Extension.SIOIndicator.Constants;
using MVM.ProcessEngine.Extension.SIOIndicator.Domain.Enumerations;
using MVM.ProcessEngine.Extension.SIOIndicator.Repositories;
using System;
using System.Collections.Generic;

namespace MVM.ProcessEngine.Extension.Service
{
    public class IndicatorHID_STR: Indicator
    {
        private HID_STRRepository _sqlRepository;

        public IndicatorHID_STR(string tenant, List<object> parametersProcess) : base(tenant, parametersProcess)
        {
            _sqlRepository = new HID_STRRepository(tenant);
        }

        public void SetElements()
        {
            var elements = _sqlRepository.GetElements(EndDate, ActiveCodes);
            _sqlRepository.DeleteData(Tables.ElementsTableName, $"WHERE IndicatorType = '{IndicatorConstants.STR}'");
            _sqlRepository.ElementsInsert(elements, Tables.ElementsTableName);
        }

        public void SetActions()
        {
            var actions = _sqlRepository.GetActions(EndDate, ActiveCodes) ;
            actions.AddRange(_sqlRepository.GetOccurrencesESP(EndDate, ActiveCodes));

            _sqlRepository.DeleteData(Tables.ActionsTableName, $"WHERE ActionType = '{CalculationConstants.HID_STR}'");
            _sqlRepository.ActionsInsert(actions, Tables.ActionsTableName);
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="parametersProcess"></param>
        public void SetConsignments()
        {
            StartDate = EndDate.AddSeconds(1).AddMonths(-1);

            var consignments = _sqlRepository.GetConsignments(StartDate, EndDate, ActiveCodes);
            var consignmentsEmergency = _sqlRepository.GetConsignmentsEmergency(StartDate, EndDate, ActiveCodes);
            var consignmentsScheduled = _sqlRepository.GetConsignmentsScheduled(StartDate, EndDate, ActiveCodes);
            var consignmentsDesvRD = _sqlRepository.GetConsignmentsDesv_RD(StartDate, EndDate, ActiveCodes, "STR");

            _sqlRepository.DeleteData(Tables.ConsignmentsTableName, $"WHERE IndicatorType = '{CalculationConstants.HID_STR}'");
            _sqlRepository.ConsignmentsInsert(consignments, Tables.ConsignmentsTableName);
            _sqlRepository.ConsignmentsInsert(consignmentsEmergency, Tables.ConsignmentsTableName);
            _sqlRepository.ConsignmentsInsert(consignmentsScheduled, Tables.ConsignmentsTableName);
            _sqlRepository.ConsignmentsInsert(consignmentsDesvRD, Tables.ConsignmentsTableName);
        }

    }
}
