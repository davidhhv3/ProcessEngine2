using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Common.Helpers
{
    public static class FileHelper
    {
        public static byte[] GetXlsFromDataTable(DataTable dataTable)
        {
            XLWorkbook wb = new XLWorkbook();
            wb.Worksheets.Add(dataTable, "data");

            using (var ms = new MemoryStream())
            {
                wb.SaveAs(ms);
                var workbookBytes = ms.ToArray();

                return workbookBytes;
            }
        }
    }
}
