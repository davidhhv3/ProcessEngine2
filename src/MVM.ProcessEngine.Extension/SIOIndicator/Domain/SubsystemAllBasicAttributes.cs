using System.Collections.Generic;

namespace MVM.ProcessEngine.Extension.SIOIndicator.Domain
{
    public class SubsystemAllBasicAttributes
    {
        public int GroupId { get; set; }
        public int GroupMRID { get; set; }
        public int GroupStateMRID { get; set; }
        public string GroupStateName { get; set; }
        public string GroupStateCode { get; set; }
        public int EnumGroupMRID { get; set; }
        public string EnumGroupName { get; set; }
        public string CodeMDC { get; set; }
        public int? OperatorCompanyMRID { get; set; }
        public string OperatorCompanyName { get; set; }
        public string OperatorCompanyCode { get; set; }
        public string UserName { get; set; }
        public string VersionComments { get; set; }
        public object ValidFrom { get; set; }
        public object ValidTo { get; set; }
        public string GroupName { get; set; }
    }

    public class SubsystemAllXmSubsystemGroup
    {
        public int SubsystemGroupMRID { get; set; }
        public double Voltage { get; set; }
        public int SubsystemType { get; set; }
        public string SubsystemTypeName { get; set; }
        public string SubsystemTypeCode { get; set; }
    }

    public class Item
    {
        public SubsystemAllBasicAttributes basicAttributes { get; set; }
        public object xmPlantGroup { get; set; }
        public object xmBayGroup { get; set; }
        public SubsystemAllXmSubsystemGroup xmSubsystemGroup { get; set; }
        public object xmBusbarGroup { get; set; }
    }

    public class SubsystemAllPagination
    {
        public int offset { get; set; }
        public int limit { get; set; }
        public int previousOffset { get; set; }
        public int nextOffset { get; set; }
        public int currentPage { get; set; }
        public int pageCount { get; set; }
        public int totalCount { get; set; }
    }

    public class SubsystemAllSortedBy
    {
        public object field { get; set; }
        public object order { get; set; }
    }

    public class SubsystemAllMetadata
    {
        public SubsystemAllPagination pagination { get; set; }
        public SubsystemAllSortedBy sortedBy { get; set; }
    }

    public class SubsystemAllData
    {
        public List<Item> items { get; set; }
        public SubsystemAllMetadata metadata { get; set; }
    }
}
