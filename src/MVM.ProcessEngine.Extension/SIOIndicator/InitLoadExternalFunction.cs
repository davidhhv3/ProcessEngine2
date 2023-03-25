using MVM.ProcessEngine.Extension.Service;
using MVM.ProcessEngine.Extension.SIOIndicator.Constants;
using MVM.ProcessEngine.Extension.SIOIndicator.Domain;
using MVM.ProcessEngine.Extension.SIOIndicator.Domain.Enumerations;
using MVM.ProcessEngine.Extension.SIOIndicator.Repositories;
using MVM.ProcessEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MVM.ProcessEngine.Extension.SIOIndicator
{
    public partial class InitLoadExternalFunction : IExternalFunction
    {
        private List<Element> Elements;
        private List<GenUnit> Units;
        private List<Consignment> Consignments;
        private List<Domain.Action> Actions;
        private string Tenant;
        private SQLRepository SqlRepository;

        /// <summary>
        /// Excute process (Command)
        /// </summary>
        /// <param name="tenant">tenan data</param>
        /// <param name="parametersProcess"> Process parameters</param>
        /// <param name="buffer">buffer</param>
        /// <returns></returns>parroquia santa ana fredonia
        public decimal Execute(string tenant, List<object> parametersProcess, List<object> buffer)
        {
            try
            {
                var _repCauses = new IndicatorCause(tenant, parametersProcess);
                _repCauses.GetCauses();

                switch (parametersProcess[1].ToString())
                {
                    case CalculationConstants.Todos_STR:
                        
                        var repMHAIASTR_Todos = new IndicatorMHAIA_STR(tenant, parametersProcess);
                        repMHAIASTR_Todos.SetElements();
                        repMHAIASTR_Todos.SetConsignments();
                        repMHAIASTR_Todos.SetActions();

                        var repHIDSTR_todos = new IndicatorHID_STR(tenant, parametersProcess);
                        repHIDSTR_todos.SetActions();
                        repHIDSTR_todos.SetConsignments();

                        var _repCneZones_Todos = new IndicatorCneZone(tenant, parametersProcess);
                        _repCneZones_Todos.GetCneZones();

                        var _repSuspensions_Todos = new IndicatorSuspensions(tenant, parametersProcess);
                        _repSuspensions_Todos.GetSuspensions();

                        var _repCneZones_HIR_Todos = new IndicatorCneZone(tenant, parametersProcess);
                        _repCneZones_HIR_Todos.GetCneZones();

                        var _repAffectations = new IndicatorAffectation(tenant, parametersProcess);
                        _repAffectations.GetAffectations("STR");

                        break;

                    case CalculationConstants.Todos_STN:

                        var repMHAIASTN_Todos = new IndicatorMHAIA_STN(tenant, parametersProcess);
                        repMHAIASTN_Todos.SetElements();
                        repMHAIASTN_Todos.SetConsignments();
                        repMHAIASTN_Todos.SetActions();

                        var repHIDSTN_todos = new IndicatorHID_STN(tenant, parametersProcess);
                        repHIDSTN_todos.SetConsignments();
                        repHIDSTN_todos.SetActions();

                        var _repCneZonesSTN_Todos = new IndicatorCneZone(tenant, parametersProcess);
                        _repCneZonesSTN_Todos.GetCneZones();

                        var _repSuspensionsSTN_Todos = new IndicatorSuspensions(tenant, parametersProcess);
                        _repSuspensionsSTN_Todos.GetSuspensions();

                        var _repCneZones_HIR_STN_Todos = new IndicatorCneZone(tenant, parametersProcess);
                        _repCneZones_HIR_STN_Todos.GetCneZones();

                        var _repAffectationsSTN_Todos = new IndicatorAffectation(tenant, parametersProcess);
                        _repAffectationsSTN_Todos.GetAffectations("STN");
                        _repAffectationsSTN_Todos.GetSecurityGeneration();

                        break;

                    case CalculationConstants.MHAIA_STR:
                       
                        var repMHAIASTR = new IndicatorMHAIA_STR(tenant, parametersProcess);
                        repMHAIASTR.SetElements();
                        repMHAIASTR.SetConsignments();
                        repMHAIASTR.SetActions();                   


                        break;
                    case CalculationConstants.HIDA_STR:
                    case CalculationConstants.THC_STR:
                    case CalculationConstants.HC_STR:
                    
                        var repElementsSTR = new IndicatorMHAIA_STR(tenant, parametersProcess);
                        repElementsSTR.SetElements();
                        break;

                    case CalculationConstants.HID_STR:
                        var _repMHAIASTR = new IndicatorMHAIA_STR(tenant, parametersProcess);
                         _repMHAIASTR.SetElements();                        
                        var repHIDSTR = new IndicatorHID_STR(tenant, parametersProcess);
                        repHIDSTR.SetActions();
                        
                        try
                        {
                            repHIDSTR.SetConsignments();
                        }
                        catch (Exception ex)
                        {

                            throw new Exception("Error En HID_STR - consignments " + parametersProcess?[0] + " - " + parametersProcess?[1] + " - " + parametersProcess?[2] + " - " + parametersProcess?[3] + " - " + parametersProcess?[4] + " - " + ex.Message);
                        }

                        var _repCneZones = new IndicatorCneZone(tenant, parametersProcess);
                        try
                        {
                            _repCneZones.GetCneZones();
                        }
                        catch (Exception ex)
                        {

                            throw new Exception("Error En HID_STR - zonas " + parametersProcess?[0] + " - " + parametersProcess?[1] + " - " + parametersProcess?[2] + " - " + parametersProcess?[3] + " - " + parametersProcess?[4] + " - " + ex.Message);
                        }

                        var _repSuspensions = new IndicatorSuspensions(tenant, parametersProcess);
                        try
                        {
                            _repSuspensions.GetSuspensions();
                        }
                        catch (Exception ex)
                        {

                            throw new Exception("Error En HID_STR - suspensions " + parametersProcess?[0] + " - " + parametersProcess?[1] + " - " + parametersProcess?[2] + " - " + parametersProcess?[3] + " - " + parametersProcess?[4] + " - " + ex.Message);
                        }

                       
                        break;
                    case CalculationConstants.HID_DESV_STR:

                        var _repDesvSTR = new IndicatorHID_STR(tenant, parametersProcess);
                        _repDesvSTR.SetElements();

                        var _repAffectationsTodos = new IndicatorAffectation(tenant, parametersProcess);
                        _repAffectationsTodos.GetAffectations("STR");


                        break;
                    case CalculationConstants.FREC_STR:

                        var _repFrecSTR = new IndicatorFrequency(tenant, parametersProcess);
                        _repFrecSTR.SetElementsSTR();
                        _repFrecSTR.SetActions("STR");

                        break;
                    case CalculationConstants.FREC_STN:

                        var _repFrecSTN = new IndicatorFrequency(tenant, parametersProcess);
                        _repFrecSTN.SetElementsSTN();
                        _repFrecSTN.SetActions("STN");

                        break;

                    case CalculationConstants.HID_DESV_STN:

                        var _repDesvSTN = new IndicatorMHAIA_STN(tenant, parametersProcess);
                        _repDesvSTN.SetElements();

                        var _repAffectationsSTN = new IndicatorAffectation(tenant, parametersProcess);
                        _repAffectationsSTN.GetAffectations("STN");
                        _repAffectationsSTN.GetSecurityGeneration();

                        break;
                    case CalculationConstants.HIR_STR:
                        var repElementsSTR_HIR = new IndicatorMHAIA_STR(tenant, parametersProcess);
                        repElementsSTR_HIR.SetElements();
                       
                        var _repCneZones_HIR = new IndicatorCneZone(tenant, parametersProcess);
                        _repCneZones_HIR.GetCneZones();
                        break;

                    case CalculationConstants.DispRealP:
                        var repDR = new IndicatorDispReal(tenant, parametersProcess);
                        repDR.SetUnits();
                        repDR.SetActions();
                        //repDR.SetConsignments();

                        break;
                    case CalculationConstants.IH_ICP:
                        
                            var repIHICPSTR = new IndicatorIH_ICP_STR(tenant, parametersProcess);
                            try
                            {
                                repIHICPSTR.SetUnits();
                            }
                            catch (Exception ex)
                            {

                                throw new Exception("Error En ICP SetUnits - " + parametersProcess?[0] + " - " + parametersProcess?[1] + " - " + parametersProcess?[2] + " - " + parametersProcess?[3] + " - " + parametersProcess?[4] + " - " + ex.Message);
                            }
                            try
                            {
                                repIHICPSTR.SetActions();
                            }
                            catch (Exception ex )
                            {

                                throw new Exception("Error En ICP SetActions - " + parametersProcess?[0] + " - " + parametersProcess?[1] + " - " + parametersProcess?[2] + " - " + parametersProcess?[3] + " - " + parametersProcess?[4] + " - " + ex.Message);
                            }
                                             

                        break;

                    case CalculationConstants.MHAIA_STN:
                        var repMHAIASTN = new IndicatorMHAIA_STN(tenant, parametersProcess);
                        repMHAIASTN.SetElements();
                        repMHAIASTN.SetConsignments();
                        repMHAIASTN.SetActions();
                        break;

                    case CalculationConstants.HID_STN:
                        var _repMHAIASTN = new IndicatorMHAIA_STN(tenant, parametersProcess);
                        _repMHAIASTN.SetElements();

                        var repHIDSTN = new IndicatorHID_STN(tenant, parametersProcess);
                        repHIDSTN.SetConsignments();
                        repHIDSTN.SetActions();

                        var _repCneZonesStn = new IndicatorCneZone(tenant, parametersProcess);
                        _repCneZonesStn.GetCneZones();

                        break;

                    case CalculationConstants.HIDA_STN:
                    case CalculationConstants.THC_STN:
                    case CalculationConstants.HC_STN:
                    
                        var repElementsSTN = new IndicatorMHAIA_STN(tenant, parametersProcess);
                        repElementsSTN.SetElements();
                        break;
                    case CalculationConstants.HIR_STN:
                        var repElementsSTN_HIR = new IndicatorMHAIA_STN(tenant, parametersProcess);
                        repElementsSTN_HIR.SetElements();

                        var _repCneZonesStn_HIR = new IndicatorCneZone(tenant, parametersProcess);
                        _repCneZonesStn_HIR.GetCneZones();

                        break;
                    case CalculationConstants.CRPCC:
                        var repElementsCRPCC = new IndicatorCRPCC(tenant, parametersProcess);
                        repElementsCRPCC.SetElements();
                        repElementsCRPCC.SetActions();
                        repElementsCRPCC.SetTopologies();


                        break;

                }

                return 1; //TODO preguntar que se necesita retornar en este metodo
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }
    }
}
