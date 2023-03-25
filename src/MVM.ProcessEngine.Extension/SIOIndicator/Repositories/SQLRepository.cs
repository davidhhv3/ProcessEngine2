using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using MVM.ProcessEngine.Common.Helpers;
using MVM.ProcessEngine.Extension.SIOIndicator.Constants;
using MVM.ProcessEngine.Extension.SIOIndicator.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Action = MVM.ProcessEngine.Extension.SIOIndicator.Domain.Action;

namespace MVM.ProcessEngine.Extension.SIOIndicator.Repositories
{
    public class SQLRepository
    {

        protected string ElementOriginConnectionString;
        protected string ConsignmentOriginConnectionString;
        protected string ManeuverOriginConnectionString;
        protected string OccurrenceOriginConnectionString;
        protected string DestinationConnectionString;
        protected int SqlBulkCopyBatchSize;
        protected int TimeoutTransaction;
        protected string UrlAuthentication;
        protected string UserAuthentication;
        protected string PasswordAuthentication;
        protected string UrlBatteries;
        protected string UrlGroups;
        protected string UrlCompanias;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenant"></param>
        public SQLRepository(string tenant)
        {
            try
            {
                ElementOriginConnectionString = GestorCalculosHelper.GetMetadataValue(tenant, "BDMIDXMConnectionString", true);
                ConsignmentOriginConnectionString = GestorCalculosHelper.GetMetadataValue(tenant, "DBConsignmentConnectionString", true);
                ManeuverOriginConnectionString = GestorCalculosHelper.GetMetadataValue(tenant, "DBManeuverConnectionString", true);
                OccurrenceOriginConnectionString = GestorCalculosHelper.GetMetadataValue(tenant, "DBOccurrenceConnectionString", true);
                DestinationConnectionString = GestorCalculosHelper.GetMetadataValue(tenant, "DBQualityIndicators", true);
                string sqlBulkCopyBatchSize = GestorCalculosHelper.GetMetadataValue(tenant, "SqlBulkCopyBatchSize", false);
                SqlBulkCopyBatchSize = (string.IsNullOrEmpty(sqlBulkCopyBatchSize)) ? 1 : (int.Parse(sqlBulkCopyBatchSize) > 10000 ? 10000 : int.Parse(sqlBulkCopyBatchSize));

                string timeoutTransaction = GestorCalculosHelper.GetMetadataValue(tenant, "TimeoutPorTransaccion", false);
                TimeoutTransaction = (string.IsNullOrEmpty(timeoutTransaction) ? 600 : int.Parse(timeoutTransaction));

                UrlAuthentication = GestorCalculosHelper.GetMetadataValue(tenant, "UrlAutenticacionMCD", true);
                UserAuthentication = GestorCalculosHelper.GetMetadataValue(tenant, "AutenticacionMCDUsuario", true);
                PasswordAuthentication = GestorCalculosHelper.GetMetadataValue(tenant, "AutenticacionMCDClave", true);
                UrlBatteries = GestorCalculosHelper.GetMetadataValue(tenant, "UrlBateriasMCD", true);
                UrlGroups = GestorCalculosHelper.GetMetadataValue(tenant, "UrlGruposMDC", true);
                UrlCompanias = GestorCalculosHelper.GetMetadataValue(tenant, "UrlMsvcCompanias", true);

            }
            catch (Exception ex)
            {

                throw new Exception("Error En SQLRepository - ConnectionString" + ex.Message);
            }        
        }
      
        
        


        

        

        
        

        
        
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName">Nombre de tabla donde se va a ejecutar la acción</param>
        /// <param name="filter">Condición que debe cumplir para realizar el borrado. Ej: WHERE id = 5 ()</param>
        public void DeleteData(string tableName, string filter = default)
        {
            SqlDatabase db = new SqlDatabase(DestinationConnectionString);
            var sql = "DELETE ";
                sql += tableName;

            if (!string.IsNullOrEmpty(filter))
                sql += $" {filter}";

            DbCommand command = db.GetSqlStringCommand(sql);
            command.CommandTimeout = TimeoutTransaction;
          //db.AddInParameter(command, "@contractId", SqlDbType.Int, contractId);
            db.ExecuteNonQuery(command);
        }

        /// <summary>
        /// Insert Data to Elements using BulkCopy
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        public void ElementsInsert(List<Element> elements, string tableName)
        {
            DataTable dataTable = ConvertToDatatTable(elements);
            dataTable.TableName = tableName;
            BulkCopy(dataTable);
        }

        /// <summary>
        /// Insert Data to GenUnit using BulkCopy
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        public void UnitsInsert(List<GenUnit> elements, string tableName)
        {
            DataTable dataTable = ConvertToDatatTable(elements);
            dataTable.TableName = tableName;
            BulkCopy(dataTable);
        }

        /// <summary>
        /// Insert Data to Consignments using BulkCopy
        /// </summary>
        /// <param name="consignments"></param>
        /// <returns></returns>
        public void ConsignmentsInsert(List<Consignment> consignments,string tableName)
        {
            DataTable dataTable = ConvertToDatatTable(consignments);
            dataTable.TableName = tableName;
            BulkCopy(dataTable);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consignments"></param>
        /// <param name="tableName"></param>
        public void ActionsInsert(List<Action> actions, string tableName)
        {
            DataTable dataTable = ConvertToDatatTable(actions);
            dataTable.TableName = tableName;
            BulkCopy(dataTable);
        }

        public void CneZonesInsert(List<CneZone> CneZones, string tableName)
        {
            DataTable dataTable = ConvertToDatatTable(CneZones);
            dataTable.TableName = tableName;
            BulkCopy(dataTable);
        }
        public void AffectationsInsert(List<Affectation> affectations, string tableName)
        {
            DataTable dataTable = ConvertToDatatTable(affectations);
            dataTable.TableName = tableName;
            BulkCopy(dataTable);
        }
        public void SecurityGenerationInsert(List<SecurityGeneration> securityGeneration, string tableName)
        {
            DataTable dataTable = ConvertToDatatTable(securityGeneration);
            dataTable.TableName = tableName;
            BulkCopy(dataTable);
        }


        public void CausesInsert(List<Cause> causes, string tableName)
        {
            DataTable dataTable = ConvertToDatatTable(causes);
            dataTable.TableName = tableName;
            BulkCopy(dataTable);
        }
        public void SuspensionsInsert(List<Suspension> suspensions, string tableName)
        {
            DataTable dataTable = ConvertToDatatTable(suspensions);
            dataTable.TableName = tableName;
            BulkCopy(dataTable);
        }

        public void TopologiesInsert(List<Topology> topologies, string tableName)
        {
            DataTable dataTable = ConvertToDatatTable(topologies);
            dataTable.TableName = tableName;
            BulkCopy(dataTable);
        }

        /// <summary>
        /// BulkCopy to SQL from DataTable
        /// </summary>
        /// <param name="dataTable">DataTable</param>
        private void BulkCopy(DataTable dataTable)
        {

            SqlDatabase db = new SqlDatabase(DestinationConnectionString);

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
