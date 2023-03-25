using System;
using System.Collections.Generic;

namespace MVM.ProcessEngine.Extension.SIOIndicator.Domain
{
    public class Company
    {
        public int companiaMrid { get; set; }
        public string codigoCompania { get; set; }
        public DateTime fechaInicioVigencia { get; set; }
        public DateTime fechaFinVigencia { get; set; }
        public string nitCompania { get; set; }
        public string nombreCompania { get; set; }
        public string tipoParticipante { get; set; }
        public string estadoCompania { get; set; }
        public string direccionCompania { get; set; }
        public DateTime fechaActualizacionOds { get; set; }
        public string nombreCorto { get; set; }
        public string codigoMid { get; set; }
        public bool esPrivado { get; set; }
    }
    public class PaginationCompany
    {
        public int offset { get; set; }
        public int limit { get; set; }
        public int previousOffset { get; set; }
        public int nextOffset { get; set; }
        public int currentPage { get; set; }
        public int pageCount { get; set; }
        public int totalCount { get; set; }
    }

    public class SortedByCompany
    {
        public string field { get; set; }
        public string order { get; set; }
    }

    public class MetadataCompany
    {
        public Pagination pagination { get; set; }
        public SortedBy sortedBy { get; set; }
    }

    public class CompanyData
    {
        public List<Company> items { get; set; }
        public Metadata metadata { get; set; }
    }
}
