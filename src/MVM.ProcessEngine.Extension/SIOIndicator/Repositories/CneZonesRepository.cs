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
   public class CneZonesRepository: SQLRepository
    {
        public CneZonesRepository(string tenant) : base(tenant) { }

        public List<CneZone> GetCneZones(DateTime fechaFinMes, List<string> codigoActivo)
        {
            List<CneZone> cneZones = new List<CneZone>();

            SqlDatabase db = new SqlDatabase(ManeuverOriginConnectionString);

            var sql = @"with zonas as (
							SELECT CASE WHEN zones.StartDate < DATEADD(MONTH, -1, DATEADD(s, 1, @pFechaFinMes)) 
											THEN DATEADD(MONTH, -1, DATEADD(s, 1, @pFechaFinMes)) 
											ELSE zones.StartDate END StartDate, 
									ISNULL(zones.EndDate, @pFechaFinMes) EndDate, Ele.ElementId, Ele.Element, zones.Description Zone, tv.value State,
									lag(zones.EndDate,1, zones.EndDate) over (partition by Ele.ElementId order by zones.StartDate) LastEnd,
									lead(zones.StartDate,1, zones.StartDate) over (partition by Ele.ElementId order by zones.StartDate) NextStart		
							FROM [dbo].[CneZones] zones
							INNER JOIN [dbo].[CneElements] Ele ON  zones.Id = ele.cneZoneId
							INNER JOIN [dbo].[TypeValues] tv ON zones.statusTypeId = tv.Id
							INNER JOIN [dbo].[Types] t ON tv.TypeId = t.Id
							WHERE (
								zones.StartDate >= DATEADD(MONTH, -1, DATEADD(s, 1, @pFechaFinMes)) and zones.EndDate <= @pFechaFinMes
								or 
								zones.StartDate <= DATEADD(MONTH, -1, DATEADD(s, 1, @pFechaFinMes)) and (zones.EndDate > DATEADD(MONTH, -1, DATEADD(s, 1, @pFechaFinMes)) or zones.EndDate is null)
								or
								(zones.EndDate >= @pFechaFinMes or zones.EndDate is null) and zones.StartDate < @pFechaFinMes
							)
							AND t.name ='Estados de Zonas CNE'
							AND tv.value  in ('Vigente','Finalizada','Aprobada')
							--AND Ele.ElementId in ('Bah0647')
						),
						overlap as (
							SELECT *,
								case when t.LastEnd between t.StartDate and t.EndDate
									then 1
									else
										case when t.NextStart between t.StartDate and t.EndDate
										then 1
										else -1 
									end 
								end grupo
							FROM zonas t
						),
						agrupados as (
							select *, (sum(case when grupo > 0 then 0 else abs(grupo) end) over (partition by ElementId order by StartDate) + 1) * grupo grupoFinal
							from overlap
						)
						select Min(StartDate) StartDate, Max(IsNull(EndDate, LastEnd)) EndDate, ElementId, Element, '' ZoneName, '' State
						From agrupados
						group by ElementId, Element, grupoFinal
						order by ElementId, grupoFinal";

            
            DbCommand command = db.GetSqlStringCommand(sql);
            command.CommandTimeout = TimeoutTransaction;
            db.AddInParameter(command, "@pFechaFinMes", SqlDbType.DateTime, fechaFinMes);

            var reader = db.ExecuteReader(command);

            while (reader.Read())
            {
                cneZones.Add(new CneZone()
                {
                    Id = Guid.NewGuid(),
                    StartDate = reader.IsDBNull(0) ? (DateTime?)null : reader.GetDateTime(0),
                    EndDate = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1),
                    ElementId = reader.GetString(2),
                    ElementName = reader.GetString(3),
                    ZoneName = reader.GetString(4),
                    State = reader.GetString(5)
                });
            }

            return cneZones;
        }

    }
}
