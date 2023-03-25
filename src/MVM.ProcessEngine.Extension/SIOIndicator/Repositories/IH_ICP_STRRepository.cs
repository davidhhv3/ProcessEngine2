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
using Action = MVM.ProcessEngine.Extension.SIOIndicator.Domain.Action;

namespace MVM.ProcessEngine.Extension.SIOIndicator.Repositories
{
    public class IH_ICP_STRRepository: SQLRepository
    {
        public IH_ICP_STRRepository(string tenant) : base(tenant) { }

        public List<GenUnit> GetUnits(DateTime fechaFinMes, List<string> codigoActivo)
        {
            List<GenUnit> units = new List<GenUnit>();

            try
            {
                SqlDatabase db = new SqlDatabase(ElementOriginConnectionString);

                string sql = @"with UniBloque as (
							select u.[objID],u.[nombre],u.[fechaIni],u.[fechaFin],[fechaPrueba],[fechaOpera],[capNominal],[capEfectiva]
							,u.[tipo],[clasificacion] ,bv.valor CEN_Unit, c.nombre fuel
							from UnidadGeneracion u
							inner join BirrelacionValor bv on u.objID=bv.objID1 and u.fechaFin=bv.fechafin
							inner JOIN Combustible c on bv.objID2=c.objID
	                        WHERE bv.objID='Bir0042' and propiedad='capEfectiva' 
							and u.objID in ('Unt0001','Unt0002','Unt0077','Unt0311',
							'Unt0323','Unt0404','Unt0435','Unt0473')
							and u.fechaFin='2020-10-16 00:00:00'
							
						)
						,result as (
                        select distinct 
                        case when isnull(mc.objid,bvg.objid1) is null then 1 else 2 end grp,
                        isnull(mc.objid,bvg.objid1) as grup, 
                        m.objID GenerationGroupId, 
                        isnull(g.nombre,u.nombre)  GenerationGroupName,
                        isnull(G.tipo,u.tipo) GroupType,
                        isnull (B.objID2,u.objid) UnitId
                        ,isnull(isnull ( uu.nombre,g.nombre), u.nombre) UnitName 
                        ,isnull(uu.fechaOpera,u.fechaOpera) as StartDate
                       	
                     
					  from MaestroCol m
                        left join (select * from GrupoGeneracion where  fechaIni <= @pFechaFinMes 
	                        AND (fechaFin >= @pFechaFinMes OR fechaFin IS NULL))  g on m.objID=g.objID
                        left join (
							select [objID],[nombre],[fechaIni],[fechaFin]
						      ,[fechaPrueba],[fechaOpera],[capNominal],[capEfectiva]
								,[tipo],[clasificacion] from UnidadGeneracion where  fechaIni <= @pFechaFinMes 
	                        AND (fechaFin >= @pFechaFinMes OR fechaFin IS NULL)
							
							union

							select [objID],[nombre],[fechaIni],[fechaFin]
						      ,[fechaPrueba],[fechaOpera],[capNominal],[capEfectiva]
								,[tipo],[clasificacion] from UniBloque
							) u on m.objID=u.objID
                        left JOIN Birrelacion B ON B.objID1 = G.objID and B.objID = 'Bir0039'
                       left join (select * from  UnidadGeneracion  where fechaIni <= @pFechaFinMes 
	                        AND (fechaFin >= @pFechaFinMes OR fechaFin IS NULL))uu on  isnull (B.objID2,u.objid)= uu.objid
                        LEFT JOIN 
	                        (SELECT * FROM BirrelacionValor  
	                        WHERE objID='Bir0042' and propiedad='capEfectiva' 
	                        AND fechaIni <= @pFechaFinMes 
	                        AND (fechaFin >= @pFechaFinMes OR fechaFin IS NULL)
	                        ) bv on isnull (B.objID2,u.objid) = bv.objID1 
                        LEFT JOIN Combustible c on bv.objID2=c.objID
	                       LEFT JOIN 
	                        (SELECT bir.*, cg.nombre FROM BirrelacionValor  bir
	                        INNER JOIN Combustible cG on bir.objID2=cG.objID 
	                        WHERE bir.objID='Bir0042' and propiedad='capEfectiva' 
	                        AND fechaIni <= @pFechaFinMes 
	                        AND (fechaFin >= @pFechaFinMes OR fechaFin IS NULL)
	                        ) bvG on B.objID1 = bvG.objID1 
                        and bvG.objID2=c.objID
                          left join (select *  from MaestroCol where  colID = 'Col0082' 
			                        AND fechaIni <= @pFechaFinMes 
			                        AND (fechaFin >= @pFechaFinMes OR fechaFin IS NULL) )Mc 
                        on mc.objID=m.objID
                       where m.objID in (
                        select objID2 
                        from Birrelacion where objID='Bir0074'
                        AND fechaIni <= @pFechaFinMes 
                        AND (fechaFin >= @pFechaFinMes OR fechaFin IS NULL)
						
						)
                         and m.subconjunto in ('Eventos','GenEventos')
                        AND m.fechaIni <= @pFechaFinMes 
                        AND ((m.fechaFin >= @pFechaFinMes OR m.fechaFin IS NULL))
                        AND (  uu.clasificacion not like '%Autog Peq. Escala%' )
                        ), Units as (
                        SELECT GenerationGroupId,GenerationGroupName,GroupType,
                        a.UnitId,UnitName,StartDate
                        FROM result a
                        INNER JOIN (
                          SELECT MAx(grp)grp,UnitId
                            FROM result
                            GROUP BY UnitId
                        ) b ON a.grp = b.grp and a.UnitId=b.UnitId
						
						union
						
						select  [objID] GenerationGroupId, [nombre] GenerationGroupName, [tipo] GroupType,
                        [objID],[nombre],fechaOpera
						from UniBloque
						)
						select GenerationGroupId,GenerationGroupName,GroupType,
                        UnitId,UnitName,StartDate
						from Units ";

                if (codigoActivo.Count() > 0)
                    sql = sql + " where UnitId in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " )";

