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
    public class IndicatorMHAIA_STR: Indicator
    {
        private MHAIA_STRRepository _sqlRepository;

        public IndicatorMHAIA_STR(string tenant, List<object> parametersProcess): base(tenant, parametersProcess)
        {
            _sqlRepository = new MHAIA_STRRepository(tenant);
        }

        public void SetElements()
        {
            var elements = _sqlRepository.GetElements(EndDate, ActiveCodes);
            _sqlRepository.DeleteData(Tables.ElementsTableName, $"WHERE IndicatorType = '{IndicatorConstants.STR}'");
            _sqlRepository.ElementsInsert(elements, Tables.ElementsTableName);
        }

        /// <summary>
        /// Get Consignments from origin, Delete Consignments from destination, Insert Consignments from destination
        /// </summary>
        /// <param name="parametersProcess"></param>
        public void SetConsignments()
        {
            StartDate = EndDate.AddSeconds(1).AddMonths(-1);

            var consignments = _sqlRepository.GetConsignments(StartDate, EndDate, ActiveCodes);
            _sqlRepository.DeleteData(Tables.ConsignmentsTableName, $"WHERE IndicatorType = '{CalculationConstants.MHAIA_STR}'");
            _sqlRepository.ConsignmentsInsert(consignments, Tables.ConsignmentsTableName);
        }

        /// <summary>
        /// Get Actions from origin, Delete Actions from destination, Insert Actions from destination
        /// </summary>
        /// <param name="parametersProcess"></param>
        public void SetActions()
        {
            StartDate = EndDate.AddSeconds(1).AddMonths(-1);

            var actions = _sqlRepository.GetActions(StartDate, EndDate, ActiveCodes);
            _sqlRepository.DeleteData(Tables.ActionsTableName, $"WHERE ActionType = '{CalculationConstants.MHAIA_STR}'");
            _sqlRepository.ActionsInsert(actions, Tables.ActionsTableName);
        }
    }
}
