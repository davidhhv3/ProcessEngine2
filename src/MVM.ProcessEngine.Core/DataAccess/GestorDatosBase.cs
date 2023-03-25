#region Derechos Reservados
// ===================================================
// Desarrollado Por	    : esteban.giraldo
// Fecha de Creación	: 2015-02-06
// Modificado Por       : esteban.giraldo
// Fecha Modificación   : 2015-02-26
// Empresa		        : MVM S.A.S
// ===================================================
#endregion
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Data;
using MVM.ProcessEngine.Common.Helpers;
using MVM.ProcessEngine.Common.Exceptions;
using MVM.ProcessEngine.TO;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

namespace MVM.ProcessEngine.Core.DataAccess
{
    /// <summary>
    /// Clase base para el acceso a datos con el gestor de cálculos
    /// </summary>
    public abstract class GestorDatosBase
    {
        #region Variables miembros de la clase
        /// <summary>
        /// Formato del parámetro
        /// </summary>
        /// <remarks>Se utiliza por defecto el formato del parámetro de SQL Server</remarks>
        protected string _formatoParametro = "@p{0}";

        //static private string _SqlBulkCopyBatchSize = GestorCalculosHelper.ObtenerAtributoDeConfiguracion("SqlBulkCopyBatchSize", false);
        //protected int SqlBulkCopyBatchSize = (string.IsNullOrEmpty(_SqlBulkCopyBatchSize)) ? 1 : (int.Parse(_SqlBulkCopyBatchSize) > 100000 ? 100000 : int.Parse(_SqlBulkCopyBatchSize));
        //static private string _TimeoutPorTransaccion = GestorCalculosHelper.ObtenerAtributoDeConfiguracion("TimeoutPorTransaccion", false);
        //protected int TimeoutPorTransaccion = (string.IsNullOrEmpty(_TimeoutPorTransaccion) ? 600 : int.Parse(_TimeoutPorTransaccion));

        #endregion

        #region Propiedades
        protected int SqlBulkCopyBatchSize = 10000;
        protected int TimeoutPorTransaccion =600;

        #endregion

        #region Métodos protegidos

        /// <summary>
        /// Permite obtener una lista desde la base de datos
        /// </summary>
        /// <param name="repository">Repositorio con la información necesaria para la ejecución</param>
        /// <param name="buffer">Obtiene o establece la lista de parámetros en memoria del cálculo</param>
        /// <param name="configuracion">Información de configuración del cálculo</param>
        /// <param name="idGestor">Identificador del proceso.</param>
        /// <returns>Estructura con la información de retorno</returns>
        public abstract DataTable GetData(string tenant, RepositorioTO repository, ConfiguracionTO configuracion, List<object> buffer, string idGestor);

        /// <summary>
        /// Permite validar el objeto repository
        /// </summary>
        /// <param name="repository">Objeto repository a validar</param>
        protected void ValidateRepository(RepositorioTO repository)
        {
            if (repository == null)
                throw new ArgumentNullException("repository");


            //SQL Injection Validation
            if (!string.IsNullOrEmpty(repository.Sql) && repository.Sql.ContainsAny("DROP", "CREATE", "DELETE"))
                throw new GestorCalculosException("GestorCalculosError_ValidacionInyeccionSQL", repository.ID);

            if (!string.IsNullOrEmpty(repository.SqlActualizacion) && repository.SqlActualizacion.ContainsAny("DROP", "CREATE", "DELETE"))
                throw new GestorCalculosException("GestorCalculosError_ValidacionInyeccionSQL", repository.ID);
            if (!repository.HabilitarActualizacion)
            {
                if (string.IsNullOrEmpty(repository.Sql) && string.IsNullOrEmpty(repository.NombreProcedimiento))
                    throw new GestorCalculosException("GestorCalculosError_RepositorioInvalido", repository.ID);
            }
            else
            {
                if (string.IsNullOrEmpty(repository.SqlActualizacion) && string.IsNullOrEmpty(repository.NombreProcedimientoActualizacion))
                    throw new GestorCalculosException("GestorCalculosError_RepositorioInvalido", repository.ID);
            }
        }

        /// <summary>
        /// Permite preparar el comando para ejecución acorde a los parámetros definidos
        /// </summary>
        /// <param name="db">objeto Database con el contexto de la ejecución actual</param>
        /// <param name="command">Objeto comando que contiene la instrucción a ejecutar</param>
        /// <param name="repository">Repositorio con la información necesaria para la ejecución</param>
        /// <param name="configuracion">Información de configuración del cálculo</param>
        /// <param name="buffer">Obtiene o establece la lista de parámetros en memoria del cálculo</param>
        /// <param name="idGestor">Identificador del proceso.</param>
        protected void PrepareCommand(Database db, DbCommand command, RepositorioTO repository, ConfiguracionTO configuracion, List<object> buffer, string idGestor)
        {
            if (repository.Parametros != null && repository.Parametros.Count > 0)
            {
                foreach (var parametro in repository.Parametros)
                {
                    object valor = parametro.Valor.ObtenerValorFormateado(configuracion.Nombre,
                            configuracion.Cultura, parametro, configuracion.Parametros, buffer, idGestor);

                    if (parametro.Direccion == DireccionValor.Entrada)
                    {
                        db.AddInParameter(command, string.Format(_formatoParametro, parametro.Nombre), parametro.TipoDato.GetDbType(), valor);
                        //Si el parámetro es de tipo ListString se establece el SqlDbType a estructurado (se utiliza TVP)
                        //TODO: Implementar genérico, que no quede solo para SQL Server
                        if (parametro.TipoDato == TipoDato.ListString)
                        {
                            var dbparam = (SqlParameter)command.Parameters[string.Format(_formatoParametro, parametro.Nombre)];
                            dbparam.SqlDbType = SqlDbType.Structured;
                            dbparam.TypeName = "dbo.ListItems";
                            continue;
                        }
                    }
                    else
                        db.AddOutParameter(command, string.Format(_formatoParametro, parametro.Nombre), parametro.TipoDato.GetDbType(), parametro.Tamano);
                }
            }
        }
        #endregion

