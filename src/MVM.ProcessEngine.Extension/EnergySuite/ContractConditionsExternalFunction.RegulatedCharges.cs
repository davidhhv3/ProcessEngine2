using MongoDB.Bson.Serialization;
using MVM.ProcessEngine.Extension.EnergySuite.Helpers;
using MVM.ProcessEngine.Extension.EnergySuite.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using MVM.ProcessEngine.Extension.EnergySuite.Domain;
using MVM.ProcessEngine.Interfaces;
using System.Globalization;
using MVM.ProcessEngine.Common.Helpers;

namespace MVM.ProcessEngine.Extension.EnergySuite
{
    public partial class ContractConditionsExternalFunction : IExternalFunction
    {

        /// <summary>
        /// Get Conditions from object Dynamic (charges) and Add To ContractConditions List
        /// </summary>
        /// <param name="objectDyamic"></param>
        private void SetContractConditionForCharges(dynamic objectDyamic)
        {
            // Valide startDate of Charge
            DateTime startDate = ((DateTime)objectDyamic.startDate).Date;
            if (startDate > EndDateOfExecution) return;

            // LoadCenter
            var loadCenters = new List<string>() { null };
            if (((IDictionary<string, object>)objectDyamic).ContainsKey("loadCenters"))
            {
                loadCenters = ((List<object>)objectDyamic.loadCenters).Select(x => x.ToString()).ToList();
            }

            //Get Formulas
            List<Guid> formulas = new List<Guid>();

            // hourly
            if (((IDictionary<string, object>)objectDyamic).ContainsKey("hourly"))
            {
                foreach (dynamic hourly in objectDyamic.hourly)
                {
                    formulas.Add(Guid.Parse(hourly));
                }
            }

            // monthly
            if (((IDictionary<string, object>)objectDyamic).ContainsKey("monthly"))
            {
                foreach (dynamic monthly in objectDyamic.monthly)
                {
                    formulas.Add(Guid.Parse(monthly));
                }
            }

            // Loop for LoadCenter
            foreach (var loadCenter in loadCenters)
            {
                // Formulas
                foreach (var formula in formulas)
                {
                    ContractConditions.Add(new
                        ContractCondition()
                    {
                        ConditionId = Guid.NewGuid(),
                        ContractId = this.ContractId,
                        ProductType = ProductType,
                        ElementId = loadCenter,
                        Date = StartDateOfExecution,
                        Period = 0,
                        ConditionType = ConditionTypeCharges,
                        CalculationId = formula
                    }
                    );
                }
            }
        }




    }
}