                DbCommand command = db.GetSqlStringCommand(sql);
                command.CommandTimeout = TimeoutTransaction;
                db.AddInParameter(command, "@pFechaFinMes", SqlDbType.DateTime, fechaFinMes);

                var reader = db.ExecuteReader(command);

                DataTable dt = new DataTable();

                dt.Load(reader);
                var rows = dt.AsEnumerable();

                try
                {
                    units = rows.Select(s => new GenUnit
                    {
                        Id = Guid.NewGuid(),
                        GenerationGroupId = s["GenerationGroupId"].ToString(),
                        GenerationGroupName = s["GenerationGroupName"].ToString(),
                        GroupType = s["GroupType"].ToString(),
                        UnitId = s["UnitId"].ToString(),
                        UnitName = s["UnitName"].ToString(),
                        //  CEN_Unit = s["CEN_Unit"].ToString(),
                        //  Fuel_Unit = s["Fuel_Unit"].ToString(),
                        //  CEN_Group = s["CEN_Group"].ToString(),
                        OperationStartDate = s["StartDate"] != DBNull.Value ? Convert.ToDateTime(s["StartDate"].ToString()) : (DateTime?)null
                    }).ToList();
                }
                catch (Exception ex)
                {

                    throw new Exception("Error En ICP GetUnits - asignando propiedades:" + ex.Message);
                }
            }
            catch (Exception ex)
            {

                throw new Exception("Error En ICP GetUnits :" +  ex.Message);
            }

