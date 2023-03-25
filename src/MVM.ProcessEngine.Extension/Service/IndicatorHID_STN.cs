using MVM.ProcessEngine.Extension.SIOIndicator.Constants;
using MVM.ProcessEngine.Extension.SIOIndicator.Domain;
using MVM.ProcessEngine.Extension.SIOIndicator.Domain.Enumerations;
using MVM.ProcessEngine.Extension.SIOIndicator.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Extension.Service
{
    public class IndicatorHID_STN: Indicator
    {
        private HID_STNRepository _sqlRepository;

        public IndicatorHID_STN(string tenant, List<object> parametersProcess) : base(tenant, parametersProcess)
        {
            _sqlRepository = new HID_STNRepository(tenant);
        }

        public void SetElements()
        {
            var elements = _sqlRepository.GetElements(EndDate, ActiveCodes);
            _sqlRepository.DeleteData(Tables.ElementsTableName, $"WHERE IndicatorType = '{IndicatorConstants.STN}'");
            _sqlRepository.ElementsInsert(elements, Tables.ElementsTableName);
        }

        public void SetConsignments()
        {
            StartDate = EndDate.AddSeconds(1).AddMonths(-1);

            var consignments = _sqlRepository.GetConsignments(StartDate, EndDate, ActiveCodes);
            var consignmentsDesvRD = _sqlRepository.GetConsignmentsDesv_RD(StartDate, EndDate, ActiveCodes, "STN");        
            _sqlRepository.DeleteData(Tables.ConsignmentsTableName, $"WHERE IndicatorType = '{IndicatorConstants.STN}'");
            _sqlRepository.ConsignmentsInsert(consignments, Tables.ConsignmentsTableName);
            _sqlRepository.ConsignmentsInsert(consignmentsDesvRD, Tables.ConsignmentsTableName);
        }

        public void SetActions()
        {
            StartDate = EndDate.AddSeconds(1).AddMonths(-1);

            var actions = _sqlRepository.GetActions(EndDate, ActiveCodes) ?? new List<Action>();
            actions.AddRange(_sqlRepository.GetOccurrencesESP(EndDate, ActiveCodes) ?? new List<Action>());

            _sqlRepository.DeleteData(Tables.ActionsTableName, $"WHERE ActionType = '{CalculationConstants.HID_STN}'");
            _sqlRepository.ActionsInsert(actions, Tables.ActionsTableName);
        }
    }
}
