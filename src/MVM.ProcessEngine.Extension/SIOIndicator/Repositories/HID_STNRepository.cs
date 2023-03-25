using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using MVM.ProcessEngine.Extension.SIOIndicator.Constants;
using MVM.ProcessEngine.Extension.SIOIndicator.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Action = MVM.ProcessEngine.Extension.SIOIndicator.Domain.Action;

namespace MVM.ProcessEngine.Extension.SIOIndicator.Repositories
{
    public class HID_STNRepository: SQLRepository
    {
        public HID_STNRepository(string tenant) : base(tenant) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fechaFinMes"></param>
        /// <param name="codigoActivo"></param>
        /// <returns></returns>
        public List<Element> GetElements(DateTime fechaFinMes, List<string> codigoActivo)
        {
            List<Element> elementList = new List<Element>();

            SqlDatabase db = new SqlDatabase(ElementOriginConnectionString);
            var sql = @"with ElementList as (
	                        SELECT t0.objID, 
		                        b51.objID1 as SubSistema, 
		                        CASE
			                        WHEN LineasT.LinT is not null THEN 'T_' + b51.objID1 
			                        ELSE b51.objID2 
		                        END AS Agregador,
		                        CASE  
			                        WHEN Barras.objid1 is not null THEN Barras.objid2 
			                        WHEN LineasT.LinT is not null THEN LineasT.LinT 
			                        ELSE Isnull(b51.objID2, t0.objID) 
		                        END AS Elemento,
		                        CASE 
			                        WHEN LineasT.LinT is not null THEN 'Lineas en T' 
			                        WHEN Barras.objid1 is not null THEN 'Barras' 
			                        ELSE Null 
		                        END AS Tipo,
		                        Null UC,
		                        CASE 
			                        WHEN t1.subconjunto is null or t1.subconjunto = 'NoCompensa' then 'NoCompensa' 
			                        ELSE 'Compensa' 
		                        END AS Compensa, 
		                        CASE 
			                        WHEN t1.subconjunto = 'CorteCentral' then CAST(1 AS BIT) 
		                        END AS IsCenterCut

	                        FROM vActivo AS t0
	                        INNER JOIN dbo.MaestroCol AS t1 on (t0.objID = t1.objid) 
	                        LEFT JOIN Birrelacion b51 on b51.objID = 'Bir0051' and t0.objID = objID2

	                        left join(  
		                        select b6.objID2 as LinT 
		                        from birrelacion b6  
		                        where objid = 'Bir0051' and b6.objID2 like 'Lin%' 
		                        AND(@pFechaFinMes >= b6.fechaIni) AND(b6.fechaFin IS NULL OR b6.fechaFin > @pFechaFinMes)  
		                        and objid1 in ( 
			                        SELECT objid1 
			                        FROM birrelacion b 
			                        where objid = 'Bir0051' and objid1 like 'Sbs%' and objID2 like 'Lin%' 
			                        AND(@pFechaFinMes >= b.fechaIni) AND(b.fechaFin IS NULL OR b.fechaFin > @pFechaFinMes) 
			                        group by objid1 
			                        having count(objID2) > 1 
		                        )
	                        ) LineasT on LineasT.LinT = b51.objID2 

	                        left join birrelacion Barras on Barras.objID1 = b51.objID2 and Barras.objid = 'Bir0037' 
			                        AND(@pFechaFinMes >= Barras.fechaIni) AND(Barras.fechaFin IS NULL OR Barras.fechaFin > @pFechaFinMes) 

	                        WHERE  (t1.colid = 'Col0091')
	                        and t1.subconjunto != 'NoCompensa'
	                        and ( @pFechaFinMes >= t1.fechaIni) AND(t1.fechaFin IS NULL OR t1.fechaFin > @pFechaFinMes )
	                        and ( @pFechaFinMes >= t0.fechaIni) AND(t0.fechaFin IS NULL OR t0.fechaFin > @pFechaFinMes )
                        )
                        --select * from ElementList
                        , MHAI as 
                        ( 
                            SELECT b.objID1 AS 'subsistema', c.valor AS 'MHAI' , Unidades 
                            FROM Birrelacion b 
                            INNER JOIN dbo.Constante c ON c.defID = b.objID2 
                            WHERE b.objID = 'Bir0251' 
                                AND(@pFechaFinMes >= b.fechaIni) AND(b.fechaFin IS NULL OR b.fechaFin > @pFechaFinMes) 
                                AND(@pFechaFinMes >= c.fechaIni) AND(c.fechaFin IS NULL OR c.fechaFin > @pFechaFinMes) 
                        )
                        , TiempoManiobra as 
                        ( 
                            SELECT b.objID1 AS 'elemento', c.valor AS 'TiempoManiobra',Unidades 
                            FROM Birrelacion b 
                            INNER JOIN dbo.Constante c ON c.defID = b.objID2 
                            WHERE b.objID = 'Bir0252' 
                            AND(@pFechaFinMes >= b.fechaIni) AND(b.fechaFin IS NULL OR b.fechaFin > @pFechaFinMes) 
                            AND(@pFechaFinMes >= c.fechaIni) AND(c.fechaFin IS NULL OR c.fechaFin > @pFechaFinMes) 
                        )
                        , encapsulado as 
                        ( 
	                        select e.elemento, CASE WHEN encapsulado.elemento IS NULL THEN 'Def0111' ELSE 'Def0236' end definicion, c.valor,c.unidades,C.fechaIni,C.fechaFin 
	                        from ElementList e 
	                        left join( 
                                SELECT distinct M.objID AS 'elemento',  'encapsulado' as encapsulado 
                                FROM MaestroCol M 
                                WHERE colid = 'Col0175' and subconjunto = 'STN' 
			                        AND(@pFechaFinMes >= M.fechaIni) AND(M.fechaFin IS NULL OR M.fechaFin > @pFechaFinMes) 
	                        ) encapsulado on e.elemento = encapsulado.elemento 
                            inner join constante c ON c.defid = CASE WHEN encapsulado.elemento IS NULL THEN 'Def0111' ELSE 'Def0236' end 
                                AND(@pFechaFinMes >= c.fechaIni) AND(c.fechaFin IS NULL OR c.fechaFin > @pFechaFinMes) 
                        )
                        select distinct isnull(e.Subsistema, e.Elemento) as SubsystemId, isnull(agregador, e.Elemento) as AggregatorId, e.Elemento as ElementId, 
	                        isnull(e.tipo, 'SinConf') as ElementType, isnull(uc, 'NA') as UCtype, compensa as Compensate, MHAI 
	                        , M.unidades as MHAIAUnit  
	                        ,t.TiempoManiobra as maneuverTime 
	                        ,t.unidades as maneuverUnit 
	                        ,encapsulado.valor as MaintenanceTime 
	                        ,encapsulado.unidades as MaintenanceUnit 
	                        ,encapsulado.fechaIni AS MaintenanceStartDate 
	                        ,encapsulado.fechaFin AS MaintenanceEndDate 
	                        ,IsCenterCut 
	                        ,l.longitud as SegmentLength 
	                        ,isnull(ms.ultNombre, ee.ultNombre) as subsystemName 
	                        ,isnull(ag.ultNombre, ee.ultNombre) as aggregatorName 
	                        ,ee.ultNombre as ElementName
                        from ElementList e 
                        left join MaestroObj ms on e.Subsistema = ms.objID 
                        left join MaestroObj ag on e.agregador = ag.objID 
                        left join MaestroObj ee on e.Elemento = ee.objID 
                        left JOIN MHAI M ON M.subsistema = e.Elemento 
                        Left JOIN TiempoManiobra T ON t.elemento = e.Elemento 
                        left join encapsulado encapsulado on e.Elemento = encapsulado.Elemento 
                        left join linea l on l.objID=e.Elemento AND(@pFechaFinMes >= l.fechaIni) AND(l.fechaFin IS NULL OR l.fechaFin > @pFechaFinMes)";

