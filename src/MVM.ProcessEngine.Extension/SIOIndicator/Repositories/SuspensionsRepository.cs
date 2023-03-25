using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Suspension = MVM.ProcessEngine.Extension.SIOIndicator.Domain.Suspension;

namespace MVM.ProcessEngine.Extension.SIOIndicator.Repositories
{
	public class SuspensionsRepository : SQLRepository
	{
		public SuspensionsRepository(string tenant) : base(tenant) { }

		public List<Suspension> GetSuspensions(DateTime fechaFinMes)
		{
			List<Suspension> suspensions = new List<Suspension>();

			SqlDatabase db = new SqlDatabase(ManeuverOriginConnectionString);

			var sql = @"SELECT Id,ConsignmentId,StartDate,EndDate,CauseValue      
						FROM dbo.Suspensions
						WHERE  
						(
							StartDate >= DATEADD(MONTH, -1, DATEADD(s, 1, @pFechaFinMes)) and EndDate <= @pFechaFinMes
														or 
							StartDate <= DATEADD(MONTH, -1, DATEADD(s, 1, @pFechaFinMes)) and (EndDate > DATEADD(MONTH, -1, DATEADD(s, 1, @pFechaFinMes)) or EndDate is null)
														or
							(EndDate >= @pFechaFinMes or EndDate is null) and StartDate < @pFechaFinMes
						)
						and causeValue in ('Riesgo de vida humana')
						and fileName is not null 
						";


			DbCommand command = db.GetSqlStringCommand(sql);
			command.CommandTimeout = TimeoutTransaction;
			db.AddInParameter(command, "@pFechaFinMes", SqlDbType.DateTime, fechaFinMes);

			var reader = db.ExecuteReader(command);

			while (reader.Read())
			{
				suspensions.Add(new Suspension()
				{
					Id = reader.GetGuid(0),
					ConsignmentId = reader.GetString(1),
					StartDate = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
					EndDate = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
					CauseValue= reader.GetString(4)
				});
			}

			return suspensions;
		}

	}
}
