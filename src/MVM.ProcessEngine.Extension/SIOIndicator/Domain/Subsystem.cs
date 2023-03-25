using System.Collections.Generic;

namespace MVM.ProcessEngine.Extension.SIOIndicator.Domain
{
    public class Subsystem
    {
        public int GroupId { get; set; }
        public int GroupMRID { get; set; }
        public int GroupStateMRID { get; set; }
        public string GroupStateName { get; set; }
        public string GroupStateCode { get; set; }
        public int EnumGroupMRID { get; set; }
        public string EnumGroupName { get; set; }
        public string CodeMDC { get; set; }
        public object OperatorCompanyMRID { get; set; }
        public object OperatorCompanyName { get; set; }
        public object OperatorCompanyCode { get; set; }
        public string UserName { get; set; }
        public object VersionComments { get; set; }
        public object ValidFrom { get; set; }
        public object ValidTo { get; set; }
        public string GroupName { get; set; }
    }

    public class XmElementGroup
    {
        public int ElementGroupMRID { get; set; }
        public int ElementId { get; set; }
        public string ElementName { get; set; }
        public string CodeMDC { get; set; }
        public int ElementMRID { get; set; }
        public string ScadaCode { get; set; }
        public string ElementGroupValidFrom { get; set; }
        public string ElementValidFrom { get; set; }
        public object ElementGroupValidTo { get; set; }
        public string ElementValidTo { get; set; }
        public string ElementType { get; set; }
        public string ElementUserIdentifier { get; set; }
    }

    public class XmSubsystemGroup
    {
        public int SubsystemGroupMRID { get; set; }
        public double Voltage { get; set; }
        public int SubsystemType { get; set; }
        public string SubsystemTypeName { get; set; }
        public string SubsystemTypeCode { get; set; }
    }

    public class SubsystemData
    {
        public Subsystem BasicAttributes { get; set; }
        public object XmPlantGroup { get; set; }
        public List<XmElementGroup> XmElementGroup { get; set; }
        public object XmBayGroup { get; set; }
        public XmSubsystemGroup XmSubsystemGroup { get; set; }
        public object XmBusbarGroup { get; set; }
        public List<object> XmAssociatedGroup { get; set; }
    }
}