            return units;
        }

        public List<Action> GetLoadActionsHO(DateTime fechaFinMes, List<string> codigoActivo)
        {
            List<Action> actions = new List<Action>();

            SqlDatabase db = new SqlDatabase(ManeuverOriginConnectionString);

            var sql = @"WITH 
	                        Registros as (
		                        SELECT a.Id EventId, InstructionTime, isnull(OccurrenceTime, ScheduledStartDate) OccurrenceTime, ConfirmationTime, ElementId, ConsignmentId,
		                            tcausa.[Name] ActionName, causa.[Value] [Type], tv.[Value] Movement, t.[Name] MovementType, ScheduledStartDate, a.NewAvailability, a.[Order], 
			                        case when tv.Value = 'En Servicio' then 1 else 2 end  ActionOrder,a.FuelName as fuel, CauseOrigin, FuelCEN , PlantCEN
		                        FROM Actions a 
		                        INNER JOIN [dbo].[TypeValues] tv on a.ActionTypeId = tv.Id 
		                        LEFT JOIN [dbo].[Types] t on tv.TypeId = t.Id

		                        LEFT JOIN [dbo].[TypeValues] causa on a.CauseOperationalId = causa.Id 
		                        LEFT JOIN [dbo].[Types] tcausa on causa.TypeId = tcausa.Id
		                        WHERE tv.Value in ('En Servicio', 'Fuera de Servicio') 
	                        )
	                        select EventId, InstructionTime, OccurrenceTime, ConfirmationTime, ElementId, ConsignmentId, ActionName, [Type], Movement, MovementType, ScheduledStartDate, isnull(NewAvailability, 0) NewAvailability,
		                        OccurrenceTime ValidFrom, lead(OccurrenceTime, 1, @pFechaFinMes) over (Partition by ElementId order by ElementId, ActionOrder, OccurrenceTime, [Order]) ValidTo, fuel, CauseOrigin, FuelCEN , PlantCEN
	                        from Registros 
	                        where OccurrenceTime BETWEEN dateadd(dd, datediff(dd, 0, DATEADD(MM, -36, @pFechaFinMes)), 0) and @pFechaFinMes ";

            if (codigoActivo.Count() > 0)
                sql = sql + " AND ElementId in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " ) ";

            sql += "order by ElementId, ActionOrder, OccurrenceTime, [Order]";


            DbCommand command = db.GetSqlStringCommand(sql);
            command.CommandTimeout = TimeoutTransaction;
            db.AddInParameter(command, "@pFechaFinMes", SqlDbType.DateTime, fechaFinMes);

            var reader = db.ExecuteReader(command);

            DataTable dt = new DataTable();

            dt.Load(reader);
            var rows = dt.AsEnumerable();

            actions = rows.Select(s => new Action
            {
                Id = Guid.NewGuid(),
                InstructionTime = s.Field<DateTime?>("InstructionTime"),
                OccurrenceTime = s.Field<DateTime?>("OccurrenceTime"),
                ConfirmationTime = s.Field<DateTime?>("ConfirmationTime"),
                ElementId = s["ElementId"].ToString(),
                ActionName = s["ActionName"].ToString(),
                Type = s["Type"].ToString(),
                ConsignmentId = s["ConsignmentId"].ToString(),
                NewAvailability = s.Field<decimal?>("NewAvailability") ?? 0,
                Movement = s["Movement"].ToString(),
                TypeMovement = s["MovementType"].ToString(),
                //EndOccurrenceTime = s.Field<DateTime?>("EndOccurrenceTime"),
                ScheduledStartDate = s.Field<DateTime?>("ScheduledStartDate"),
                //ScheduledEndDate = s.Field<DateTime?>("ScheduledEndDate"),
                ValidFrom = s.Field<DateTime?>("ValidFrom"),
                ValidTo = s.Field<DateTime?>("ValidTo"),
                Fuel = s["Fuel"].ToString(),
                //CauseOrigin = s["CauseOrigin"].ToString(),
                FuelCEN = s["FuelCEN"].ToString(),
                PlantCEN = s["PlantCEN"].ToString(),
                ActionType = CalculationConstants.HO,

            }).ToList();

            return actions;
        }

        public List<Action> GetLoadActionsHI(DateTime fechaFinMes, List<string> codigoActivo)
        {
            List<Action> actions = new List<Action>();

            SqlDatabase db = new SqlDatabase(ManeuverOriginConnectionString);

            var sql = @"WITH  causesExcluded AS
              (SELECT m causesExcluded
               FROM
                 (
                    select causas.value m
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
						group by causas.value

	              )MM)
	              ,Registros as (
		                    SELECT a.Id EventId, InstructionTime, isnull(OccurrenceTime, ScheduledStartDate) OccurrenceTime, ConfirmationTime, ElementId, 
		                    tcausa.[Name] ActionName, causa.[Value] [Type], tv.[Value] Movement, t.[Name] MovementType, ScheduledStartDate, a.NewAvailability, a.[Order], 
			                case when tv.Value = 'Indisponible' then 1 else 2 end  ActionOrder,a.FuelName as fuel, CauseOrigin, FuelCEN , PlantCEN
		                    FROM Actions a 
		                    INNER JOIN [dbo].[TypeValues] tv on a.ActionTypeId = tv.Id 
		                    LEFT JOIN [dbo].[Types] t on tv.TypeId = t.Id
				            LEFT JOIN [dbo].[TypeValues] causa on a.CauseOperationalId = causa.Id 
		                    LEFT JOIN [dbo].[Types] tcausa on causa.TypeId = tcausa.Id
		                    WHERE tv.Value in ('Indisponible', 'Disponible') 
				            and isnull(causa.[Value],'')<> 'Mantenimiento'
				            and isnull(causa.[Value],'') not in (select causesExcluded from causesExcluded)
	                        )
	                        select EventId, InstructionTime, OccurrenceTime, ConfirmationTime, ElementId, ActionName, [Type], Movement, MovementType, ScheduledStartDate, isnull(NewAvailability, 0) NewAvailability,
		                    OccurrenceTime ValidFrom, lead(OccurrenceTime, 1, @pFechaFinMes) over (Partition by ElementId order by ElementId, ActionOrder, OccurrenceTime, [Order]) ValidTo, fuel, CauseOrigin, FuelCEN , PlantCEN
	 			            from Registros 
	                        where OccurrenceTime BETWEEN dateadd(dd, datediff(dd, 0, DATEADD(MM, -36, @pFechaFinMes)), 0) and @pFechaFinMes ";

            if (codigoActivo.Count() > 0)
                sql = sql + " AND ElementId in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " ) ";

            sql += "order by ElementId, ActionOrder, OccurrenceTime, [Order]";


            DbCommand command = db.GetSqlStringCommand(sql);
            command.CommandTimeout = TimeoutTransaction;
            db.AddInParameter(command, "@pFechaFinMes", SqlDbType.DateTime, fechaFinMes);

            var reader = db.ExecuteReader(command);

            DataTable dt = new DataTable();

            dt.Load(reader);
            var rows = dt.AsEnumerable();

            actions = rows.Select(s => new Action
            {
                Id = Guid.NewGuid(),
                InstructionTime = s.Field<DateTime?>("InstructionTime"),
                OccurrenceTime = s.Field<DateTime?>("OccurrenceTime"),
                ConfirmationTime = s.Field<DateTime?>("ConfirmationTime"),
                ElementId = s["ElementId"].ToString(),
                ActionName = s["ActionName"].ToString(),
                Type = s["Type"].ToString(),
                ConsignmentId = s["ElementId"].ToString(),
                NewAvailability = s.Field<decimal?>("NewAvailability") ?? 0,
                Movement = s["Movement"].ToString(),
                TypeMovement = s["MovementType"].ToString(),
                //EndOccurrenceTime = s.Field<DateTime?>("EndOccurrenceTime"),
                ScheduledStartDate = s.Field<DateTime?>("ScheduledStartDate"),
                //ScheduledEndDate = s.Field<DateTime?>("ScheduledEndDate"),
                ValidFrom = s.Field<DateTime?>("ValidFrom"),
                ValidTo = s.Field<DateTime?>("ValidTo"),
                Fuel = s["Fuel"].ToString(),
                //CauseOrigin = s["CauseOrigin"].ToString(),
                FuelCEN = s["FuelCEN"].ToString(),
                PlantCEN = s["PlantCEN"].ToString(),
                ActionType = CalculationConstants.HI,
            }).ToList();

            return actions;
        }

        public List<Action> GetLoadActionsHM(DateTime fechaFinMes, List<string> codigoActivo)
        {
            List<Action> actions = new List<Action>();

            SqlDatabase db = new SqlDatabase(ManeuverOriginConnectionString);

            var sql = @"WITH Registros as (
		                    SELECT a.Id EventId, InstructionTime, isnull(OccurrenceTime, ScheduledStartDate) OccurrenceTime, ConfirmationTime, ElementId, 
		                    tcausa.[Name] ActionName, causa.[Value] [Type], tv.[Value] Movement, t.[Name] MovementType, ScheduledStartDate, a.NewAvailability, a.[Order], 
			                case when tv.Value = 'Indisponible' then 1 else 2 end  ActionOrder,a.FuelName as fuel, CauseOrigin, FuelCEN , PlantCEN
		                    FROM Actions a 
		                    INNER JOIN [dbo].[TypeValues] tv on a.ActionTypeId = tv.Id 
		                    LEFT JOIN [dbo].[Types] t on tv.TypeId = t.Id
				            LEFT JOIN [dbo].[TypeValues] causa on a.CauseOperationalId = causa.Id 
		                    LEFT JOIN [dbo].[Types] tcausa on causa.TypeId = tcausa.Id
		                    WHERE tv.Value in ('Indisponible', 'Disponible') 
				            and isnull(causa.[Value],'')= 'Mantenimiento'
				            
	                        )
	                        select EventId, InstructionTime, OccurrenceTime, ConfirmationTime, ElementId, ActionName, [Type], Movement, MovementType, ScheduledStartDate, isnull(NewAvailability, 0) NewAvailability,
		                    OccurrenceTime ValidFrom, lead(OccurrenceTime, 1, @pFechaFinMes) over (Partition by ElementId order by ElementId, ActionOrder, OccurrenceTime, [Order]) ValidTo, fuel, CauseOrigin, FuelCEN , PlantCEN
	 			            from Registros 
	                        where OccurrenceTime BETWEEN dateadd(dd, datediff(dd, 0, DATEADD(MM, -36, @pFechaFinMes)), 0) and @pFechaFinMes ";

            if (codigoActivo.Count() > 0)
                sql = sql + " AND ElementId in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " ) ";

            sql += "order by ElementId, ActionOrder, OccurrenceTime, [Order]";


            DbCommand command = db.GetSqlStringCommand(sql);
            command.CommandTimeout = TimeoutTransaction;
            db.AddInParameter(command, "@pFechaFinMes", SqlDbType.DateTime, fechaFinMes);

            var reader = db.ExecuteReader(command);

            DataTable dt = new DataTable();

            dt.Load(reader);
            var rows = dt.AsEnumerable();

            actions = rows.Select(s => new Action
            {
                Id = Guid.NewGuid(),
                InstructionTime = s.Field<DateTime?>("InstructionTime"),
                OccurrenceTime = s.Field<DateTime?>("OccurrenceTime"),
                ConfirmationTime = s.Field<DateTime?>("ConfirmationTime"),
                ElementId = s["ElementId"].ToString(),
                ActionName = s["ActionName"].ToString(),
                Type = s["Type"].ToString(),
                ConsignmentId = s["ElementId"].ToString(),
                NewAvailability = s.Field<decimal?>("NewAvailability") ?? 0,
                Movement = s["Movement"].ToString(),
                TypeMovement = s["MovementType"].ToString(),
                //EndOccurrenceTime = s.Field<DateTime?>("EndOccurrenceTime"),
                ScheduledStartDate = s.Field<DateTime?>("ScheduledStartDate"),
                //ScheduledEndDate = s.Field<DateTime?>("ScheduledEndDate"),
                ValidFrom = s.Field<DateTime?>("ValidFrom"),
                ValidTo = s.Field<DateTime?>("ValidTo"),
                Fuel = s["Fuel"].ToString(),
                //CauseOrigin = s["CauseOrigin"].ToString(),
                FuelCEN = s["FuelCEN"].ToString(),
                PlantCEN = s["PlantCEN"].ToString(),
                ActionType = CalculationConstants.HM,
            }).ToList();

            return actions;
        }

        public List<Action> GetLoadActionsHD(DateTime fechaFinMes, List<string> codigoActivo)
        {
            List<Action> actions = new List<Action>();

            SqlDatabase db = new SqlDatabase(ManeuverOriginConnectionString);

            var sql = @"WITH  causesExcluded AS
              (SELECT m causesExcluded
               FROM
                 (
                        select causas.value m
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
						group by causas.value
	              )MM)
	              ,Registros as (
		                  SELECT isnull(a.causeorigin,'') causeorigin,a.Id EventId, InstructionTime, isnull(OccurrenceTime, ScheduledStartDate) OccurrenceTime, ConfirmationTime, ElementId, 
		                    tcausa.[Name] ActionName, causa.[Value] [Type], tv.[Value] Movement, t.[Name] MovementType, ScheduledStartDate, a.NewAvailability, a.[Order], 
			                case when tv.Value = 'Cambio de disponibilidad' then 1 else 2 end  ActionOrder,a.FuelName as fuel, FuelCEN , PlantCEN
		                    FROM Actions a 
		                    INNER JOIN [dbo].[TypeValues] tv on a.ActionTypeId = tv.Id 
		                    LEFT JOIN [dbo].[Types] t on tv.TypeId = t.Id
				            LEFT JOIN [dbo].[TypeValues] causa on a.CauseOperationalId = causa.Id 
		                    LEFT JOIN [dbo].[Types] tcausa on causa.TypeId = tcausa.Id
		                    WHERE tv.Value in ('Cambio de disponibilidad', 'Finaliza cambio de disponibilidad') 
							and isnull(causa.[Value],'') not in (select causesExcluded from causesExcluded)
	                        )
	                        select EventId, InstructionTime, OccurrenceTime, ConfirmationTime, ElementId, ActionName, [Type], Movement, MovementType, ScheduledStartDate, isnull(NewAvailability, 0) NewAvailability,
		                    OccurrenceTime ValidFrom, lead(OccurrenceTime, 1, @pFechaFinMes) over (Partition by ElementId order by ElementId, ActionOrder, OccurrenceTime, [Order]) ValidTo, fuel, CauseOrigin, FuelCEN , PlantCEN
	 			            from Registros 
	                        where OccurrenceTime BETWEEN dateadd(dd, datediff(dd, 0, DATEADD(MM, -36, @pFechaFinMes)), 0) and @pFechaFinMes ";

            if (codigoActivo.Count() > 0)
                sql = sql + " AND ElementId in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " ) ";

            sql += "order by ElementId, ActionOrder, OccurrenceTime, [Order]";


            DbCommand command = db.GetSqlStringCommand(sql);
            command.CommandTimeout = TimeoutTransaction;
            db.AddInParameter(command, "@pFechaFinMes", SqlDbType.DateTime, fechaFinMes);

            var reader = db.ExecuteReader(command);

            DataTable dt = new DataTable();

            dt.Load(reader);
            var rows = dt.AsEnumerable();

            actions = rows.Select(s => new Action
            {
                Id = Guid.NewGuid(),
                InstructionTime = s.Field<DateTime?>("InstructionTime"),
                OccurrenceTime = s.Field<DateTime?>("OccurrenceTime"),
                ConfirmationTime = s.Field<DateTime?>("ConfirmationTime"),
                ElementId = s["ElementId"].ToString(),
                ActionName = s["ActionName"].ToString(),
                Type = s["Type"].ToString(),
                ConsignmentId = s["ElementId"].ToString(),
                NewAvailability = s.Field<decimal?>("NewAvailability") ?? 0,
                Movement = s["Movement"].ToString(),
                TypeMovement = s["MovementType"].ToString(),
                //EndOccurrenceTime = s.Field<DateTime?>("EndOccurrenceTime"),
                ScheduledStartDate = s.Field<DateTime?>("ScheduledStartDate"),
                //ScheduledEndDate = s.Field<DateTime?>("ScheduledEndDate"),
                ValidFrom = s.Field<DateTime?>("ValidFrom"),
                ValidTo = s.Field<DateTime?>("ValidTo"),
                Fuel = s["Fuel"].ToString(),
                FuelCEN = s["FuelCEN"].ToString(),
                PlantCEN = s["PlantCEN"].ToString(),
                ActionType = CalculationConstants.HD

            }).ToList();

            return actions;
        }

    }
}
