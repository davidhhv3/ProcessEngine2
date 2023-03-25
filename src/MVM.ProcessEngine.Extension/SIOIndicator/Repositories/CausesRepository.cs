using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using MVM.ProcessEngine.Extension.SIOIndicator.Constants;
using MVM.ProcessEngine.Extension.SIOIndicator.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CneZone = MVM.ProcessEngine.Extension.SIOIndicator.Domain.CneZone;

namespace MVM.ProcessEngine.Extension.SIOIndicator.Repositories
{
	public class CausesRepository : SQLRepository
	{
		public CausesRepository(string tenant) : base(tenant) { }

		public List<Cause> GetCauses()
		{
			List<Cause> causes = new List<Cause>();

			SqlDatabase db = new SqlDatabase(ManeuverOriginConnectionString);

			var sql = @"select causas.value,convert(bit,max(convert(int,IsExcluded))) IsExcluded,
						case when sistemas.Value='Generación' 
							then 'DispRealP'  
							else sistemas.Value
						end system 
						from [dbo].[TypeDependency] 
						inner join (
						select tv.id,tv.Value from [dbo].[Types] T
						inner join  [TypeValues] TV on t.Id=tv.TypeId
						where t.name in ('Listas Maestras Relacionadas')
						and tv.value IN ('Causa - Sistema')
						) causaSistema
						on causaSistema.id= dependencygroupId 
						inner join (
						select tv.id,tv.Value from [dbo].[Types] T
						inner join  [TypeValues] TV on t.Id=tv.TypeId
						where t.name in ('Sistemas')
						and tv.value IN ('STR','Stn','Generación')
						)sistemas on TypeTargetId = sistemas.Id
						inner join 
						(
						select tv.id,tv.Value from [dbo].[Types] T
						inner join  [TypeValues] TV on t.Id=tv.TypeId
						where t.name in ('Causa Cambio Disponibilidad')
						)causas
						on causas.id = TypeOriginId
						group by causas.value,sistemas.Value
						 
						 union 

						select causas.value, convert(bit,1) as IsExcluded,'IH_ICP' system 
						from [dbo].[TypeDependencyExternalPanningIndicators]
						inner join (
						select tv.id,tv.Value from [dbo].[Types] T
						inner join  [TypeValues] TV on t.Id=tv.TypeId
						where t.name in ('Listas Maestras Relacionadas')
						and tv.value IN ('Causa - Sistema')
						) causaSistema
						on causaSistema.id= dependencygroupId 
						inner join 
						(
						select tv.id,tv.Value from [dbo].[Types] T
						inner join  [TypeValues] TV on t.Id=tv.TypeId
						where t.name in ('Causa Cambio Disponibilidad')						
						)causas
						on causas.id = TypeOriginId
						group by causas.value";


			DbCommand command = db.GetSqlStringCommand(sql);
			command.CommandTimeout = TimeoutTransaction;
			
			var reader = db.ExecuteReader(command);

			while (reader.Read())
			{
				causes.Add(new Cause()
				{
					Id = Guid.NewGuid(),
					CauseName = reader.GetString(0),
					IsExcluded =reader.GetBoolean(1),
					System = reader.GetString(2),
				});
			}

			return causes;
		}

	}
}