            if (codigoActivo.Count() > 0)
                sql = sql + " where e.elemento in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " )";

            DbCommand command = db.GetSqlStringCommand(sql);
            command.CommandTimeout = TimeoutTransaction;
            db.AddInParameter(command, "@pFechaFinMes", SqlDbType.DateTime, fechaFinMes);

            var reader = db.ExecuteReader(command);

            while (reader.Read())
            {
                elementList.Add(new Element()
                {
                    SubsystemId = reader.GetString(0),
                    AggregatorId = reader.GetString(1),
                    ElementId = reader.GetString(2),
                    ElementType = reader.GetString(3),
                    UCtype = reader.GetString(4),
                    Compensate = reader.GetString(5),
                    MHAI = reader.IsDBNull(6) ? (double?)null : reader.GetDouble(6),
                    MHAIAUnit = reader.IsDBNull(7) ? null : reader.GetString(7),
                    ManeuverTime = reader.IsDBNull(8) ? (double?)null : reader.GetDouble(8),
                    ManeuverUnit = reader.IsDBNull(9) ? null : reader.GetString(9),
                    MaintenanceTime = reader.IsDBNull(10) ? (double?)null : reader.GetDouble(10),
                    MaintenanceUnit = reader.IsDBNull(11) ? null : reader.GetString(11),
                    MaintenanceStartDate = reader.GetDateTime(12),
                    MaintenanceEndDate = reader.GetDateTime(13),
                    IsCenterCut = reader.IsDBNull(14) ? (bool?)null : reader.GetBoolean(14),
                    SegmentLength = reader.IsDBNull(15) ? (double?)null : reader.GetDouble(15),
                    SubsystemName = reader.IsDBNull(16) ? null : reader.GetString(16),
                    AggregatorName = reader.IsDBNull(17) ? null : reader.GetString(17),
                    ElementName = reader.IsDBNull(18) ? null : reader.GetString(18),
                    IndicatorType = IndicatorConstants.STN
                });
                ;
            }

