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
    public class DispRealRepository: SQLRepository
    {
        public DispRealRepository(string tenant) : base(tenant) { }

        public List<GenUnit> GetUnits(DateTime fechaFinMes, List<string> codigoActivo)
        {
            List<GenUnit> units = new List<GenUnit>();

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
						select  GenerationGroupId, GenerationGroupName, GroupType,
                        UnitId, UnitName, StartDate
						from Units 
                        where StartDate is not null
                        ";

            if (codigoActivo.Count() > 0)
                sql = sql + " and UnitId in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " )";

           
            string valores = "Valores: ";
            valores = valores + " - " + string.Join("|",codigoActivo) + "/n";
            valores = valores + " - " + fechaFinMes + "/n";

            try
            {
                DbCommand command = db.GetSqlStringCommand(sql);
                command.CommandTimeout = TimeoutTransaction;
                db.AddInParameter(command, "@pFechaFinMes", SqlDbType.DateTime, fechaFinMes);

                var reader = db.ExecuteReader(command);
               /* reader.GetSchemaTable().Constraints.Clear();
                                
                DataTable dt = new DataTable();
                dt.Columns.Add("GenerationGroupId", typeof(string));
                dt.Columns.Add("GenerationGroupName", typeof(string));
                dt.Columns.Add("GroupType", typeof(string));
                dt.Columns.Add("UnitId", typeof(string));
                dt.Columns.Add("UnitName", typeof(string));
                dt.Columns.Add("StartDate", typeof(DateTime));

                dt.Load(reader);*/
              
               /* for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int j = 0; j < dt.Columns.Count-1; j++)
                    {
                        valores = valores + "--" + (dt.Rows[i][j] != DBNull.Value ? dt.Rows[i][j].ToString() : null);
                        //var unit = new GenUnit();
                        //unit.GenerationGroupId = dt.Rows[j][i] != DBNull.Value ? dt.Rows[j][i].ToString() : null;
                    }
                    valores = valores + "/n";
                }*/
                
                //var rows = dt.AsEnumerable();
                //valores = valores + " fila 1 : " + dt.Rows[1][1] + "/n"; 

                valores = valores + " - Antes del if -";
                if (reader != null)
                {
                    valores = valores + " - despues del if -";
                    
                    while (reader.Read())
                    { 
                        valores = valores + " - dentro del while -";
                        valores = valores + " - reader.GetValue(0) -" + reader.GetValue(0) ;
                        
                       
                        var n = new GenUnit();
                        try
                        {
                            n.Id = Guid.NewGuid();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Error en campo Id" + ex.Message);
                        }

                        try
                        {
                            n.GenerationGroupId = reader.IsDBNull(0) ? null : reader.GetValue(0).ToString();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Error en campo GenerationGroupId -" + ex.Message);
                        }

                        try
                        {
                            n.GenerationGroupName = reader.IsDBNull(1) ? null : reader.GetValue(1).ToString();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Error en campo GenerationGroupName - " + ex.Message);
                        }

                        try
                        {
                            n.GroupType = reader.IsDBNull(2) ? null : reader.GetValue(2).ToString();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Error en campo GroupType - " + ex.Message);
                        }

                        try
                        {
                            n.UnitId = reader.IsDBNull(3) ? null : reader.GetValue(3).ToString();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Error en campo UnitId - " + ex.Message);
                        }

                        try
                        {
                            n.UnitName = reader.IsDBNull(4) ? null : reader.GetValue(4).ToString();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Error en campo UnitName - " + ex.Message);
                        }
                     
                        /* try
                         {
                             n.CEN_Unit = reader.IsDBNull(7) ? null : reader.GetValue(7).ToString();
                         }
                         catch (Exception ex)
                         {
                             throw new Exception("Error en campo CEN_Unit- "  + ex.Message);
                         }

                         try
                         {
                             n.Fuel_Unit = reader.IsDBNull(8) ? null : reader.GetValue(8).ToString();
                         }
                         catch (Exception ex)
                         {
                             throw new Exception("Error en campo Fuel_Unit - " + ex.Message);
                         }

                         try
                         {
                             n.CEN_Group = reader.IsDBNull(9) ? null : reader.GetValue(9).ToString();
                         }
                         catch (Exception ex)
                         {
                             throw new Exception("Error en campo CEN_Group - "  + ex.Message);
                         }
                        */
                        try
                        {
                            n.OperationStartDate = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Error en campo OperationStartDate" + ex.Message);
                        }
                        units.Add(n);
                    }
                }
            }
            catch (Exception ex)
            {

                throw new Exception("Error ejecutando consulta - " + valores + ex.Message);
            }

            

            return units;
        }

        public List<Action> GetActions(DateTime fechaInicioMes, DateTime fechaFinMes, List<string> codigoActivo)
        {
            List<Action> actions = new List<Action>();

            SqlDatabase db = new SqlDatabase(ManeuverOriginConnectionString);

            var sql = @"WITH 
                            Tipos AS( 
                                SELECT tv.id, tv.Value valor, t.Name AS tipo FROM [dbo].[TypeValues] Tv 
                                INNER JOIN[dbo].[Types] T on tv.TypeId= t.Id 
                            ), 
                            EventList AS( 
                                    SELECT Id, ElementId, ConsignmentId, NewAvailability, causeoperationalid, ActionTypeId, InstructionTime, OccurrenceTime , ConfirmationTime, ScheduledStartDate, fuelName as fuel, CauseOrigin,FuelCEN
                                    FROM Actions a 
                                    WHERE (a.OccurrenceTime BETWEEN @pFechaInicioMes AND @pFechaFinMes) 
                                    UNION 
                                        SELECT a.Id,a.ElementId,a.ConsignmentId,a.NewAvailability,a.causeoperationalid, a.ActionTypeId,InstructionTime,a.OccurrenceTime,ConfirmationTime,ScheduledStartDate, fuelName as fuel, CauseOrigin,FuelCEN
                                    FROM Actions a 
                                    INNER JOIN( 
                                            SELECT ElementId, MAX(OccurrenceTime) AS LastEvent 
                                            FROM Actions 
                                            WHERE OccurrenceTime < @pFechaInicioMes
                                            GROUP BY ElementId 
                                            )AS EventF on EventF.ElementId = a.ElementId AND 
                                            EventF.LastEvent = a.OccurrenceTime 
                            ), 
                            EventListValues AS( 
                                        SELECT e.Id, e.ElementId, e.ConsignmentId, e.NewAvailability, e.causeoperationalid, e.ActionTypeId, e.InstructionTime, e.OccurrenceTime , e.ConfirmationTime, e.ScheduledStartDate 
                                        , causa.valor AS [type], causa.tipo ActionName, tipoMovimiento.valor AS movimiento , tipoMovimiento.tipo tipoMovimiento ,fuel, CauseOrigin,FuelCEN
                                        FROM EventList e 
                                        LEFT JOIN Tipos causa on e.CauseOperationalId = causa.Id 
                                        LEFT JOIN Tipos tipoMovimiento on e.ActionTypeId = tipoMovimiento.Id 
                            ), 
                            EventsEnd AS 
                            ( 
                                SELECT id AS eventId, InstructionTime, OccurrenceTime, ConfirmationTime, ElementId, ActionName, [type], ConsignmentId, NewAvailability, 
                                movimiento AS movement, tipomovimiento AS typeMovement,fuel, CauseOrigin ,FuelCEN
                                , (SELECT top 1 OccurrenceTime 
                                            FROM EventListValues WHERE OccurrenceTime > ev.OccurrenceTime 
                                        and ev.ElementId = ElementId ORDER BY OccurrenceTime) EndOccurrenceTime 
                                        ,ScheduledStartDate 
                                ,(SELECT top 1 ScheduledStartDate 
                                        FROM EventListValues WHERE ScheduledStartDate > ev.ScheduledStartDate 
                                        and ev.ElementId = ElementId ORDER BY ScheduledStartDate) ScheduledEndDate 
                                    FROM EventListValues ev 
                            ),  
                            Result as ( 
                                SELECT eventId, InstructionTime, OccurrenceTime, ConfirmationTime, ElementId, ActionName,[type], ConsignmentId, NewAvailability, 
                                    movement, typeMovement, ISNULL(EndOccurrenceTime, @pFechaFinMes) EndOccurrenceTime, ScheduledStartDate, ISNULL(ScheduledEndDate, @pFechaFinMes) ScheduledEndDate 
                                    ,CASE WHEN ev.OccurrenceTime < @pFechaInicioMes THEN @pFechaInicioMes ELSE ev.OccurrenceTime END AS ValidFrom, 
                                    CASE WHEN isnull(EndOccurrenceTime, @pFechaFinMes) >= @pFechaFinMes THEN @pFechaFinMes ELSE EndOccurrenceTime END AS ValidTo 
									,fuel, CauseOrigin,FuelCEN
                                FROM EventsEnd ev 

                            )
                            SELECT eventId,InstructionTime,OccurrenceTime,ConfirmationTime,ElementId,ActionName,[type],ConsignmentId,NewAvailability, 
                                    movement, typeMovement, EndOccurrenceTime ,ScheduledStartDate,ScheduledEndDate 
                                    ,ValidFrom,ValidTo ,fuel, CauseOrigin,FuelCEN
                            FROM Result 
                            WHERE  movement in (SELECT valor FROM tipos 
                                            WHERE valor in ('Cambio de disponibilidad', 'Finaliza cambio de disponibilidad', 
                                                            'Disponible', 'Indisponible'))";

            if (codigoActivo.Count() > 0)
                sql = sql + " AND ElementId in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " )";

            DbCommand command = db.GetSqlStringCommand(sql);
            command.CommandTimeout = TimeoutTransaction;
            db.AddInParameter(command, "@pFechaInicioMes", SqlDbType.DateTime, fechaInicioMes);
            db.AddInParameter(command, "@pFechaFinMes", SqlDbType.DateTime, fechaFinMes);

            var reader = db.ExecuteReader(command);

            while (reader.Read())
            {
                actions.Add(new Action()
                {
                    Id = reader.GetGuid(0),
                    InstructionTime = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1),
                    OccurrenceTime = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
                    ConfirmationTime = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                    ElementId = reader.GetString(4),
                    ActionName = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Type = reader.IsDBNull(6) ? null : reader.GetString(6),
                    ConsignmentId = reader.IsDBNull(7) ? null : reader.GetString(7),
                    NewAvailability = reader.IsDBNull(8) ? (decimal?)null : reader.GetDecimal(8),
                    Movement = reader.IsDBNull(9) ? null : reader.GetString(9),
                    TypeMovement = reader.IsDBNull(10) ? null : reader.GetString(10),
                    EndOccurrenceTime = reader.IsDBNull(11) ? (DateTime?)null : reader.GetDateTime(11),
                    ScheduledStartDate = reader.IsDBNull(12) ? (DateTime?)null : reader.GetDateTime(12),
                    ScheduledEndDate = reader.IsDBNull(13) ? (DateTime?)null : reader.GetDateTime(13),
                    ValidFrom = reader.IsDBNull(14) ? (DateTime?)null : reader.GetDateTime(14),
                    ValidTo = reader.IsDBNull(15) ? (DateTime?)null : reader.GetDateTime(15),
                    Fuel = reader.IsDBNull(16) ? null : reader.GetString(16),
                    CauseOrigin = reader.IsDBNull(17) ? null : reader.GetString(17),
                    FuelCEN= reader.IsDBNull(18) ? null : reader.GetString(18),
                    ActionType = CalculationConstants.DispRealP,
                });
            }

            return actions;
        }

        public List<Consignment> GetConsignments(DateTime fechaInicioMes, DateTime fechaFinMes, List<string> codigoActivo)
        {
            List<Consignment> consignments = new List<Consignment>();

            SqlDatabase db = new SqlDatabase(ConsignmentOriginConnectionString);

            var sql = "  SELECT Consecutive,ElementId,origen.MaintenanceOriginName, " +
                      "  NULL CauseName,	ScheduledStartDate,ScheduledEndDate , " +
                      "  RealStartDate,RealEndDate, NULL penalDate, NULL ActivePenal " +
                      "  FROM [dbo].[Consignments] c " +
                      "  INNER JOIN " +
                      " (SELECT [value] as MaintenanceOriginName, id from [dbo].[TypeValues] " +
                      "     WHERE typeid IN ( " +
                      "         SELECT Id FROM [dbo].[Types] " +
                      "         WHERE name LIKE '%Origen Mantenimiento%' " +
                      "     AND value IN('MtoMayor') " +
                      "     )) origen " +
                      " ON origen.Id = c.MaintenanceOriginId ";
            //   "  AND ElementId like @pCodActivo ";

            if (codigoActivo.Count() > 0)
                sql = sql + " AND ElementId in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " )";


            DbCommand command = db.GetSqlStringCommand(sql);
            command.CommandTimeout = TimeoutTransaction;

            //db.AddInParameter(command, "@pFechaInicioMes", SqlDbType.DateTime, fechaInicioMes);
            //db.AddInParameter(command, "@pFechaFinMes", SqlDbType.DateTime, fechaFinMes);

            //Si CodActivo es Nulo lo convertimos en % para traer todos los datos
            //codigoActivo = String.IsNullOrEmpty(codigoActivo) ? "%" : codigoActivo;
            //db.AddInParameter(command, "@pCodActivo", SqlDbType.NVarChar, codigoActivo);

            var reader = db.ExecuteReader(command);

            while (reader.Read())
            {
                consignments.Add(new Consignment()
                {
                    Consecutive = reader.GetString(0),
                    ElementId = reader.GetString(1),
                    MaintenanceOriginName = reader.GetString(2),
                    CauseName = reader.IsDBNull(3) ? null : reader.GetString(3),
                    ScheduledStartDate = reader.GetDateTime(4),
                    ScheduledEndDate = reader.GetDateTime(5),
                    RealStartDate = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                    RealEndDate = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                    PenalDate = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8),
                    ActivePenal = reader.IsDBNull(9) ? (Boolean?)null : reader.GetBoolean(9),
                    //TODO: IndicatorType = Constants.IndicatorConstants.STR ??
                });
            }

            return consignments;
        }
    }
}
