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
    public class HID_STRRepository: SQLRepository
    {        
        public HID_STRRepository(string tenant): base(tenant) {       
            
        }

        
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
            var sql = @"With ElementList as ( 
							select b1.objID, b1.objID1 as SubSistema,  
                            CASE 
                               WHEN Anillo.ObjID1 is not null then Anillo.ObjID1 
                               WHEN LineasT.LinT is not null THEN 'T_' + b1.objID1 
							                           
                               ELSE b1.objID2 
                              END AS Agregador, 
                            CASE  
                               WHEN Anillo.ObjID1 is not null then Anillo.ObjID2 
                               WHEN Barras.objid1 is not null THEN Barras.objid2 
                               WHEN LineasT.LinT is not null THEN LineasT.LinT 
							   WHEN uc.corteCentral is not null then uc.corteCentral     
                               ELSE b1.objID2 
                              END AS Elemento, 
                            CASE 
                               WHEN Anillo.ObjID1 is not null then 'Anillo' 
                               WHEN uc.objid is not null THEN 'Int. y Medio' 
                               WHEN LineasT.LinT is not null THEN 'Lineas en T' 
                               WHEN Barras.objid1 is not null THEN 'Barras' 
                               ELSE Null 
                              END AS Tipo,  
							CASE WHEN Anillo.UC is not null then Anillo.UC
							     WHEN uc.UC is not null THEN uc.UC  
								 ELSE 'NA'
							END UC,
                            CASE 
                               WHEN Col96.subconjunto is null or Col96.subconjunto = 'NoCompensa' then 'NoCompensa' 
                               ELSE 'Compensa' 
                              END AS Compensa, 
                            CASE 
                             WHEN Col96.subconjunto = 'CorteCentral' then CAST(1 AS BIT) 
                             END AS IsCenterCut 
							,uc.subconjunto	
                            from birrelacion b1 
                            left join 
							(
								SELECT DISTINCT  cc.objid as CorteCentral, Bah.objid1 objid,
								CASE WHEN Def.objID1 = 'Def0330' THEN 'UC14' ELSE 'UC15' END UC
								,cc.subconjunto
								FROM Maestrocol cc
								INNER JOIN Birrelacion Bah ON Bah.objID = 'Bir0506' AND bah.objID2 =cc.objID
								INNER JOIN Bahia tBah ON tBah.objID = Bah.objid1
								INNER JOIN Birrelacion Unc ON Unc.objID = 'Bir0054' AND Unc.objid2 = tBah.objID
								INNER JOIN Birrelacion Def ON Def.objID = 'Bir0457' AND Def.objid2 = Unc.objID1 AND Def.objID1 IN ('Def0330','Def0331')
								WHERE cc.subconjunto = 'CorteCentral' AND cc.ColId = 'Col0096'
								AND(@pFechaFinMes >= cc.fechaIni) AND(cc.fechaFin IS NULL OR cc.fechaFin > @pFechaFinMes) 
								AND(@pFechaFinMes >= Bah.fechaIni) AND(Bah.fechaFin IS NULL OR Bah.fechaFin > @pFechaFinMes) 
								AND(@pFechaFinMes >= tBah.fechaIni) AND(tBah.fechaFin IS NULL OR tBah.fechaFin > @pFechaFinMes) 
								AND(@pFechaFinMes >= Unc.fechaIni) AND(Unc.fechaFin IS NULL OR Unc.fechaFin > @pFechaFinMes) 
								AND(@pFechaFinMes >= Def.fechaIni) AND(Def.fechaFin IS NULL OR Def.fechaFin > @pFechaFinMes) 
							)UC on UC.objID = b1.objID2 
							left join(
									select DISTINCT Inte.OBJID1, Inte.OBJID2,
									CASE WHEN Def.objID1 = 'Def0330' THEN 'UC14' ELSE 'UC15' END UC
									from birrelacion Inte
									INNER JOIN Maestrocol col ON col.objid = Inte.objid2 AND col.subconjunto <> 'NoCompensa' AND col.colid = 'Col0096'
									AND(@pFechaFinMes >= col.fechaIni) AND(col.fechaFin IS NULL OR col.fechaFin > @pFechaFinMes)
									INNER JOIN Maestrocol col1 ON col1.objid = Inte.objid1 AND col1.subconjunto <> 'NoCompensa' AND col1.colid = 'Col0096'
									AND(@pFechaFinMes >= col1.fechaIni) AND(col1.fechaFin IS NULL OR col1.fechaFin > @pFechaFinMes)
									INNER JOIN bahia bh ON Inte.objid2 = bh.objid  
									AND(@pFechaFinMes >= bh.fechaIni) AND(bh.fechaFin IS NULL OR bh.fechaFin > @pFechaFinMes)
									INNER JOIN bahia bh2 ON  Inte.objid1 = bh2.objid
									AND(@pFechaFinMes >= bh2.fechaIni) AND(bh2.fechaFin IS NULL OR bh2.fechaFin > @pFechaFinMes)
									INNER JOIN Birrelacion Unc ON Unc.objID = 'Bir0054' AND Unc.objid2 = bh2.objID
									AND(@pFechaFinMes >= Unc.fechaIni) AND(Unc.fechaFin IS NULL OR Unc.fechaFin > @pFechaFinMes)
                                    INNER JOIN Birrelacion Def ON Def.objID = 'Bir0457' AND Def.objid2 = Unc.objID1 AND Def.objID1 IN ('Def0330','Def0331')
									WHERE
									 Inte.objID ='Bir0028'
									 AND (@pFechaFinMes >= Inte.fechaIni) AND(Inte.fechaFin IS NULL OR Inte.fechaFin > @pFechaFinMes)

                            ) Anillo on Anillo.ObjID1 = b1.objID2 
                            left join birrelacion Barras on Barras.objID1 = b1.objID2 and Barras.objid = 'Bir0037' 
                            AND(@pFechaFinMes >= Barras.fechaIni) AND(Barras.fechaFin IS NULL OR Barras.fechaFin > @pFechaFinMes) 
                            left join[dbo].[MaestroCol] Col96 on Col96.objID = b1.ObjID2 and colID = 'Col0096' 
                            AND(@pFechaFinMes >= Col96.fechaIni) AND(Col96.fechaFin IS NULL OR Col96.fechaFin > @pFechaFinMes) 
                            left join(  
                                select b6.objID2 as LinT from birrelacion b6  
                                where objid = 'Bir0051' and b6.objID2 like 'Lin%' 
                                AND(@pFechaFinMes >= b6.fechaIni) AND(b6.fechaFin IS NULL OR b6.fechaFin > @pFechaFinMes)  
                               and objid1 in ( 
                                               SELECT objid1 
                                               FROM birrelacion b 
                                               where objid = 'Bir0051' and objid1 like 'Sbs%' and objID2 like 'Lin%' 
                                               AND(@pFechaFinMes >= b.fechaIni) AND(b.fechaFin IS NULL OR b.fechaFin > @pFechaFinMes) 
                                               group by objid1 
                                               having count(objID2) > 1 
                                                )) LineasT on LineasT.LinT = b1.objID2 
                            where b1.objid = 'Bir0051' 
                            and b1.objID2 not in ( 
                                                select DISTINCT Inte.OBJID2												
												from birrelacion Inte
												INNER JOIN Maestrocol col ON col.objid = Inte.objid2 AND col.subconjunto <> 'NoCompensa' AND col.colid = 'Col0096'
												AND(@pFechaFinMes >= col.fechaIni) AND(col.fechaFin IS NULL OR col.fechaFin > @pFechaFinMes)
												INNER JOIN Maestrocol col1 ON col1.objid = Inte.objid1 AND col1.subconjunto <> 'NoCompensa' AND col1.colid = 'Col0096'
												AND(@pFechaFinMes >= col1.fechaIni) AND(col1.fechaFin IS NULL OR col1.fechaFin > @pFechaFinMes)
												INNER JOIN bahia bh ON Inte.objid2 = bh.objid  
												AND(@pFechaFinMes >= bh.fechaIni) AND(bh.fechaFin IS NULL OR bh.fechaFin > @pFechaFinMes)
												INNER JOIN bahia bh2 ON  Inte.objid1 = bh2.objid
												AND(@pFechaFinMes >= bh2.fechaIni) AND(bh2.fechaFin IS NULL OR bh2.fechaFin > @pFechaFinMes)
												WHERE
												 Inte.objID ='Bir0028'
												 AND (@pFechaFinMes >= Inte.fechaIni) AND(Inte.fechaFin IS NULL OR Inte.fechaFin > @pFechaFinMes)

                                                union all

											    SELECT DISTINCT  cc.objid as CorteCentral
												FROM Maestrocol cc
												INNER JOIN Birrelacion Bah ON Bah.objID = 'Bir0506' AND bah.objID2 =cc.objID
												INNER JOIN Bahia tBah ON tBah.objID = Bah.objid1
												INNER JOIN Birrelacion Unc ON Unc.objID = 'Bir0054' AND Unc.objid2 = tBah.objID
												INNER JOIN Birrelacion Def ON Def.objID = 'Bir0457' AND Def.objid2 = Unc.objID1 AND Def.objID1 IN ('Def0330','Def0331')
												WHERE cc.subconjunto = 'CorteCentral' AND cc.ColId = 'Col0096'
												AND(@pFechaFinMes >= cc.fechaIni) AND(cc.fechaFin IS NULL OR cc.fechaFin > @pFechaFinMes) 
												AND(@pFechaFinMes >= Bah.fechaIni) AND(Bah.fechaFin IS NULL OR Bah.fechaFin > @pFechaFinMes) 
												AND(@pFechaFinMes >= tBah.fechaIni) AND(tBah.fechaFin IS NULL OR tBah.fechaFin > @pFechaFinMes) 
												AND(@pFechaFinMes >= Unc.fechaIni) AND(Unc.fechaFin IS NULL OR Unc.fechaFin > @pFechaFinMes) 
												AND(@pFechaFinMes >= Def.fechaIni) AND(Def.fechaFin IS NULL OR Def.fechaFin > @pFechaFinMes) 
                                    
                            ) 
                            AND(@pFechaFinMes >= b1.fechaIni) AND(b1.fechaFin IS NULL OR b1.fechaFin > @pFechaFinMes) 
                            ), 
                            MHAI as 
                            ( 
                                SELECT b.objID1 AS 'subsistema', c.valor AS 'MHAI' , Unidades 
                                FROM Birrelacion b 
                                INNER JOIN dbo.Constante c ON c.defID = b.objID2 
                                 WHERE b.objID = 'Bir0052' 
                                  AND(@pFechaFinMes >= b.fechaIni) AND(b.fechaFin IS NULL OR b.fechaFin > @pFechaFinMes) 
                                  AND(@pFechaFinMes >= c.fechaIni) AND(c.fechaFin IS NULL OR c.fechaFin > @pFechaFinMes) 
                            ), 
                           TiempoManiobra as 
                          ( 
                              SELECT b.objID1 AS 'elemento', c.valor AS 'TiempoManiobra',Unidades 
                              FROM Birrelacion b 
                              INNER JOIN dbo.Constante c ON c.defID = b.objID2 
                              WHERE b.objID = 'Bir0252' 
                              AND(@pFechaFinMes >= b.fechaIni) AND(b.fechaFin IS NULL OR b.fechaFin > @pFechaFinMes) 
                              AND(@pFechaFinMes >= c.fechaIni) AND(c.fechaFin IS NULL OR c.fechaFin > @pFechaFinMes) 
                          ),encapsulado as 
                          ( 
                           select e.elemento, CASE WHEN encapsulado.elemento IS NULL THEN 'Def0111' ELSE 'Def0237' end definicion, c.valor,
                           CASE WHEN encapsulado.elemento IS NULL THEN 'NoE' else 'E' end unidades
                           ,C.fechaIni,C.fechaFin 
                           from ElementList e 
                            left join( 
                                    SELECT distinct M.objID AS 'elemento',  'encapsulado' as encapsulado 
                                     FROM MaestroCol M 
                                    WHERE colid = 'Col0175' and subconjunto = 'STR' 
                                    AND(@pFechaFinMes >= M.fechaIni) AND(M.fechaFin IS NULL OR M.fechaFin > @pFechaFinMes) 
                           )encapsulado on e.elemento = encapsulado.elemento 
                                           inner join constante c ON c.defid = CASE WHEN encapsulado.elemento IS NULL THEN 'Def0111' ELSE 'Def0237' end 
                                           AND(@pFechaFinMes >= c.fechaIni) AND(c.fechaFin IS NULL OR c.fechaFin > @pFechaFinMes) 
                          ) 

                             select distinct e.Subsistema as SubsystemId,agregador as AggregatorId,e.Elemento as ElementId, 
                            isnull(e.tipo, 'SinConf') as ElementType,isnull(uc, 'NA') as UCtype,compensa as Compensate, MHAI 
                           , M.unidades as MHAIAUnit  
                           ,t.TiempoManiobra as maneuverTime 
                           ,t.unidades as maneuverUnit 
                            ,encapsulado.valor as MaintenanceTime 
                            ,encapsulado.unidades as MaintenanceUnit 
                            ,encapsulado.fechaIni AS MaintenanceStartDate 
                            ,encapsulado.fechaFin AS MaintenanceEndDate 
                            ,case when e.subconjunto='CorteCentral' then CAST(1 AS BIT) end IsCenterCut 
                            ,l.longitud as SegmentLength 
                           ,ms.ultNombre as subsystemName 
                            , isnull(ag.ultNombre,ms.ultNombre) as aggregatorName 
                          	,ee.ultNombre as ElementName							
                            from ElementList e 
                            inner join MaestroObj ms on e.Subsistema = ms.objID 
                            left join MaestroObj ag on e.agregador = ag.objID 
                            left join MaestroObj ee on e.Elemento = ee.objID 
                           left JOIN MHAI M ON M.subsistema = e.SubSistema 
                           Left JOIN TiempoManiobra T ON t.elemento = e.Elemento 
                           left join encapsulado encapsulado on e.Elemento = encapsulado.Elemento 
                           left join linea l on l.objID=e.Elemento AND(@pFechaFinMes >= l.fechaIni) AND(l.fechaFin IS NULL OR l.fechaFin > @pFechaFinMes) 
                           Where Compensa='Compensa'
                      ";


            if (codigoActivo.Count() > 0)
                sql = sql + " and e.elemento in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " )";




            DbCommand command = db.GetSqlStringCommand(sql);
            command.CommandTimeout = TimeoutTransaction;
            db.AddInParameter(command, "@pFechaFinMes", SqlDbType.DateTime, fechaFinMes);

            /*
            Si CodActivo es Nulo lo convertimos en % para traer todos los datos
            codigoActivo = String.IsNullOrEmpty(codigoActivo) ? "%" : codigoActivo;
            db.AddInParameter(command, "@pCodActivo", SqlDbType.NVarChar, string.Concat("'", string.Join("','", codigoActivo), "'"));
            */

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
                    IndicatorType = IndicatorConstants.STR
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
            var StartDate = fechaFinMes.AddSeconds(1).AddMonths(-1);            

            var consignments = GetConsignments(StartDate, fechaFinMes, codigoActivo);
            var csg = consignments.Select(cng => cng.Consecutive);
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
                                SELECT Id, ElementId, ConsignmentId, NewAvailability, causeoperationalid, ActionTypeId, InstructionTime, OccurrenceTime , ConfirmationTime, ScheduledStartDate, EspElementId, CauseOrigin, FuelCEN , PlantCEN,ElementCausingId,ElementCompanyShortName
                                FROM ActionsAgents a 
                                WHERE (a.OccurrenceTime BETWEEN DATEADD(month,-1,DATEADD(s, 1, @pFechaFinMes))  AND @pFechaFinMes) 
                                /*ultimo evento anterior a la fecha de calculo*/
		                        UNION 

                                SELECT a.Id,a.ElementId,a.ConsignmentId,a.NewAvailability,a.causeoperationalid, a.ActionTypeId,InstructionTime,a.OccurrenceTime,ConfirmationTime,ScheduledStartDate, EspElementId, CauseOrigin, FuelCEN , PlantCEN,ElementCausingId,ElementCompanyShortName
                                FROM ActionsAgents a 
                                INNER JOIN( 
					                        SELECT ElementId, MAX(OccurrenceTime) AS LastEvent 
					                        FROM ActionsAgents 
					                        WHERE OccurrenceTime<DATEADD(month,-1,DATEADD(s, 1, @pFechaFinMes))  
					                        GROUP BY ElementId 
                                        ) AS EventF on EventF.ElementId = a.ElementId AND EventF.LastEvent = a.OccurrenceTime 
                            )
	                        /*select * from EventList  */
	                        ,EventListValues AS( 
		                        SELECT e.Id, e.ElementId, e.ConsignmentId, e.NewAvailability, e.causeoperationalid, e.ActionTypeId, e.InstructionTime, e.OccurrenceTime , e.ConfirmationTime, 
			                        e.ScheduledStartDate ,causa.valor AS [type], causa.tipo ActionName, tipoMovimiento.valor AS movimiento , tipoMovimiento.tipo tipoMovimiento, EspElementId, CauseOrigin , FuelCEN , PlantCEN,ElementCausingId,ElementCompanyShortName
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
			                        null ScheduledEndDate, EspElementId, CauseOrigin , FuelCEN , PlantCEN,ElementCausingId,ElementCompanyShortName
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
			                            CASE WHEN isnull(EndOccurrenceTime, @pFechaFinMes) >= @pFechaFinMes THEN @pFechaFinMes ELSE EndOccurrenceTime END AS ValidTo, EspElementId,CauseOrigin , FuelCEN , PlantCEN,ElementCausingId
									    ,ElementCompanyShortName
                                    FROM EventsEnd ev
								    )
		                        /*select * from Result  */
	                            /*1.1.	Eventos asociados a cambios de disponibilidad*/
                                SELECT eventId, InstructionTime, OccurrenceTime, ConfirmationTime, ElementId, ActionName, [type], ConsignmentId, NewAvailability, 
			                        movement, typeMovement, EndOccurrenceTime ,ScheduledStartDate, ScheduledEndDate, ValidFrom, ValidTo, CauseOrigin , FuelCEN , PlantCEN,ElementCausingId
								    ,ElementCompanyShortName	
                                FROM Result 
                                WHERE movement in (SELECT m FROM mov) AND type not in ('Mantenimiento')  and not (movement in ('Disponible', 'Indisponible') AND CauseOrigin = 'Actuación ESP')
                                AND CauseOrigin in (
								'Causa forzado',
								'Evento No Programado Otro Sistema',
								'Evento No Programado en Consignación'
								)

                                UNION 
	                            /*1.2.	Eventos asociados a retrasos en los tiempos de maniobra*/
                                SELECT eventId, InstructionTime, OccurrenceTime, ConfirmationTime, ElementId, ActionName, [type], ConsignmentId, NewAvailability, 
			                        movement, typeMovement, EndOccurrenceTime, ScheduledStartDate, ScheduledEndDate, ValidFrom, ValidTo, CauseOrigin , FuelCEN , PlantCEN,ElementCausingId
                                ,ElementCompanyShortName
							    FROM Result 
                                WHERE((movement in ('Abrir', 'Cerrar') AND typeMovement in ('Acciones Cambio Operativo')) 
			                        or (movement in ('Bajar TAP', 'Subir TAP') AND typeMovement in ('Acciones Control PVQ'))) 
			                        /* AND DATEDIFF(MINUTE,[InstructionTime] ,[OccurrenceTime] ) > 0 */ and not (movement in ('Disponible', 'Indisponible') AND CauseOrigin = 'Actuación ESP')

                                UNION 
	                            /*2.	Acciones por consignación de emergencia - aplica para carga de acciones punto 4.*/
                                SELECT eventId, InstructionTime, OccurrenceTime, ConfirmationTime, ElementId, ActionName, [type], ConsignmentId, NewAvailability, 
			                        movement, typeMovement, EndOccurrenceTime, ScheduledStartDate, ScheduledEndDate, ScheduledStartDate as ValidFrom,  ValidTo, CauseOrigin , FuelCEN , PlantCEN,ElementCausingId
                                ,ElementCompanyShortName
							    FROM Result 
                                WHERE movement in (SELECT m FROM mov) and type in ('Mantenimiento')  and not (movement in ('Disponible', 'Indisponible') AND CauseOrigin = 'Actuación ESP')
			                     

                                UNION 
	                            /*1.4.	Ajuste de tiempos por horas no utilizadas de mantenimiento */
                                SELECT eventId, InstructionTime, OccurrenceTime, ConfirmationTime, ElementId, ActionName, [type], ConsignmentId, NewAvailability, 
			                        movement, typeMovement, EndOccurrenceTime, ScheduledStartDate, ScheduledEndDate, ScheduledStartDate as ValidFrom,  ValidTo, CauseOrigin , FuelCEN , PlantCEN,ElementCausingId
                                ,ElementCompanyShortName
							    FROM Result 
                                WHERE movement in (SELECT m FROM mov) and type in ('Mantenimiento')  and not (movement in ('Disponible', 'Indisponible') AND CauseOrigin = 'Actuación ESP')
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
		                        END  ElementeId , 
		                        ActionName, [type], ConsignmentId, NewAvailability, 
		                        movement, typeMovement, EndOccurrenceTime ,ScheduledStartDate, ScheduledEndDate, ValidFrom, ValidTo, CauseOrigin, FuelCEN , PlantCEN, ElementCausingId
                            ,ElementCompanyShortName
						    FROM Result 
                            WHERE movement in ('Disponible', 'Indisponible') AND CauseOrigin = 'Actuación ESP'

                            union
						    /*Carga HIR - no operativos*/
						    select eventId, InstructionTime, OccurrenceTime, ConfirmationTime, ElementId, ActionName, [type], ConsignmentId, NewAvailability, 
			                        movement, typeMovement, EndOccurrenceTime, ScheduledStartDate, ScheduledEndDate, ScheduledStartDate AS ValidFrom, ValidFrom AS ValidTo, CauseOrigin , FuelCEN , PlantCEN,ElementCausingId
						    ,ElementCompanyShortName
						    from Result
						    where movement in ('No Operativo', 'Finaliza No Operativo') 

