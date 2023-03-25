using System.Collections.Generic;

namespace MVM.ProcessEngine.Extension.SIOIndicator.Domain
{
    public class Battery
    {
        public BasicAttributes BasicAttributes { get; set; }
        public XmElementAttributes XmElementAttributes { get; set; }
        public List<XmBatteryUnit> XmBatteryUnit { get; set; }
    }
    public class BasicAttributes
    {
        public int? ElementId { get; set; }
        public int ElementMRID { get; set; }
        public string CodeMdc { get; set; }
        public string ElementName { get; set; }
        public string ElementUserIdentifier { get; set; }
        public object ScadaCode { get; set; }
        public string ElementType { get; set; }
        public int? ElementState { get; set; }
        public string ElementStateName { get; set; }
        public string ElementStateCode { get; set; }
        public string OperatorCompanyName { get; set; }
        public string OperatorCompanyCode { get; set; }
        public string IsCompleted { get; set; }
        public object TransmissionSystemType { get; set; }
        public object TransmissionSystemTypeName { get; set; }
        public object TransmissionSystemTypeNameCode { get; set; }
        public string IsUsage { get; set; }
        public string IsConnection { get; set; }
        public string IsShared { get; set; }
        public object FpoDate { get; set; }
        public string UserName { get; set; }
        public object ValidTo { get; set; }
        public object ValidFrom { get; set; }
        public object VersionComments { get; set; }
        public bool? IsAutomatedName { get; set; }
        public object ConnectionType { get; set; }
        public object ConnectionTypeName { get; set; }
        public object ConnectionTypeCode { get; set; }
        public int? Area { get; set; }
        public string AreaName { get; set; }
        public string AreaCode { get; set; }
        public int? SubArea { get; set; }
        public string SubAreaName { get; set; }
        public string SubAreaCode { get; set; }
        public string Municipality { get; set; }
        public string MunicipalityCode { get; set; }
        public string Department { get; set; }
        public string DepartmentCode { get; set; }
    }
    public class XmElementAttributes
    {
        public int? SaebMRID { get; set; }
        public int? ElementId { get; set; }
        public int? GeographicLocation { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Altitude { get; set; }
        public int? SubAreaID { get; set; }
        public double? RatedCapacity { get; set; }
        public double? NominalVoltaje { get; set; }
        public double? MaxP { get; set; }
        public double? DischargeMaxP { get; set; }
        public double? ChargeMaxP { get; set; }
        public double? MinimumPercentCharge { get; set; }
        public double? MinimumEfficiency { get; set; }
        public double? ChargeEfficiency { get; set; }
        public double? DischargeEfficiency { get; set; }
        public double? StorageEfficiency { get; set; }
        public double? TimeToMaximumPower { get; set; }
        public double? AdditionalCompensationType { get; set; }
        public string AdditionalCompensationTypeName { get; set; }
        public string AdditionalCompensationTypeCode { get; set; }
        public double? Statism { get; set; }
        public double? MinQ { get; set; }
        public double? MaxQ { get; set; }
        public double? KConstant { get; set; }
        public double? TInitialQ { get; set; }
        public double? Tr { get; set; }
        public double? Te { get; set; }
        public double? Tev { get; set; }
        public double? MaximumChargeSpeed { get; set; }
        public double? MaximumDischargeSpeed { get; set; }
        public double? AdjustedChargeSpeed { get; set; }
        public double? AdjustedDischargeSpeed { get; set; }
    }
    public class XmBatteryUnit
    {
        public int? BatteryUnitMRID { get; set; }
        public int? ElementId { get; set; }
        public int? ElementMRID { get; set; }
        public string ElementName { get; set; }
        public string ScadaCode { get; set; }
        public string ValidFrom { get; set; }
        public string CodeMdc { get; set; }
        public int? NumberOfInverters { get; set; }
    }
    public class Metadata
    {
        public Pagination Pagination { get; set; }
        public SortedBy SortedBy { get; set; }
    }

    public class Pagination
    {
        public int? Offset { get; set; }
        public int? Limit { get; set; }
        public int? PreviousOffset { get; set; }
        public int? NextOffset { get; set; }
        public int? CurrentPage { get; set; }
        public int? PageCount { get; set; }
        public int? TotalCount { get; set; }
    }

    public class SortedBy
    {
        public object Field { get; set; }
        public object Order { get; set; }
    }

    public class BatteryData
    {
        public List<Battery> Items { get; set; }
        public Metadata Metadata { get; set; }
    }
}
