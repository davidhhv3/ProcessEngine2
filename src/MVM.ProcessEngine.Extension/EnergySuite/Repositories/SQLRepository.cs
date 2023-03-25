using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using MVM.ProcessEngine.Common;
using MVM.ProcessEngine.Common.Helpers;
using MVM.ProcessEngine.Extension.EnergySuite.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Extension.EnergySuite.Repositories
{
    public class SQLRepository
    {

        private string StringConn ;
        private int SqlBulkCopyBatchSize;
        private int TimeoutTransaction;

        private string ContractConditionsTableName = "[Master].[ContractConditions]";
        private string ContractAditionalVariablesTableName = "[Transaction].[ContractAditionalVariables]";

        public SQLRepository(string tenant)
        {
            StringConn = GestorCalculosHelper.GetMetadataValue(tenant,"DBConnectionString", true);
            string sqlBulkCopyBatchSize = GestorCalculosHelper.GetMetadataValue(tenant,"SqlBulkCopyBatchSize", false);
            SqlBulkCopyBatchSize = (string.IsNullOrEmpty(sqlBulkCopyBatchSize)) ? 1 : (int.Parse(sqlBulkCopyBatchSize) > 10000 ? 10000 : int.Parse(sqlBulkCopyBatchSize));

            string timeoutTransaction = GestorCalculosHelper.GetMetadataValue(tenant,"TimeoutPorTransaccion", false);
            TimeoutTransaction = (string.IsNullOrEmpty(timeoutTransaction) ? 600 : int.Parse(timeoutTransaction));
        }
        /// <summary>
        /// Save Data in DB( Contract conditions and Contract Aditional Variables )
        /// </summary>
        /// <param name="contractId">Id of Contract</param>
        /// <param name="productType">Product Type</param>
        /// <param name="startDate">Atart Date</param>
        /// <param name="endDate">End Date</param>
        /// <param name="conditions">List of Condition from Contract</param>
        /// <param name="aditionalVariables">List of  Aditional Variables from Contract</param>
        public void SaveData(int contractId, string productType, DateTime startDate, DateTime endDate, IList<ContractCondition> conditions, IList<ContractAditionalVariable> aditionalVariables)
        {
            // Delete First
            DeleteData(contractId, productType, startDate, endDate, ContractConditionsTableName);
            DeleteData(contractId, productType, startDate, endDate, ContractAditionalVariablesTableName);

            ContractConditionsInsert(conditions);
            ContractAditionalVariablesInsert(aditionalVariables);
        }

        /// <summary>
        /// Get HoliDays for Range
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public List<DateTime> GetHoliDays(DateTime startDate, DateTime endDate)
        {
            List<DateTime> holidayList = new List<DateTime>();

            SqlDatabase db = new SqlDatabase(StringConn);
            var sql = "Select [Date] From [Master].[HolidayDates]";
            sql += " Where [Date] >= @startDate AND  [Date] <= @endDate ";

            DbCommand command = db.GetSqlStringCommand(sql);
            command.CommandTimeout = TimeoutTransaction;
            db.AddInParameter(command, "@startDate", SqlDbType.Date, startDate.ToString("o"));
            db.AddInParameter(command, "@endDate", SqlDbType.Date, endDate.ToString("o"));

            var reader = db.ExecuteReader(command);

            while (reader.Read())
            {
                holidayList.Add(reader.GetDateTime(0));
            }

            return holidayList;
        }

        /// <summary>
        /// Get Time Slots for Range
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public List<TimeSlot> GetTimeSlots(DateTime startDate, DateTime endDate)
        {
            List<TimeSlot> TimeSlotList = new List<TimeSlot>();

            SqlDatabase db = new SqlDatabase(StringConn);
            var sql = "WITH d AS ( ";
            sql += " SELECT TOP (DATEDIFF(DAY, @startDate, @endDate) + 1)  ";
            sql += " d = ROW_NUMBER() OVER (ORDER BY [object_id]) ";
            sql += " FROM sys.all_objects ) ";
            sql += " SELECT DATEADD(DAY, d-1, @startDate) AS [Date], TS.Region , TS.Hour ,TS.SlotType ";
            sql += " FROM d,[Master].TimeSlots AS TS ";
            sql += " WHERE  TS.DayType = ( SELECT CASE  ";
            sql += " WHEN EXISTS(SELECT * FROM [Master].HolidayDates AS HD WHERE HD.Date = DATEADD(DAY, d-1, @startDate)) ";
            sql += " THEN (SELECT Id FROM Master.TypeValues WHERE TypeId = 'TDIA' AND [Order] = 0) ";
            sql += " ELSE (SELECT Id FROM Master.TypeValues WHERE TypeId = 'TDIA' AND [Order] = DATEPART(WEEKDAY,DATEADD(DAY, d-1, @startDate))) ";
            sql += " END ) ";
            sql += " AND TS.StartDate <= DATEADD(DAY, d-1, @startDate) ";
            sql += " AND TS.EndDate >= DATEADD(DAY, d-1, @startDate) ";


            DbCommand command = db.GetSqlStringCommand(sql);
            command.CommandTimeout = TimeoutTransaction;
            db.AddInParameter(command, "@startDate", SqlDbType.Date, startDate.ToString("o"));
            db.AddInParameter(command, "@endDate", SqlDbType.Date, endDate.ToString("o"));

            var reader = db.ExecuteReader(command);
            LoadType? loadType = null;

            while (reader.Read())
            {

                switch (reader.GetString(3))
                {
                    case "FRBase": loadType = LoadType.Low; break;
                    case "FRInterm": loadType = LoadType.Medium; break;
                    case "FRPunta": loadType = LoadType.High; break;
                }

                TimeSlotList.Add(new TimeSlot()
                {
                    Date = reader.GetDateTime(0),
                    Region = reader.GetString(1),
                    Hour = reader.GetInt32(2),
                    SlotType = loadType
                });
            }

            return TimeSlotList;
        }

        /// <summary>
        /// Get Load Centers by Id List 
        /// </summary>
        /// <param name="loadCenters"></param>
        /// <returns></returns>
        public List<LoadCenter> GetLoadCenters(string loadCenters)
        {
            List<LoadCenter> LoadCenterList = new List<LoadCenter>();

            SqlDatabase db = new SqlDatabase(StringConn);
            var sql = "SELECT LoadCenterId , Region FROM [Master].LoadCenters AS LC ";
            sql += " WHERE LC.LoadCenterId IN (SELECT * FROM [dbo].[GetSplit] (@loadcenter,',')) ";
           

            DbCommand command = db.GetSqlStringCommand(sql);
            command.CommandTimeout = TimeoutTransaction;
            db.AddInParameter(command, "@loadcenter", SqlDbType.NVarChar, loadCenters);

            var reader = db.ExecuteReader(command);

            while (reader.Read())
            {

                LoadCenterList.Add(new LoadCenter()
                {
                    LoadCenterId = reader.GetString(0),
                    Region = reader.GetString(1)

                });
            }

            return LoadCenterList;
        }
        /// <summary>
        /// Delete Data (before Save)
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="productType"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="tableName"></param>
        private void DeleteData(int contractId, string productType, DateTime startDate, DateTime endDate , string tableName)
        {

            SqlDatabase db = new SqlDatabase(StringConn);
            var sql = "DELETE ";
                sql += tableName;
                sql += " WHERE [ContractId] = @contractId AND [ProductType] = @productType AND  [Date] >= @startDate AND  [Date] <= @endDate ";

            DbCommand command = db.GetSqlStringCommand(sql);
            command.CommandTimeout = TimeoutTransaction;
            db.AddInParameter(command, "@contractId", SqlDbType.Int, contractId);
            db.AddInParameter(command, "@productType", SqlDbType.VarChar, productType);
            db.AddInParameter(command, "@startDate", SqlDbType.Date, startDate.ToString("o"));
            db.AddInParameter(command, "@endDate", SqlDbType.Date, endDate.ToString("o"));

            db.ExecuteNonQuery(command);
        }

        /// <summary>
        /// Insert Data to ContractConditions using BulkCopy
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        private void ContractConditionsInsert(IList<ContractCondition> conditions)
        {
            DataTable dataTable = ConvertToDatatTable<ContractCondition>(conditions.ToList());
            dataTable.TableName = ContractConditionsTableName;
            BulkCopy(dataTable);
        }

        /// <summary>
        /// Insert Data to ContractAditionalVariables using BulkCopy
        /// </summary>
        /// <param name="aditionalVariables"></param>
        /// <returns></returns>
        private void ContractAditionalVariablesInsert(IList<ContractAditionalVariable> aditionalVariables)
        {
            DataTable dataTable = ConvertToDatatTable<ContractAditionalVariable>(aditionalVariables.ToList());
            dataTable.TableName = ContractAditionalVariablesTableName;
            BulkCopy(dataTable);
        }

      

        /// <summary>
        /// BulkCopy to SQL from DataTable
        /// </summary>
        /// <param name="dataTable">DataTable</param>
        private void BulkCopy(DataTable dataTable)
        {

            SqlDatabase db = new SqlDatabase(StringConn);

            using (SqlBulkCopy sbc = new SqlBulkCopy(db.ConnectionString))
            {
                sbc.DestinationTableName = dataTable.TableName;

                // Timeout
                sbc.BulkCopyTimeout = TimeoutTransaction;

                // Number of records to be processed in one go
                sbc.BatchSize = SqlBulkCopyBatchSize;

                // Mapping col
                foreach (var column in dataTable.Columns)
                {
                    sbc.ColumnMappings.Add(column.ToString(), column.ToString());
                }

                // Number of records after which client has to be notified about its status
                sbc.NotifyAfter = dataTable.Rows.Count;

                // Finally write to server
                sbc.WriteToServer(dataTable);
                sbc.Close();
            }
        }




        /// <summary>
        /// Convert List To DataTable
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="data">List of T</param>
        /// <returns>DataTable</returns>
        private DataTable ConvertToDatatTable<T>(List<T> data)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();

            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    table.Columns.Add(prop.Name, prop.PropertyType.GetGenericArguments()[0]);
                else
                    table.Columns.Add(prop.Name, prop.PropertyType);
            }

            object[] values = new object[props.Count];

            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }

            return table;
        }

    }
}