;";

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
                    actions.Add(new Action()
                    {
                        Id = reader.GetGuid(0),
                        InstructionTime = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1),
                        OccurrenceTime = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
                        ConfirmationTime = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                        ElementId = reader.GetString(4),
                        ActionName = reader.IsDBNull(5) ? null : reader.GetString(5),
                        Type = reader.IsDBNull(6) ? null : reader.GetString(6),
                        ConsignmentId = reader.IsDBNull(7) ? "" : reader.GetString(7),
                        NewAvailability = reader.IsDBNull(8) ? (decimal?)null : reader.GetDecimal(8),
                        Movement = reader.IsDBNull(9) ? null : reader.GetString(9),
                        TypeMovement = reader.IsDBNull(10) ? null : reader.GetString(10),
                        EndOccurrenceTime = reader.IsDBNull(11) ? (DateTime?)null : reader.GetDateTime(11),
                        ScheduledStartDate = reader.IsDBNull(12) ? (DateTime?)null : reader.GetDateTime(12),
                        ScheduledEndDate = reader.IsDBNull(7) ? reader.GetDateTime(13) : consignments.Where(c => c.Consecutive == reader.GetString(7)).Select(cng => cng.ScheduledEndDate).FirstOrDefault(),    //reader.IsDBNull(13) ? (DateTime?)null : reader.GetDateTime(13),
                        ValidFrom = reader.IsDBNull(14) ? (DateTime?)null : reader.GetDateTime(14),
                        ValidTo = reader.IsDBNull(15) ? (DateTime?)null : reader.GetDateTime(15),
                        CauseOrigin = reader.IsDBNull(16) ? null : reader.GetString(16),
                        ElementCausingId = reader.IsDBNull(19) ? null : reader.GetString(19),
                        ElementCompanyShortName = reader.IsDBNull(20) ? null : reader.GetString(20),
                        ActionType = CalculationConstants.HID_STR

                    });
                }
            }
            catch (Exception ex)
            {

                throw new Exception("Error En HID_STR - datos - " + ex.Message);
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
                ActionType = CalculationConstants.HID_STR
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
            List<Consignment> consignmentsE = new List<Consignment>();
            List<Consignment> consignmentsNoE = new List<Consignment>();
            List<Consignment> consignments = new List<Consignment>();
            var elements = GetElements(fechaFinMes, codigoActivo);
            var elementsE = elements.Where(e => e.IndicatorType.Equals("STR") && e.MaintenanceUnit.Equals("E")).Select(e=>e.ElementId).ToList();
            var elementsNoE = elements.Where(e => e.IndicatorType.Equals("STR") && e.MaintenanceUnit.Contains("NoE")).Select(e => e.ElementId).ToList();


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
					   select (startDate) as startDatePlan,(enddate)as  EndDatePlan
							from (
							  SELECT startDate,EndDate
							  FROM [dbo].[Plans]
							  where name like '%STR Encapsulado%' 
							  and  @pFechaFinMes between StartDate and EndDate							  
							  )planMtto
					   )PlanMto
                       where
					   Consecutive is not null
					   and c.ElementId is not null
					   and ( ScheduledStartDate between  startDatePlan and EndDatePlan
					         or
							 ScheduledEndDate between  startDatePlan and EndDatePlan
						   )                       

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
                    consignmentsE.Add(new Consignment()
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
                        IndicatorType = Constants.CalculationConstants.HID_STR
                    });
                }

            }
            catch (Exception ex)
            {

                throw new Exception("Error En HID_STR - met consg " + ex.Message);
            }
            consignmentsE = consignmentsE.Where(c => elementsE.Contains(c.ElementId)).ToList();
            /*No encap*/
             sql = @" SELECT Consecutive,c.ElementId,origen.MaintenanceOriginName, 
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
					   select (startDate) as startDatePlan,(enddate)as  EndDatePlan
							from (
							  SELECT startDate,EndDate
							  FROM [dbo].[Plans]
							  where name like '%STR No Encapsulado%' 
							  and  @pFechaFinMes between StartDate and EndDate							  
							  )planMtto
					   )PlanMto
                       where
					   Consecutive is not null
					   and c.ElementId is not null
					   and ( ScheduledStartDate between  startDatePlan and EndDatePlan
					         or
							 ScheduledEndDate between  startDatePlan and EndDatePlan
						   )                       

            ";
            //   "  AND ElementId like @pCodActivo ";

            if (codigoActivo.Count() > 0)
                sql = sql + " AND c.ElementId in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " )";


            DbCommand command2 = db.GetSqlStringCommand(sql);
            command2.CommandTimeout = TimeoutTransaction;

            //db.AddInParameter(command, "@pFechaInicioMes", SqlDbType.DateTime, fechaInicioMes);
            db.AddInParameter(command2, "@pFechaFinMes", SqlDbType.DateTime, fechaFinMes);

            //Si CodActivo es Nulo lo convertimos en % para traer todos los datos
            //codigoActivo = String.IsNullOrEmpty(codigoActivo) ? "%" : codigoActivo;
            //db.AddInParameter(command, "@pCodActivo", SqlDbType.NVarChar, codigoActivo);

            reader = db.ExecuteReader(command2);

            try
            {
                while (reader.Read())
                {
                    consignmentsNoE.Add(new Consignment()
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
                        IndicatorType = Constants.CalculationConstants.HID_STR
                    });
                }

            }
            catch (Exception ex)
            {

                throw new Exception("Error En HID_STR - met consg " + ex.Message);
            }
            consignmentsNoE = consignmentsNoE.Where(c => elementsNoE.Contains(c.ElementId)).ToList();
            consignments = consignmentsE.Union(consignmentsNoE).ToList();
            return consignments;
        }

        public List<Consignment> GetConsignmentsEmergency(DateTime fechaInicioMes, DateTime fechaFinMes, List<string> codigoActivo)
        {
            List<Consignment> consignments = new List<Consignment>();
            var actions = GetActions(fechaFinMes, codigoActivo);
            var cns = actions.Where(c=>c.ConsignmentId.Length>0).Select(cng => cng.ConsignmentId );
            if (cns.ToList().Count() == 0) return consignments;

            SqlDatabase db = new SqlDatabase(ConsignmentOriginConnectionString);

            var sql = @" 
                        SELECT Consecutive,c.ElementId,origen.MaintenanceOriginName, 
                        NULL CauseName,	ScheduledStartDate,ScheduledEndDate ,
                       RealStartDate,RealEndDate, NULL penalDate, NULL ActivePenal 
                        , ElementCompanyShortName , EntryType, null,null,null
                       FROM [dbo].[Consignments] c 
                        INNER JOIN 
                       (SELECT [value] as MaintenanceOriginName, id from [dbo].[TypeValues]                          
                        where [value]='Normal'
					   ) origen 
                       ON origen.Id = c.MaintenanceOriginId 
					   INNER JOIN 
                       (SELECT [value] as EntryType, id from [dbo].[TypeValues]                          
                        where [value]='Emergencia'
					   ) CnsType  on CnsType.Id = c.EntryTypeId 
                       where
					   Consecutive is not null
					   and c.ElementId is not null 

                        UNION

                        SELECT distinct Consecutive,c.ElementId,origen.MaintenanceOriginName, 
                        NULL CauseName,	c.ScheduledStartDate,c.ScheduledEndDate ,
                        aff.RealStartDate,aff.RealEndDate, NULL penalDate, NULL ActivePenal 
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
					   ) CnsType  on CnsType.Id = c.EntryTypeId 
                       inner join [dbo].[Affectations] aff on c.Id= aff.ConsignmentId
					   inner join (
							SELECT [value] as Affectation, id 
						    from [dbo].[TypeValues]                          
							where (typeid ='3CFF24DB-C7DB-416A-87F3-62B8AB250D76'
							and [value]  in ('RD'))							
					   )typeAff on  aff.AffectationTypeId=typeAff.id
					   where   Consecutive is not null
					   and c.ElementId is not null 
					   and (c.ScheduledStartDate BETWEEN DATEADD(month,-1,DATEADD(s, 1, @pFechaFinMes))  AND @pFechaFinMes 
                       or (c.ScheduledEndDate BETWEEN DATEADD(month,-1,DATEADD(s, 1, @pFechaFinMes))  AND @pFechaFinMes)	)

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
                        AffectationTypeId= reader.IsDBNull(12) ? null : reader.GetString(12),
                        IndicatorType = Constants.CalculationConstants.HID_STR
                    });
                }

            }
            catch (Exception ex)
            {

                throw new Exception("Error En HID_STR - met consg " + ex.Message);
            }
            consignments = consignments.Where(a => cns.Contains(a.Consecutive)).ToList();
            return consignments;
        }

        public List<Consignment> GetConsignmentsScheduled(DateTime fechaInicioMes, DateTime fechaFinMes, List<string> codigoActivo)
        {
            List<Consignment> consignments = new List<Consignment>();
           
            SqlDatabase db = new SqlDatabase(ConsignmentOriginConnectionString);

            var sql = @" 
                        SELECT distinct Consecutive,c.ElementId,origen.MaintenanceOriginName, 
                        NULL CauseName,	c.ScheduledStartDate,c.ScheduledEndDate ,
                        aff.RealStartDate,aff.RealEndDate, NULL penalDate, NULL ActivePenal 
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
					   ) CnsType  on CnsType.Id = c.EntryTypeId 
                       inner join [dbo].[Affectations] aff on c.Id= aff.ConsignmentId
                       and c.ElementId = aff.ElementId
					   inner join (
							SELECT [value] as Affectation, id 
						    from [dbo].[TypeValues]                          
							where (typeid ='3CFF24DB-C7DB-416A-87F3-62B8AB250D76'
							and [value]  in ('D','DD','DA','DP'))							
					   )typeAff on  aff.AffectationTypeId=typeAff.id
					   where   Consecutive is not null
					   and c.ElementId is not null 
					   and (aff.CauseRealStartDateChangeId in (
															SELECT id from [dbo].[TypeValues] where typeid='E507C309-EB54-4FF5-8AE4-6B0396AD533B'
														)
					   or aff.CauseRealEndDateChangeId in (
															SELECT id from [dbo].[TypeValues] where typeid='E507C309-EB54-4FF5-8AE4-6B0396AD533B'
														))
					  and (c.ScheduledStartDate BETWEEN DATEADD(month,-1,DATEADD(s, 1, @pFechaFinMes))  AND @pFechaFinMes 
                      or (c.ScheduledEndDate BETWEEN DATEADD(month,-1,DATEADD(s, 1, @pFechaFinMes))  AND @pFechaFinMes)	)

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
                        AffectationTypeId= reader.IsDBNull(12) ? null : reader.GetString(12),
                        CauseRealStartDateChange= reader.IsDBNull(13) ? null : reader.GetString(13),
                        CauseRealEndDateChange= reader.IsDBNull(14) ? null : reader.GetString(14),
                        IndicatorType = Constants.CalculationConstants.HID_STR
                    });
                }

            }
            catch (Exception ex)
            {

                throw new Exception("Error En HID_STR - consg scheduled " + ex.Message);
            }
            
            return consignments;
        }

        public List<Consignment> GetConsignmentsDesv_RD(DateTime fechaInicioMes, DateTime fechaFinMes, List<string> codigoActivo, string indicatorType)
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
                        CauseRealStartDateChange= reader.IsDBNull(13) ? null : reader.GetString(13),
                        CauseRealEndDateChange= reader.IsDBNull(14) ? null : reader.GetString(14),
                        IndicatorType = Constants.CalculationConstants.HID_STR
                    });
                }

            }
            catch (Exception ex)
            {

                throw new Exception("Error En HID_STR - desv RD consg " + ex.Message);
            }
            consignments = consignments.Where(cons => ele.Contains(cons.ElementId)).ToList();
            return consignments;
        }


    }
}