            return elementList;
        }

        public List<Element> GetElementsByType(string indicatorType)
        {
            List<Element> elementList = new List<Element>();

            SqlDatabase db = new SqlDatabase(DestinationConnectionString);
            var sql = @"select SubsystemId,AggregatorId,ElementId, 
						ElementType,UCtype,Compensate, MHAI, MHAIAUnit  
						,maneuverTime ,maneuverUnit,MaintenanceTime 
						,MaintenanceUnit ,MaintenanceStartDate ,MaintenanceEndDate 
						,IsCenterCut,SegmentLength,subsystemName,aggregatorName ,ElementName 
						from [dbo].[Elements]
						where IndicatorType=@IndicatorType
                      ";

            DbCommand command = db.GetSqlStringCommand(sql);
            command.CommandTimeout = TimeoutTransaction;
            db.AddInParameter(command, "@IndicatorType", SqlDbType.VarChar, indicatorType);

            var reader = db.ExecuteReader(command);

            while (reader.Read())
            {
                elementList.Add(new Element()
                {
                    SubsystemId = reader.GetString(0),
                    AggregatorId = reader.GetString(1),
                    ElementId = reader.GetString(2),
                    ElementType = reader.GetString(3),
                    UCtype = reader.GetString(4),
                    //Compensate = reader.GetString(5),
                    //MHAI = reader.IsDBNull(6) ? (double?)null : reader.GetDouble(6),
                    //MHAIAUnit = reader.IsDBNull(7) ? null : reader.GetString(7),
                    //ManeuverTime = reader.IsDBNull(8) ? (double?)null : reader.GetDouble(8),
                    //ManeuverUnit = reader.IsDBNull(9) ? null : reader.GetString(9),
                    //MaintenanceTime = reader.IsDBNull(10) ? (double?)null : reader.GetDouble(10),
                    //MaintenanceUnit = reader.IsDBNull(11) ? null : reader.GetString(11),
                    ///MaintenanceStartDate = reader.GetDateTime(12),
                    //MaintenanceEndDate = reader.GetDateTime(13),
                    //IsCenterCut = reader.IsDBNull(14) ? (bool?)null : reader.GetBoolean(14),
                    //SegmentLength = reader.IsDBNull(15) ? (double?)null : reader.GetDouble(15),
                    SubsystemName = reader.IsDBNull(16) ? null : reader.GetString(16),
                    AggregatorName = reader.IsDBNull(17) ? null : reader.GetString(17),
                    //ElementName = reader.IsDBNull(18) ? null : reader.GetString(18)

                });
                ;
            }

            return elementList;
        }

        public List<Action> GetActions(DateTime fechaFinMes, List<string> codigoActivo)
        {
            List<Action> actions = new List<Action>();

            SqlDatabase db = new SqlDatabase(ManeuverOriginConnectionString);

            var sql = @"WITH mov AS (
                            SELECT m
                            FROM (SELECT 'Cambio de disponibilidad' M
                                    UNION SELECT 'Finaliza cambio de disponibilidad' M
                                    UNION SELECT 'Disponible' M
                                    UNION SELECT 'Indisponible' M) MM
                            )
	                        /*select * from mov*/
                            ,Tipos AS( 
		                        SELECT tv.id, tv.Value valor, t.Name AS tipo FROM [dbo].[TypeValues] Tv 
		                        INNER JOIN[dbo].[Types] T on tv.TypeId= t.Id 
                            )
	                        /*select * from tipos  */
	                        , 
                            EventList AS( 
			                        /*Eventos del mes a calcular*/
                                SELECT Id, ElementId, ConsignmentId, NewAvailability, causeoperationalid, ActionTypeId, InstructionTime, OccurrenceTime , ConfirmationTime, ScheduledStartDate, EspElementId, CauseOrigin, ElementCausingId,ElementCompanyShortName
                                FROM Actions a 
                                WHERE (a.OccurrenceTime BETWEEN DATEADD(month,-1,DATEADD(s, 1, @pFechaFinMes))  AND @pFechaFinMes) 
                                /*ultimo evento anterior a la fecha de calculo*/
		                        UNION 

                                SELECT a.Id,a.ElementId,a.ConsignmentId,a.NewAvailability,a.causeoperationalid, a.ActionTypeId,InstructionTime,a.OccurrenceTime,ConfirmationTime,ScheduledStartDate, EspElementId, CauseOrigin, ElementCausingId,ElementCompanyShortName
                                FROM Actions a 
                                INNER JOIN( 
					                        SELECT ElementId, MAX(OccurrenceTime) AS LastEvent 
					                        FROM Actions 
					                        WHERE OccurrenceTime<DATEADD(month,-1,DATEADD(s, 1, @pFechaFinMes))  
					                        GROUP BY ElementId 
                                        ) AS EventF on EventF.ElementId = a.ElementId AND EventF.LastEvent = a.OccurrenceTime 
                            )
	                        /*select * from EventList  */
	                        ,EventListValues AS( 
		                        SELECT e.Id, e.ElementId, e.ConsignmentId, e.NewAvailability, e.causeoperationalid, e.ActionTypeId, e.InstructionTime, e.OccurrenceTime , e.ConfirmationTime, 
			                        e.ScheduledStartDate ,causa.valor AS [type], causa.tipo ActionName, tipoMovimiento.valor AS movimiento , tipoMovimiento.tipo tipoMovimiento, EspElementId, CauseOrigin, ElementCausingId,ElementCompanyShortName
                                FROM EventList e 
                                LEFT JOIN Tipos causa on e.CauseOperationalId = causa.Id 
                                LEFT JOIN Tipos tipoMovimiento on e.ActionTypeId = tipoMovimiento.Id
                            )
	                        /*select * from EventListValues  */
	                        ,EventsEnd AS 
                            ( 
		                        /*obtener fecha fin de ocurrencia*/
                                SELECT id AS eventId, InstructionTime, OccurrenceTime, ConfirmationTime, ElementId, ActionName, [type], ConsignmentId, NewAvailability, 
			                        movimiento AS movement, tipomovimiento AS typeMovement , 
			                        (SELECT top 1 OccurrenceTime 
			                            FROM EventListValues 
			                            WHERE OccurrenceTime > ev.OccurrenceTime and ev.ElementId = ElementId 
			                            ORDER BY OccurrenceTime) EndOccurrenceTime,
			                        ScheduledStartDate,
			                        null ScheduledEndDate, EspElementId, CauseOrigin, ElementCausingId
									,ElementCompanyShortName
                                FROM EventListValues ev ";

            if (codigoActivo.Count() > 0)
                sql = sql + " WHERE ElementId in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " )";

            sql += @")
	                        /*select * from EventsEnd  */
	                        ,Result as ( 
		                        /*calculo de fechas de inicio y fin de acuerdo al mes de calculo*/
                                SELECT eventId, InstructionTime, OccurrenceTime, ConfirmationTime, ElementId, ActionName,[type], ConsignmentId, NewAvailability, movement, typeMovement, 
			                        ISNULL(EndOccurrenceTime, @pFechaFinMes) EndOccurrenceTime, ScheduledStartDate, 
			                        ISNULL(EndOccurrenceTime, @pFechaFinMes) ScheduledEndDate,
			                        CASE WHEN ev.OccurrenceTime < DATEADD(month, -1, DATEADD(s, 1, @pFechaFinMes)) THEN DATEADD(month,-1,DATEADD(s, 1, @pFechaFinMes)) ELSE ev.OccurrenceTime END AS ValidFrom, 
			                        CASE WHEN isnull(EndOccurrenceTime, @pFechaFinMes) >= @pFechaFinMes THEN @pFechaFinMes ELSE EndOccurrenceTime END AS ValidTo, EspElementId, CauseOrigin, ElementCausingId
									,ElementCompanyShortName
                                FROM EventsEnd ev
								)
		                /*select * from Result  */
	                    /*1.1.	Eventos asociados a cambios de disponibilidad*/
                        SELECT eventId, InstructionTime, OccurrenceTime, ConfirmationTime, ElementId, ActionName, [type], ConsignmentId, NewAvailability, 
			                movement, typeMovement, EndOccurrenceTime ,ScheduledStartDate, ScheduledEndDate, ValidFrom, ValidTo, CauseOrigin, ElementCausingId
							,ElementCompanyShortName
                        FROM Result 
                        WHERE movement in (SELECT m FROM mov) AND type not in ('Mantenimiento') and not (movement in ('Disponible', 'Indisponible') AND CauseOrigin = 'Actuación ESP')

                        UNION 
	                    /*1.2.	Eventos asociados a retrasos en los tiempos de maniobra*/
                        SELECT eventId, InstructionTime, OccurrenceTime, ConfirmationTime, ElementId, ActionName, [type], ConsignmentId, NewAvailability, 
			                movement, typeMovement, EndOccurrenceTime, ScheduledStartDate, ScheduledEndDate, ValidFrom, ValidTo, CauseOrigin, ElementCausingId
						,ElementCompanyShortName
                        FROM Result 
                        WHERE((movement in ('Abrir', 'Cerrar') AND typeMovement in ('Acciones Cambio Operativo')) 
			                or (movement in ('Bajar TAP', 'Subir TAP') AND typeMovement in ('Acciones Control PVQ'))) 
			                /* AND DATEDIFF(MINUTE,[InstructionTime] ,[OccurrenceTime] ) > 0 */ and not (movement in ('Disponible', 'Indisponible') AND CauseOrigin = 'Actuación ESP')

                        UNION 
	                    /*1.4.	Ajuste de tiempos por horas no utilizadas de mantenimiento */
                        SELECT eventId, InstructionTime, OccurrenceTime, ConfirmationTime, ElementId, ActionName, [type], ConsignmentId, NewAvailability, 
			                movement, typeMovement, EndOccurrenceTime, ScheduledStartDate, ScheduledEndDate, ScheduledStartDate AS ValidFrom, ValidFrom AS ValidTo, CauseOrigin, ElementCausingId
                        ,ElementCompanyShortName
						FROM Result 
                        WHERE movement in (SELECT m FROM mov) and type in ('Mantenimiento') and not (movement in ('Disponible', 'Indisponible') AND CauseOrigin = 'Actuación ESP')
			                and(
				                EXISTS(
					                SELECT EventId 
                                    FROM Result 
                                    WHERE movement in (SELECT m FROM mov)
                                        and type in ('Mantenimiento') 					
                                        and 
					                    (
							                /*prog antes de la ventana -NoUtilizaMante*/
							                (ScheduledStartDate < ValidFrom and ValidFrom > DATEADD(month, -1, DATEADD(s, 1, @pFechaFinMes)))
							                or
							                /*prog despues de la ventana -NoUtilizaMante*/
							                (ScheduledEndDate > ValidTo and ValidTo < @pFechaFinMes )
							                /*terminó despues de la programacion -NoProgramadoMant*/
							                or  
							                (ValidTo > ScheduledEndDate and ValidTo < @pFechaFinMes)
					                    )
                                    )     
                        )

                        UNION 

	                    SELECT eventId, InstructionTime, OccurrenceTime, ConfirmationTime,
		                    CASE WHEN NOT EspElementId IS NULL AND NOT EspElementId = '' 
			                    THEN EspElementId ELSE ElementId 
		                    END  ElementeId, 
		                    ActionName, [type], ConsignmentId, NewAvailability, 
		                    movement, typeMovement, EndOccurrenceTime ,ScheduledStartDate, ScheduledEndDate, ValidFrom, ValidTo, CauseOrigin, ElementCausingId
                        ,ElementCompanyShortName
						FROM Result 
                        WHERE movement in ('Disponible', 'Indisponible') AND CauseOrigin = 'Actuación ESP'

                        UNION
                        /*Carga HIR - no operativos*/
						select eventId, InstructionTime, OccurrenceTime, ConfirmationTime, ElementId, ActionName, [type], ConsignmentId, NewAvailability, 
			                movement, typeMovement, EndOccurrenceTime, ScheduledStartDate, ScheduledEndDate, ScheduledStartDate AS ValidFrom, ValidFrom AS ValidTo, CauseOrigin , ElementCausingId
						,ElementCompanyShortName
						from Result
						where movement in ('No Operativo', 'Finaliza No Operativo');";



            DbCommand command = db.GetSqlStringCommand(sql);
            command.CommandTimeout = TimeoutTransaction;
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
                    CauseOrigin = reader.IsDBNull(16) ? null : reader.GetString(16),
                    ElementCausingId = reader.IsDBNull(17) ? null : reader.GetString(17),
                    ElementCompanyShortName= reader.IsDBNull(18) ? null : reader.GetString(18),
                    ActionType = CalculationConstants.HID_STN
                });
            }

            return actions;
        }

        public List<Action> GetOccurrencesESP(DateTime fechaFinMes, List<string> codigoActivo)
        {
            List<Action> actions = new List<Action>();

            SqlDatabase db = new SqlDatabase(OccurrenceOriginConnectionString);

            var sql = @"WITH Tipos AS( 
	                        SELECT tv.id, tv.Value valor, t.Name AS tipo FROM [dbo].[TypeValues] Tv 
	                        INNER JOIN[dbo].[Types] T on tv.TypeId= t.Id 
                        ), 
                        EventList AS( 
                            SELECT Id, EspElementId ElementId, 
		                        Null ConsignmentId, 0 NewAvailability, CauseId, TypeId, Null InstructionTime, StartDate OccurrenceTime, EndDate EndOccurrenceTime,
		                        Null ConfirmationTime, Null ScheduledStartDate 
                            FROM Occurrences a 
                            WHERE (a.StartDate BETWEEN DATEADD(month,-1,DATEADD(s, 1, @pFechaFinMes))  AND @pFechaFinMes) 

	                        UNION

	                        SELECT Id, EspElementId ElementId, 
		                        Null ConsignmentId, 0 NewAvailability, CauseId, TypeId, Null InstructionTime, StartDate OccurrenceTime, EndDate EndOccurrenceTime,
		                        Null ConfirmationTime, Null ScheduledStartDate 
                            FROM Occurrences a 
                            WHERE a.StartDate < DATEADD(month,-1,DATEADD(s, 1, @pFechaFinMes))  AND a.EndDate > DATEADD(month,-1,DATEADD(s, 1, @pFechaFinMes))
                        ),
                        EventListValues AS( 
	                        SELECT e.Id, e.ElementId, e.ConsignmentId, e.NewAvailability, e.CauseId, e.TypeId, e.InstructionTime, e.OccurrenceTime, e.EndOccurrenceTime, e.ConfirmationTime, 
		                        e.ScheduledStartDate ,causa.valor AS [type], causa.tipo ActionName, tipoMovimiento.valor AS movimiento , tipoMovimiento.tipo tipoMovimiento, causa.valor CauseOrigin
                            FROM EventList e 
                            INNER JOIN Tipos tipoMovimiento on e.TypeId = tipoMovimiento.Id 
                            LEFT JOIN Tipos causa on e.CauseId = causa.Id 
	                        where causa.valor = 'Actuación ESP' and tipoMovimiento.valor = 'Suceso de DNA'
                        )
                        SELECT id eventId, InstructionTime, OccurrenceTime, ConfirmationTime, ElementId, ActionName,[type], ConsignmentId, NewAvailability, movimiento movement, tipoMovimiento typeMovement, 
	                        ISNULL(EndOccurrenceTime, @pFechaFinMes) EndOccurrenceTime, ScheduledStartDate, 
	                        ISNULL(EndOccurrenceTime, @pFechaFinMes) ScheduledEndDate,
	                        CASE WHEN ev.OccurrenceTime < DATEADD(month, -1, DATEADD(s, 1, @pFechaFinMes)) THEN DATEADD(month,-1,DATEADD(s, 1, @pFechaFinMes)) ELSE ev.OccurrenceTime END AS ValidFrom, 
	                        CASE WHEN isnull(EndOccurrenceTime, @pFechaFinMes) >= @pFechaFinMes THEN @pFechaFinMes ELSE EndOccurrenceTime END AS ValidTo, CauseOrigin
                        FROM EventListValues ev ";

            if (codigoActivo.Count() > 0)
                sql = sql + " WHERE ElementId in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " )";

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
                InstructionTime = s["InstructionTime"] != DBNull.Value ? Convert.ToDateTime(s["InstructionTime"].ToString()) : (DateTime?)null,
                OccurrenceTime = s["OccurrenceTime"] != DBNull.Value ? Convert.ToDateTime(s["OccurrenceTime"].ToString()) : (DateTime?)null,
                ConfirmationTime = s["ConfirmationTime"] != DBNull.Value ? Convert.ToDateTime(s["ConfirmationTime"].ToString()) : (DateTime?)null,
                ElementId = s["ElementId"].ToString(),
                ActionName = s["ActionName"].ToString(),
                Type = s["Type"].ToString(),
                ConsignmentId = s["ConsignmentId"].ToString(),
                NewAvailability = Convert.ToDecimal(s["NewAvailability"].ToString()),
                Movement = s["Movement"].ToString(),
                TypeMovement = s["TypeMovement"].ToString(),
                EndOccurrenceTime = s["EndOccurrenceTime"] != DBNull.Value ? Convert.ToDateTime(s["EndOccurrenceTime"].ToString()) : (DateTime?)null,
                ScheduledStartDate = s["ScheduledStartDate"] != DBNull.Value ? Convert.ToDateTime(s["ScheduledStartDate"].ToString()) : (DateTime?)null,
                ScheduledEndDate = s["ScheduledEndDate"] != DBNull.Value ? Convert.ToDateTime(s["ScheduledEndDate"].ToString()) : (DateTime?)null,
                ValidFrom = s["ValidFrom"] != DBNull.Value ? Convert.ToDateTime(s["ValidFrom"].ToString()) : (DateTime?)null,
                ValidTo = s["ValidTo"] != DBNull.Value ? Convert.ToDateTime(s["ValidTo"].ToString()) : (DateTime?)null,
                CauseOrigin = s["CauseOrigin"].ToString(),
                ActionType = CalculationConstants.HID_STN
            }).ToList();

            return actions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fechaInicioMes"></param>
        /// <param name="fechaFinMes"></param>
        /// <param name="codigoActivo"></param>
        /// <returns></returns>
        public List<Consignment> GetConsignments(DateTime fechaInicioMes, DateTime fechaFinMes, List<string> codigoActivo)
        {
            List<Consignment> consignments = new List<Consignment>();

            SqlDatabase db = new SqlDatabase(ConsignmentOriginConnectionString);

            var sql = @" SELECT Consecutive,c.ElementId,origen.MaintenanceOriginName, 
                        NULL CauseName,	ScheduledStartDate,ScheduledEndDate ,
                       RealStartDate,RealEndDate, NULL penalDate, NULL ActivePenal 
                        , ElementCompanyShortName ,null EntryType,startDatePlan,EndDatePlan
                        FROM [dbo].[Consignments] c 
                        INNER JOIN 
                       (SELECT [value] as MaintenanceOriginName, id from [dbo].[TypeValues] 
                         WHERE typeid IN ( 
                               SELECT Id FROM [dbo].[Types] 
                               WHERE name LIKE '%Origen Mantenimiento%' 
                           AND value IN('MtoMayor') 
                           )
                      ) origen 
                       ON origen.Id = c.MaintenanceOriginId 
					   INNER JOIN 
                       (SELECT [value] as EntryType, id from [dbo].[TypeValues]                          
                        where [value]='Plan'
					   ) CnsType  on CnsType.Id = c.EntryTypeId 
					   cross join (
					   select min(startDate) as startDatePlan,max(enddate)as  EndDatePlan
							from (
							  SELECT startDate,EndDate
							  FROM [dbo].[Plans]
							  where name like '%STN Encapsulado%' 
							  and  getdate() between StartDate and EndDate
							  union
								SELECT startDate,EndDate
							  FROM [dbo].[Plans]
							  where name like '%STN No Encapsulado%'
							  and  getdate() between StartDate and EndDate
							  )planMtto
					   )PlanMto
                       where
					   Consecutive is not null
					   and c.ElementId is not null
					   and ( ScheduledStartDate between  startDatePlan and EndDatePlan
					         or
							 ScheduledEndDate between  startDatePlan and EndDatePlan
						   ) ";

            if (codigoActivo.Count() > 0)
                sql = sql + " and c.ElementId in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " )";



            DbCommand command = db.GetSqlStringCommand(sql);
            command.CommandTimeout = TimeoutTransaction;

            db.AddInParameter(command, "@pFechaInicioMes", SqlDbType.DateTime, fechaInicioMes);
            db.AddInParameter(command, "@pFechaFinMes", SqlDbType.DateTime, fechaFinMes);

            var reader = db.ExecuteReader(command);

            while (reader.Read())
            {
                consignments.Add(new Consignment()
                {
                    Consecutive = reader.GetString(0),
                    ElementId = reader.GetString(1),
                    MaintenanceOriginName = reader.GetString(2),
                    CauseName = reader.IsDBNull(3) ? null : reader.GetString(3),
                    ScheduledStartDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                    ScheduledEndDate = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                    ActivePenal = reader.IsDBNull(9) ? (bool?)null : reader.GetBoolean(9),
                    ElementCompanyShortName = reader.IsDBNull(10) ? null : reader.GetString(10),
                    IndicatorType = Constants.IndicatorConstants.STN
                });
            }

            return consignments;
        }

        public List<Consignment> GetConsignmentsDesv_RD(DateTime fechaInicioMes, DateTime fechaFinMes, List<string> codigoActivo,string indicatorType)
        {
            List<Consignment> consignments = new List<Consignment>();
            var elements = GetElementsByType(indicatorType);
            var ele = elements.Select(e => e.ElementId).Union(elements.Select(e => e.AggregatorId)).ToList();

            SqlDatabase db = new SqlDatabase(ConsignmentOriginConnectionString);

            var sql = @" 
                        SELECT distinct Consecutive,c.ElementId,origen.MaintenanceOriginName, 
                        NULL CauseName,  
						case when c.ScheduledStartDate < DATEADD(month,-1,DATEADD(s, 1, @pFechaFinMes)) then 
						DATEADD(month,-1,DATEADD(s, 1, @pFechaFinMes)) else c.ScheduledStartDate end
						ScheduledStartDate
						,case when c.ScheduledEndDate> @pFechaFinMes then @pFechaFinMes else c.ScheduledEndDate end ScheduledEndDate ,
                        case when aff.RealStartDate < DATEADD(month,-1,DATEADD(s, 1, @pFechaFinMes)) then 
						DATEADD(month,-1,DATEADD(s, 1, @pFechaFinMes)) else aff.RealStartDate end RealStartDate,
					    case when aff.RealEndDate> @pFechaFinMes then @pFechaFinMes else aff.RealEndDate end RealEndDate ,						
						NULL penalDate, NULL ActivePenal 
                        , c.ElementCompanyShortName , EntryType
                        ,typeAff.Affectation
                        ,(SELECT value from [dbo].[TypeValues] where id=aff.CauseRealStartDateChangeId) CauseRealStartDateChange
                        ,(SELECT value from [dbo].[TypeValues] where id=aff.CauseRealEndDateChangeId) CauseRealEndDateChange
                       FROM [dbo].[Consignments] c 
                        INNER JOIN 
                       (SELECT [value] as MaintenanceOriginName, id from [dbo].[TypeValues]                          
                        where [value]='Normal'
                       ) origen 
                       ON origen.Id = c.MaintenanceOriginId 
                       INNER JOIN 
                       (SELECT [value] as EntryType, id from [dbo].[TypeValues]                          
                        where [value] in ('Plan','Fuera Plan T')
                       ) CnsType  on CnsType.Id = c.EntryTypeId 
                       inner join [dbo].[Affectations] aff on c.Id= aff.ConsignmentId
					   and aff.elementid=c.elementid
                       inner join (
                            SELECT [value] as Affectation, id 
                            from [dbo].[TypeValues]                          
                            where (typeid ='3CFF24DB-C7DB-416A-87F3-62B8AB250D76'
                            and [value]  in ('RD'))                            
                       )typeAff on  aff.AffectationTypeId=typeAff.id
                       where   Consecutive is not null
                       and c.ElementId is not null 
					  and (aff.CauseRealStartDateChangeId in (
															SELECT id from [dbo].[TypeValues] where typeid='E507C309-EB54-4FF5-8AE4-6B0396AD533B'
															and id='F7CCB7BF-A9BB-400E-A5D0-3E9661CAA8F3'
														)
					   or aff.CauseRealEndDateChangeId in (
															SELECT id from [dbo].[TypeValues] where typeid='E507C309-EB54-4FF5-8AE4-6B0396AD533B'
															and id='F7CCB7BF-A9BB-400E-A5D0-3E9661CAA8F3'
														))


                       and (c.ScheduledStartDate BETWEEN DATEADD(month,-1,DATEADD(s, 1, @pFechaFinMes))  AND @pFechaFinMes 
                       or (c.ScheduledEndDate BETWEEN DATEADD(month,-1,DATEADD(s, 1, @pFechaFinMes))  AND @pFechaFinMes)    )
            ";
            //   "  AND ElementId like @pCodActivo ";

            if (codigoActivo.Count() > 0)
                sql = sql + " AND c.ElementId in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " )";


            DbCommand command = db.GetSqlStringCommand(sql);
            command.CommandTimeout = TimeoutTransaction;

            //db.AddInParameter(command, "@pFechaInicioMes", SqlDbType.DateTime, fechaInicioMes);
            db.AddInParameter(command, "@pFechaFinMes", SqlDbType.DateTime, fechaFinMes);

            //Si CodActivo es Nulo lo convertimos en % para traer todos los datos
            //codigoActivo = String.IsNullOrEmpty(codigoActivo) ? "%" : codigoActivo;
            //db.AddInParameter(command, "@pCodActivo", SqlDbType.NVarChar, codigoActivo);

            var reader = db.ExecuteReader(command);

            try
            {
                while (reader.Read())
                {
                    consignments.Add(new Consignment()
                    {
                        Consecutive = reader.GetString(0),
                        ElementId = reader.GetString(1),
                        MaintenanceOriginName = reader.GetString(2),
                        CauseName = reader.IsDBNull(3) ? null : reader.GetString(3),
                        ScheduledStartDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                        ScheduledEndDate = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                        RealStartDate = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                        RealEndDate = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                        PenalDate = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8),
                        ActivePenal = reader.IsDBNull(9) ? (Boolean?)null : reader.GetBoolean(9),
                        ElementCompanyShortName = reader.IsDBNull(10) ? null : reader.GetString(10),
                        EntryType = reader.IsDBNull(11) ? null : reader.GetString(11),
                        AffectationTypeId = reader.IsDBNull(12) ? null : reader.GetString(12),
                        CauseRealStartDateChange = reader.IsDBNull(13) ? null : reader.GetString(13),
                        CauseRealEndDateChange = reader.IsDBNull(14) ? null : reader.GetString(14),
                        IndicatorType = Constants.CalculationConstants.HID_STN
                    });
                }

            }
            catch (Exception ex)
            {

                throw new Exception("Error En HID_STN - desv RD consg " + ex.Message);
            }
            consignments = consignments.Where(cons => ele.Contains(cons.ElementId)).ToList();
            return consignments;
        }

    }
}
