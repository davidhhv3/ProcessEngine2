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
        /// Get Conditions from object Dynamic (otherCharges) and Add To ContractConditions List
        /// </summary>
        /// <param name="objectDyamic"></param>
        private void SetContractConditionForOtherCharges(dynamic objectDyamic)
        {
            // Valide startDate of Charge
            DateTime startDate = ((DateTime)objectDyamic.startDate).Date;
            if (startDate > EndDateOfExecution) return;

            // Valide Formula
            if (!((IDictionary<string, object>)objectDyamic).ContainsKey("formula")) return;

            var formula = objectDyamic.formula.formulaCode;

            // LoadCenter
            var loadCenters = new List<string>() { null };
            if (((IDictionary<string, object>)objectDyamic).ContainsKey("loadCenters"))
            {
                loadCenters = ((List<object>)objectDyamic.loadCenters).Select(x => x.ToString()).ToList();
            }

            // Loop for LoadCenter
            foreach (var loadCenter in loadCenters)
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
                    ConditionType = ConditionTypeOtherCharges,
                    CalculationId = Guid.Parse(formula)
                }
                );
            }

            
            // Check Aditionals Variables for the Formula 
            if (((IDictionary<string, object>)objectDyamic.formula).ContainsKey("variables"))
            {
                SetAditionalVariablesOtherCharges(objectDyamic.formula.variables, loadCenters);
            }
        }

        /// <summary>
        /// Get Aditional Variables from object Dynamic (variables) and Add To ContractAditionalVariables List
        /// </summary>
        /// <param name="variables">dynamic object: Variables</param>
        /// <param name="loadCenters">List of Load Center</param>
        private void SetAditionalVariablesOtherCharges(dynamic variables, List<string> loadCenters)
        {
            foreach (var variable in variables)
            {
                DateTime startDate = ((DateTime)variable.startDate).Date;
                DateTime endDate = ((DateTime)variable.endDate).Date;

                // Validate Range of Dates
                if (startDate <= EndDateOfExecution && endDate >= StartDateOfExecution)
                {
                    // Loop for LoadCenter
                    // Note : Always loadcenter have minimus 1 element ( null = defalut) 
                    foreach (var loadCenter in loadCenters)
                    {
                        AddAditionalVariableOtherCharges(variable, loadCenter);
                    }
                }
            }
        }


        /// <summary>
        /// Add Aditional Variable for OtherCharges
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="elementId"></param>
        private void AddAditionalVariableOtherCharges(dynamic variable, string elementId)
        {

            // Validate Concept
            VariablesConcepts.TryGetValue(((string)variable.variableCode), out string conceptId);
            if (conceptId == null) return;

            // Value
            object value = null;

            // Type : Value , Date or List
            var typeValue = TypeValueAditionalVariable.Decimal;
            if (((IDictionary<string, object>)variable).ContainsKey("methodType"))
            {
                string typeValStr = variable.methodType.ToString();
                switch (typeValStr)
                {
                    case "Value":
                        typeValue = TypeValueAditionalVariable.Decimal;
                        value = variable.value;
                        break;
                    case "Date":
                        typeValue = TypeValueAditionalVariable.Date;
                        value = variable.value;
                        break;
                    case "List":
                        typeValue = TypeValueAditionalVariable.Text;
                        string valueList = string.Empty;

                        // Elements
                        if (((IDictionary<string, object>)variable).ContainsKey("elements"))
                        {
                            valueList = (string.Join(",", variable.elements));
                        }

                        value = valueList;
                        break;
                    default:
                        typeValue = TypeValueAditionalVariable.Decimal;
                        value = variable.value;
                        break;
                }
            }
            else // Assume Decimal
            {
                typeValue = TypeValueAditionalVariable.Decimal;
                value = variable.value;
            }

            // Insert variable only for start execution day and period 0
            ContractAditionalVariables.Add(SetAditionalVariable(StartDateOfExecution, 0, conceptId, elementId, typeValue, value));
        }

    }
}