        #region Métodos públicos

        /// <summary>
        /// Permite ejecutar una operación de almacenamiento sobre la base de datos
        /// </summary>
        /// <param name="repository">Repositorio con la información necesaria para la ejecución</param>
        /// <param name="buffer">Obtiene o establece la lista de parámetros en memoria del cálculo</param>
        /// <param name="idGestor">Identificador del proceso.</param>
        /// <returns>Retorna el número de filas afectadas</returns>
        /// <remarks>Método utilizado para las variables que se requieran almacenar</remarks>
        public int ExecuteNonQuery(string tenant, RepositorioTO repository, ConfiguracionTO configuracion, List<object> buffer, string idGestor)
        {
            int rowsAffected = 0;
            ValidateRepository(repository);

            //Database db = DatabaseFactory.CreateDatabase(repository.NombreCadenaConexion);
            string stringConn = GestorCalculosHelper.GetMetadataValue(tenant, repository.NombreCadenaConexion, true);
            SqlDatabase db = new SqlDatabase(stringConn);

            if (repository.HabilitarActualizacion)
            {
                //Se intenta actualizar el registro
                DbCommand updateCommand = (!string.IsNullOrEmpty(repository.Sql)) ? db.GetSqlStringCommand(repository.SqlActualizacion) : db.GetStoredProcCommand(repository.NombreProcedimientoActualizacion);
                PrepareCommand(db, updateCommand, repository, configuracion, buffer, idGestor);
                updateCommand.CommandTimeout = TimeoutPorTransaccion;
                rowsAffected = db.ExecuteNonQuery(updateCommand);
            }

            if (rowsAffected == 0)
            {
                //Se intenta insertar el registro 
                DbCommand insertCommand = (!string.IsNullOrEmpty(repository.Sql)) ? db.GetSqlStringCommand(repository.Sql) : db.GetStoredProcCommand(repository.NombreProcedimiento);
                PrepareCommand(db, insertCommand, repository, configuracion, buffer, idGestor);
                insertCommand.CommandTimeout = TimeoutPorTransaccion;
                rowsAffected = db.ExecuteNonQuery(insertCommand);
            }

            return rowsAffected;
        }


        /// <summary>
        /// Permite ejecutar una operación y retornar un valor
        /// </summary>
        /// <param name="repository">Repositorio con la información necesaria para la ejecución</param>
        /// <param name="buffer">Obtiene o establece la lista de parámetros en memoria del cálculo</param>
        /// <param name="idGestor">Identificador del proceso.</param>
        /// <returns>El valor resultante</returns>
        public object ExecuteScalar(string tenant, RepositorioTO repository, ConfiguracionTO configuracion, List<object> buffer, string idGestor)
        {

            // TODO: Tunning
            ValidateRepository(repository);
            //Database db = DatabaseFactory.CreateDatabase(repository.NombreCadenaConexion);
            string stringConn = GestorCalculosHelper.GetMetadataValue(tenant,repository.NombreCadenaConexion, true);
            SqlDatabase db = new SqlDatabase(stringConn);

            DbCommand command = (!string.IsNullOrEmpty(repository.Sql)) ? db.GetSqlStringCommand(repository.Sql) : db.GetStoredProcCommand(repository.NombreProcedimiento);
            PrepareCommand(db, command, repository, configuracion, buffer, idGestor);
            command.CommandTimeout = TimeoutPorTransaccion;
            return db.ExecuteScalar(command);

        }

        /// <summary>
        /// Permite ejecutar una operación de BulkCopy a la DB
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="dataTable"></param>
        public void ExecuteNonQueryBulkCopy(string tenant, RepositorioTO repository, DataTable dataTable)
        {
            //Database db = DatabaseFactory.CreateDatabase(repository.NombreCadenaConexion);
            string stringConn = GestorCalculosHelper.GetMetadataValue(tenant,repository.NombreCadenaConexion, true);
            SqlDatabase db = new SqlDatabase(stringConn);

            using (SqlBulkCopy sbc = new SqlBulkCopy(db.ConnectionString))
            {
                sbc.DestinationTableName = dataTable.TableName;

                sbc.BulkCopyTimeout = TimeoutPorTransaccion;

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
        #endregion
    }
}
