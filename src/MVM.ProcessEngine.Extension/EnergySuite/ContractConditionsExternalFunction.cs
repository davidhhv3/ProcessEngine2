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
        private static string ConditionTypeQuantity = "CONDCANT";
        private static string ConditionTypePrice = "CONDPREC";
        private static string ConditionTypeCharges = "CONDCARG";
        private static string ConditionTypeOtherCharges = "CONDOTCA";
        private static string ProductType = "TPEner";
        private static string GeneralRegion = "RGPais";

        private List<ContractCondition> ContractConditions;
        private List<ContractAditionalVariable> ContractAditionalVariables;
        private Dictionary<string, string> VariablesConcepts;
        private int ContractId;
        private byte VersionOfInformation = 0;
        private string Tenant;

        public DateTime StartDateOfExecution;
        public DateTime EndDateOfExecution;
        public List<DateTime> Holidays = new List<DateTime>();
        public List<TimeSlot> TimeSlots = new List<TimeSlot>();


        /// <summary>
        /// Execute Process
        /// </summary>
        /// <param name="parametersProcess"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public decimal Execute(string tenant, List<object> parametersProcess, List<object> buffer)
        {

            Tenant = tenant;

            // Get Parameters
            int contractCode = int.Parse(buffer[0].ToString());
            StartDateOfExecution = DateTime.Parse(parametersProcess[0].ToString());
            EndDateOfExecution = DateTime.Parse(parametersProcess[1].ToString());

            // Get Concepts
            VariablesConcepts = new ProcessManagerRepository().GetConcepts(tenant);

            // Get Holidays Excution Range
            Holidays = new SQLRepository(Tenant).GetHoliDays(StartDateOfExecution, EndDateOfExecution);

            // Get TimeSlot for Excution Range
            TimeSlots = new SQLRepository(Tenant).GetTimeSlots(StartDateOfExecution, EndDateOfExecution);

            // Run Transformation
            TransformQuantityandPriceConditions(contractCode);

            return ContractConditions.Count + ContractAditionalVariables.Count;
        }


        /// <summary>
        /// Get Contract's Conditions from MongoDB and Transform to Series Date/Hour and 
        /// put on SQL Server DB
        /// </summary>
        /// <param name="startDate">Start Date Of Execution</param>
        /// <param name="endDate">End Date Of Execution</param>
        /// <param name="contractCode">Code Of Contract</param>
        private void TransformQuantityandPriceConditions(int contractCode)
        {
            ContractConditions = new List<ContractCondition>();
            ContractAditionalVariables = new List<ContractAditionalVariable>();
            ContractId = contractCode;


            dynamic contractDynamic;
            string contractString;
            bool noFound = true;
            int retry = 0;

            do
            {
                // Get Contract from MongoDB
                var contractBson = new MongoRepository(Tenant).GetItemCollectionById("contract", "code", contractCode);
                if (contractBson == null) return;

                contractString = contractBson.ToString();
                // Deserialize Bson to dynamic
                contractDynamic = BsonSerializer.Deserialize<dynamic>(contractBson);

                //Validate Products 
                if (((IDictionary<string, object>)contractDynamic).ContainsKey("products"))
                {
                    noFound = false;
                }
                else
                {
                    retry++;
                }

            } while (noFound && retry < 3);


            if (noFound) throw new Exception($"No products in contract: {contractCode} {contractString}");

            // Loop Products of Contract 
            foreach (var product in contractDynamic.products)
            {
                // Only Energy
                if (product.productTypeCode != "productTypeEnergy") continue;

                // Quantities
                if (((IDictionary<string, object>)product).ContainsKey("quantities"))
                {
                    foreach (dynamic quantities in product.quantities)
                    {
                        SetContractCondition(quantities, ConditionTypeQuantity);
                    }
                }

                // Prices
                if (((IDictionary<string, object>)product).ContainsKey("prices"))
                {
                    foreach (var price in product.prices)
                    {
                        SetContractCondition(price, ConditionTypePrice);
                    }
                }

                // Charges
                if (((IDictionary<string, object>)product).ContainsKey("charges"))
                {
                    foreach (var charge in product.charges)
                    {
                        SetContractConditionForCharges(charge);
                    }
                }

                // Others Charges
                if (((IDictionary<string, object>)product).ContainsKey("otherCharges"))
                {
                    foreach (var otherCharge in product.otherCharges)
                    {
                        SetContractConditionForOtherCharges(otherCharge);
                    }
                }
            }

            // Save ContractConditions 
            SaveData();
        }

        /// <summary>
        /// Get Conditions from object Dynamic (qualitiy/price) and Add To ContractConditions List
        /// </summary>
        /// <param name="objectDyamic"></param>
        /// <param name="conditionType"></param>
        private void SetContractCondition(dynamic objectDyamic, string conditionType)
        {

            // Get Type of condition: Preocess only Type "Formula"
            if (!objectDyamic.type.ToString().Equals("Formula")) return;
            var formula = objectDyamic.formula.formulaCode;
            string formulaType = objectDyamic.formula.type;


            bool withLoadCenters = ((IDictionary<string, object>)objectDyamic).ContainsKey("loadCenters");
            bool withSubCodes = ((IDictionary<string, object>)objectDyamic).ContainsKey("subCode");

            // **********
            // LoadCenter
            // **********
            var loadCenters = new List<LoadCenter>(); // { new LoadCenter { LoadCenterId = null, Region = null } }; // Initial for Contract whitout load center 
            if (withLoadCenters)
            {
                var loadCentersId = ((List<object>)objectDyamic.loadCenters).Select(x => x.ToString()).ToList();

                if (loadCentersId[0] == null)
                {
                    loadCenters.Clear();
                    withLoadCenters = false;
                }
                else
                {
                    loadCenters = new SQLRepository(Tenant).GetLoadCenters(string.Join(",", loadCentersId));

                    // Loop for LoadCenter
                    foreach (var loadCenter in loadCenters)
                    {
                        AddCondition(formula, conditionType, formulaType, objectDyamic.period, loadCenter.LoadCenterId, loadCenter.Region);
                    }
                }
            }


            // **********
            // SubCodes
            // **********
            var subCodes = new List<string>();
            if (withSubCodes)
            {
                subCodes = ((List<object>)objectDyamic.subCode).Select(x => x?.ToString()).ToList();

                if (subCodes[0] == null)
                {
                    subCodes.Clear();
                    withSubCodes = false;
                }
                else
                {
                    // Loop for SubCodes
                    foreach (var subCode in subCodes)
                    {
                        AddCondition(formula, conditionType, formulaType, objectDyamic.period, subCode, GeneralRegion);
                    }
                }
            }

            // **********
            // with out LoadCenter and with out subCodes -- Only one condition (IdElement = null)
            // **********
            if (!withLoadCenters && !withSubCodes)
            {
                AddCondition(formula, conditionType, formulaType, objectDyamic.period, null, null);
            }


            // ***************
            // Check Aditionals Variables for the Formula 
            // **********
            if (((IDictionary<string, object>)objectDyamic.formula).ContainsKey("variables"))
            {
                SetAditionalVariables(objectDyamic.formula.variables, loadCenters, subCodes);
            }


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="formula"></param>
        /// <param name="conditionType"></param>
        /// <param name="formulaType"></param>
        /// <param name="period"></param>
        /// <param name="elementId"></param>
        /// <param name="region"></param>
        private void AddCondition(string formula, string conditionType, string formulaType, dynamic period, string elementId, string region)
        {
            // Eval Type - If is Price/Quantity (Regular)
            if (formulaType.Equals("Price") || formulaType.Equals("Quantity"))
            {
                // Get Dates/Hours of object
                var datesHours = GetDatesHours(period, region);

                // Validate Dates/Hours 
                if (datesHours == null) return;

                // Loop Date/Hours
                foreach (var dateHour in datesHours)
                {
                    ContractConditions.Add(new
                        ContractCondition()
                    {
                        ConditionId = Guid.NewGuid(),
                        ContractId = this.ContractId,
                        ProductType = ProductType,
                        ElementId = elementId,
                        Date = dateHour.Date,
                        Period = Byte.Parse(dateHour.Hour.ToString()),
                        ConditionType = conditionType,
                        CalculationId = Guid.Parse(formula)
                    }
                    );
                }
            }
            else
            { //Other , like : "Valorizacion"

                ContractConditions.Add(new
                  ContractCondition()
                {
                    ConditionId = Guid.NewGuid(),
                    ContractId = this.ContractId,
                    ProductType = ProductType,
                    ElementId = elementId,
                    Date = StartDateOfExecution,
                    Period = 0,
                    ConditionType = ConditionTypeCharges,
                    CalculationId = Guid.Parse(formula)
                }
              );
            }

        }




        /// <summary>
        /// Get Aditional Variables from object Dynamic (variables) and Add To ContractAditionalVariables List
        /// </summary>
        /// <param name="variables">dynamic object: Variables</param>
        /// <param name="loadCenters">List of Load Center</param>
        private void SetAditionalVariables(dynamic variables, List<LoadCenter> loadCenters, List<string> subCodes)
        {
            foreach (var variable in variables)
            {
                // Loop for LoadCenter
                foreach (var loadCenter in loadCenters)
                {
                    AddAditionaVariable(variable, loadCenter.LoadCenterId, loadCenter.Region);
                }

                // Loop for Subcode
                foreach (var subcode in subCodes)
                {
                    AddAditionaVariable(variable, subcode, GeneralRegion);
                }

                // with out LoadCenter and with out subCodes -- Only one condition (IdElement = null)
                if (loadCenters.Count == 0 && subCodes.Count == 0)
                {
                    AddAditionaVariable(variable, null, null);
                }

            }
        }


        private void AddAditionaVariable(dynamic variable, string elementId, string region)
        {
            // Get Dates/Hours of object
            var datesHours = GetDatesHours(variable.period, region);

            // Validate Dates/Hours 
            if (datesHours == null) return;

            // Validate Concept
            string conceptId = null;
            VariablesConcepts.TryGetValue(((string)variable.variableCode), out conceptId);
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

            // Loop Date/Hours
            foreach (DateHour dateHour in datesHours)
            {
                ContractAditionalVariables.Add(
                    SetAditionalVariable(dateHour.Date, Byte.Parse(dateHour.Hour.ToString()), conceptId, elementId, typeValue, value)
                );
            }
        }



        /// <summary>
        /// Get DatesHours
        /// </summary>
        /// <param name="objectDyamic"></param>
        /// <returns></returns>
        private List<DateHour> GetDatesHours(dynamic period, string region)
        {
            DateTime startDate = ((DateTime)period.startDate).Date;
            DateTime endDate = ((DateTime)period.endDate).Date;

            // Validate Range of Dates
            if (startDate <= EndDateOfExecution && endDate >= StartDateOfExecution)
            {
                FormulaCondition formulaCondition = new FormulaCondition();

                // Region
                formulaCondition.Region = region;

                // Start and End Dates
                formulaCondition.StartDate = startDate;
                formulaCondition.EndDate = endDate;

                // Type of Day 
                formulaCondition.NormalDay = (bool)period.dayType.normal;
                formulaCondition.Holiday = (bool)period.dayType.holiday;

                // All or Specif Days
                formulaCondition.AllDays = !((IDictionary<string, object>)period).ContainsKey("specificDays");

                // Get Type Day (ALL Normal and Holiday , ALL Normal, All Holiday, Specific)
                SelectedTypeDay selectTypeDay = EnergySuiteHelper.GetSelectionTypeDay(
                                                                                formulaCondition.AllDays,
                                                                                formulaCondition.NormalDay,
                                                                                formulaCondition.Holiday);
                // Get Days
                if (formulaCondition.AllDays)
                {
                    formulaCondition.Days = EnergySuiteHelper.GetDayOfWeek(new object[0], selectTypeDay);

                }
                else
                {
                    formulaCondition.Days = EnergySuiteHelper.GetDayOfWeek(period.specificDays.ToArray(), selectTypeDay);
                }

                // Hours
                // Specific Hours
                if (((IDictionary<string, object>)period).ContainsKey("specificPeriods"))
                {
                    formulaCondition.Hours = EnergySuiteHelper.GetHourOfRange(period.specificPeriods.ToArray());
                    formulaCondition.LoadTypes = new List<LoadType>() { LoadType.Specific };
                }
                else
                {
                    bool loadTypeLow = (bool)period.loadType.low;
                    bool loadTypeMedium = (bool)period.loadType.medium;
                    bool loadTypeHigh = (bool)period.loadType.high;
                    // formulaCondition.Hours = new List<int>();

                    formulaCondition.LoadTypes = new List<LoadType>();
                    // Load Type
                    if (loadTypeLow)
                    {
                        formulaCondition.LoadTypes.Add(LoadType.Low);
                        // formulaCondition.Hours.AddRange(EnergySuiteHelper.GetHourOfLoadType(LoadType.Low));
                    }

                    if (loadTypeMedium)
                    {
                        formulaCondition.LoadTypes.Add(LoadType.Medium);
                        //formulaCondition.Hours.AddRange(EnergySuiteHelper.GetHourOfLoadType(LoadType.Medium));
                    }

                    if (loadTypeHigh)
                    {
                        formulaCondition.LoadTypes.Add(LoadType.High);
                        //formulaCondition.Hours.AddRange(EnergySuiteHelper.GetHourOfLoadType(LoadType.High));
                    }
                }


                return GetDatesHoursByCondition(formulaCondition);
            }

            return null;
        }

        /// <summary>
        /// Get DatesHours By Condition
        /// </summary>
        /// <param name="formulaCondition"></param>
        /// <returns></returns>
        public List<DateHour> GetDatesHoursByCondition(FormulaCondition formulaCondition)
        {
            var datesHours = new List<DateHour>();

            // Loop Dates of condition
            for (DateTime date = formulaCondition.StartDate; date <= formulaCondition.EndDate; date = date.AddDays(1))
            {
                // Validate date in Range Execution
                if (date >= StartDateOfExecution && date <= EndDateOfExecution)
                {

                    // If condition contains dayOWeek of date 
                    if (formulaCondition.Days.ToList().Any(d => d.Equals(date.DayOfWeek)))
                    {
                        // Valide Holidays
                        // All or Specific but Exclude Holidays
                        if (formulaCondition.NormalDay && !formulaCondition.Holiday && Holidays.Contains(date)) continue;

                        // Specific Only Holyday
                        if (!formulaCondition.AllDays && !formulaCondition.NormalDay && formulaCondition.Holiday && !Holidays.Contains(date)) continue;

                        // Get and Loop Hours
                        List<int> hours = GetHours(formulaCondition, date);
                        foreach (var hour in hours)
                        {
                            datesHours.Add(new DateHour() { Date = date, Hour = hour });
                        }

                    }

                    // If Date is Holiday and Select All, include date on conditions!
                    if (Holidays.Contains(date) && formulaCondition.AllDays && !formulaCondition.NormalDay && formulaCondition.Holiday)
                    {

                        // Get and Loop Hours
                        List<int> hours = GetHours(formulaCondition, date);
                        foreach (var hour in hours)
                        {
                            datesHours.Add(new DateHour() { Date = date, Hour = hour });
                        }
                    }
                }
            }

            return datesHours;
        }

        /// <summary>
        /// Get Hours
        /// </summary>
        /// <param name="formulaCondition"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        private List<int> GetHours(FormulaCondition formulaCondition, DateTime date)
        {
            List<int> hours = new List<int>();

            if (formulaCondition.LoadTypes.Contains(LoadType.Specific))
            {
                hours = formulaCondition.Hours;
            }
            else
            {
                // Loop LoadTypes
                foreach (var slotType in formulaCondition.LoadTypes)
                {
                    hours.AddRange(TimeSlots.Where(w => w.Date.Equals(date) && w.Region.Equals(formulaCondition.Region) && w.SlotType.Equals(slotType)).Select(s => s.Hour).ToList());
                }
            }

            return hours;
        }



        /// <summary>
        /// Delete All exists Contidion for Range and Insert New Transformation
        /// </summary>
        private void SaveData()
        {
            new SQLRepository(Tenant).SaveData(ContractId, ProductType, StartDateOfExecution, EndDateOfExecution, ContractConditions, ContractAditionalVariables);
        }

        /// <summary>
        /// Set Aditional Variable Entity
        /// </summary>
        /// <param name="date"></param>
        /// <param name="period"></param>
        /// <param name="concept"></param>
        /// <param name="elementId"></param>
        /// <param name="typeValue"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private ContractAditionalVariable SetAditionalVariable(
           DateTime date,
           Byte period,
           string concept,
           string elementId,
           TypeValueAditionalVariable typeValue,
           object value
           )
        {

            var aditionalVariable = new ContractAditionalVariable();

            aditionalVariable.Date = date;
            aditionalVariable.Period = period;
            aditionalVariable.Version = VersionOfInformation;
            aditionalVariable.ProductType = ProductType;
            aditionalVariable.ConceptId = concept;
            aditionalVariable.ContractId = ContractId;
            aditionalVariable.ElementId = elementId;
            aditionalVariable.DateValue = null;

            switch (typeValue)
            {
                case TypeValueAditionalVariable.Decimal:
                    try { aditionalVariable.Value = decimal.Parse(value.ToString()); }
                    catch { aditionalVariable.Value = 0; }
                    break;
                case TypeValueAditionalVariable.Text:
                    aditionalVariable.TextValue = (string)value;
                    break;
                case TypeValueAditionalVariable.Date:
                    DateTime dateTime;
                    if (DateTime.TryParse(value.ToString(), out dateTime))
                    {
                        aditionalVariable.DateValue = dateTime.Date;
                    }
                    break;
                default:
                    break;
            }

            return aditionalVariable;

        }


    }
}
