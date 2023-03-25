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
using MVM.ProcessEngine.TO;
using MVM.ProcessEngine.Common.Helpers;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

namespace MVM.ProcessEngine.Core.DataAccess
{
    /// <summary>
    /// Clase que gestiona el acceso a las fuentes de datos SQL para el gestor de cálculos
    /// </summary>
    public class GestorDatosSQL : GestorDatosBase
    {
        #region Constructor
        /// <summary>
        /// Permite crear una nueva instancia del tipo <see cref="MVM.ProcessEngine.DataAccess.GestorDatosSQL"/>
        /// </summary>
        public GestorDatosSQL()
        {
            _formatoParametro = "@p{0}";
        } 
        #endregion

        #region Métodos sobreescritos
        /// <summary>
        /// <see cref="MVM.ProcessEngine.DataAccess.GetData"/>
        /// </summary>
        public override DataTable GetData(string tenant, RepositorioTO repository, ConfiguracionTO configuracion, List<object> buffer, string idGestor)
        {
            DataTable result = null;
            ValidateRepository(repository);

            //  Database db = DatabaseFactory.CreateDatabase(repository.NombreCadenaConexion);
            string stringConn = GestorCalculosHelper.GetMetadataValue(tenant,repository.NombreCadenaConexion, true);
            SqlDatabase db = new SqlDatabase(stringConn);


            DbCommand command = (!string.IsNullOrEmpty(repository.Sql)) ? db.GetSqlStringCommand(repository.Sql) : db.GetStoredProcCommand(repository.NombreProcedimiento);
            PrepareCommand(db, command, repository, configuracion, buffer, idGestor);
            command.CommandTimeout = TimeoutPorTransaccion;
            DataSet data = db.ExecuteDataSet(command);

            if (data != null && data.Tables.Count > 0)
                result = data.Tables[0];

            return result;
        }
        #endregion
    }
}
