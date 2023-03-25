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
    public class IndicatorIH_ICP_STR: Indicator
    {
        private IH_ICP_STRRepository _sqlRepository;

        public IndicatorIH_ICP_STR(string tenant, List<object> parametersProcess) : base(tenant, parametersProcess)
        {
            _sqlRepository = new IH_ICP_STRRepository(tenant);
        }

        public void SetUnits()
        {
            var units = _sqlRepository.GetUnits(EndDate, ActiveCodes);
            try
            {
                _sqlRepository.DeleteData(Tables.UnitsTableName);
                _sqlRepository.UnitsInsert(units, Tables.UnitsTableName);
            }
            catch (Exception ex)
            {

                throw new Exception("Error En ICP DeleteData - UnitsInsert - tabla:" + Tables.UnitsTableName + " - cantidad de unidades " + units.Count() + " - " + ex.Message);
            }
        }

        /// <summary>
        /// Carga de acciones HO, HI, HM, HD
        /// </summary>
        /// <param name="parametersProcess"></param>
        public void SetActions()
        {
            var actions = _sqlRepository.GetLoadActionsHO(EndDate, ActiveCodes);
            _sqlRepository.DeleteData(Tables.ActionsTableName, $"WHERE ActionType = '{CalculationConstants.HO}'");
            _sqlRepository.ActionsInsert(actions, Tables.ActionsTableName);

            actions = _sqlRepository.GetLoadActionsHI(EndDate, ActiveCodes);
            _sqlRepository.DeleteData(Tables.ActionsTableName, $"WHERE ActionType = '{CalculationConstants.HI}'");
            _sqlRepository.ActionsInsert(actions, Tables.ActionsTableName);

            actions = _sqlRepository.GetLoadActionsHM(EndDate, ActiveCodes);
            _sqlRepository.DeleteData(Tables.ActionsTableName, $"WHERE ActionType = '{CalculationConstants.HM}'");
            _sqlRepository.ActionsInsert(actions, Tables.ActionsTableName);

            actions = _sqlRepository.GetLoadActionsHD(EndDate, ActiveCodes);
            _sqlRepository.DeleteData(Tables.ActionsTableName, $"WHERE ActionType = '{CalculationConstants.HD}'");
            _sqlRepository.ActionsInsert(actions, Tables.ActionsTableName);
        }
    }
}
