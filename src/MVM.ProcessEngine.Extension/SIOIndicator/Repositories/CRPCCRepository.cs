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
using Element= MVM.ProcessEngine.Extension.SIOIndicator.Domain.Element;
using Action = MVM.ProcessEngine.Extension.SIOIndicator.Domain.Action;

namespace MVM.ProcessEngine.Extension.SIOIndicator.Repositories
{
    public class CRPCCRepository : SQLRepository
    {
        public CRPCCRepository(string tenant) : base(tenant) { }
        public List<Element> GetElements(DateTime fechaFinMes, List<string> codigoActivo)
        {
            List<Element> elementList = new List<Element>();

            SqlDatabase db = new SqlDatabase(ElementOriginConnectionString);
            var sql = @"Select bi.objid1 as subsystemid, bi.objid2 as aggregator, 
                        bi.objid2 as elementid,null ElementType,  null UCtype,
                        null Compensate,null MHAI, null MHAIAUnit,null ManeuverTime,
                        null ManeuverUnit,null MaintenanceTime, null as MaintenanceUnit,
                        ac.fechaini MaintenanceStartDate, isnull(ac.fechafin,getdate()) MaintenanceEndDate,
                        null IsCenterCut ,null SegmentLength, ac.nombre as SubsystemName,
                        t1.ultNombre as AggregatorName,t1.ultNombre as ElementName,
                        'RACC' as IndicatorType
                        ,b.objID1  companyId
                        ,c.nombre companyName
                        ,c.sigla as shortName
                        ,ac.capacidadTransporte DefaultCapacity
                        from Birrelacion as BI
                        Left join MaestroObj T1 on T1.objID = BI.objID2
                        left join registrar.AcuerdoFrontera ac on BI.objID1=ac.objID
                        inner join Birrelacion as B on b.objID2=bi.objid2
                        inner join Compania c on c.objID=b.objID1
                        where (BI.objID = 'Bir0516' ) 
                        and   (b.objID = 'Bir0014')
                        AND(@pFechaFinMes >= BI.fechaIni) AND(BI.fechaFin IS NULL OR BI.fechaFin > @pFechaFinMes) 
                        AND(@pFechaFinMes >= ac.fechaIni) AND(ac.fechaFin IS NULL OR ac.fechaFin > @pFechaFinMes) 
                        AND(@pFechaFinMes >= B.fechaIni) AND(B.fechaFin IS NULL OR B.fechaFin > @pFechaFinMes) 
                        AND(@pFechaFinMes >= c.fechaIni) AND(c.fechaFin IS NULL OR c.fechaFin > @pFechaFinMes) ";

            if (codigoActivo.Count() > 0)
                sql = sql + " and bi.objid2 in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " )";

            DbCommand command = db.GetSqlStringCommand(sql);
            command.CommandTimeout = TimeoutTransaction;
            db.AddInParameter(command, "@pFechaFinMes", SqlDbType.DateTime, fechaFinMes);

            var reader = db.ExecuteReader(command);

            try
            {
                while (reader.Read())
                {
                    elementList.Add(new Element()
                    {
                        SubsystemId = reader.GetString(0),
                        AggregatorId = reader.GetString(1),
                        ElementId = reader.GetString(2),
                        //UCtype = reader.IsDBNull(4) ? null : reader.GetString(4),
                        //Compensate = reader.GetString(5),
                        //MHAI = reader.IsDBNull(6) ? (double?)null : reader.GetDouble(6),
                        //MHAIAUnit = reader.IsDBNull(7) ? null : reader.GetString(7),
                        //ManeuverTime = reader.IsDBNull(8) ? (double?)null : reader.GetDouble(8),
                        //ManeuverUnit = reader.IsDBNull(9) ? null : reader.GetString(9),
                        //MaintenanceTime = reader.IsDBNull(10) ? (double?)null : reader.GetDouble(10),
                        //MaintenanceUnit = reader.IsDBNull(11) ? null : reader.GetString(11),
                        MaintenanceStartDate = reader.GetDateTime(12) ,
                        MaintenanceEndDate = reader.GetDateTime(13) ,
                        //IsCenterCut = reader.IsDBNull(14) ? (bool?)null : reader.GetBoolean(14),
                        //SegmentLength = reader.IsDBNull(15) ? (double?)null : reader.GetDouble(15),
                        SubsystemName = reader.IsDBNull(16) ? null : reader.GetString(16),
                        AggregatorName = reader.IsDBNull(17) ? null : reader.GetString(17),
                        ElementName = reader.IsDBNull(18) ? null : reader.GetString(18),
                        IndicatorType = IndicatorConstants.RACC,
                        OperatorCompanyId= reader.IsDBNull(20) ? null : reader.GetString(20),
                        OperatorCompanyName= reader.IsDBNull(21) ? null : reader.GetString(21),
                        OperatorCompanyShortName= reader.IsDBNull(22) ? null : reader.GetString(22),
                        DefaultCapacity= reader.IsDBNull(23) ? null : reader.GetString(23)
                    });
                    ;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return elementList;
        }

        public List<Action> GetActions(DateTime fechaFinMes, List<string> codigoActivo)
        {
            var StartDate = fechaFinMes.AddSeconds(1).AddMonths(-1);

            var elements = GetElements(fechaFinMes, codigoActivo);
            var elemts = elements.Select(ele => ele.ElementId);
            List<Action> actions = new List<Action>();

            SqlDatabase db = new SqlDatabase(ManeuverOriginConnectionString);

            var sql = @"WITH mov AS (
                            SELECT m
                            FROM (SELECT 'Abrir' M
                                  UNION SELECT 'Cerrar' M
								  UNION SELECT 'Fuera de servicio' M
								  UNION SELECT 'En Servicio' M
                                  UNION SELECT 'Disponible' M
                                  UNION SELECT 'Indisponible' M
								  UNION SELECT 'No Operativo' M
                                  UNION SELECT 'Finaliza no operativo' M) MM
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
                                FROM Actions a 
                                WHERE (a.OccurrenceTime BETWEEN 
								convert(datetime,convert(date,@pFechaFinMes))
								AND DATEADD(s, -1, DATEADD(day,1,convert(datetime,convert(date,@pFechaFinMes))))) 
                                /*ultimo evento anterior a la fecha de calculo*/
		                        UNION 

                                SELECT a.Id,a.ElementId,a.ConsignmentId,a.NewAvailability,a.causeoperationalid, a.ActionTypeId,InstructionTime,a.OccurrenceTime,ConfirmationTime,ScheduledStartDate, EspElementId, CauseOrigin, FuelCEN , PlantCEN,ElementCausingId,ElementCompanyShortName
                                FROM Actions a 
                                INNER JOIN( 
					                        SELECT ElementId, MAX(OccurrenceTime) AS LastEvent 
					                        FROM Actions 
					                        WHERE OccurrenceTime<convert(datetime,convert(date,@pFechaFinMes))  
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
                                FROM EventListValues ev  ";

            if (codigoActivo.Count() > 0)
                sql = sql + " WHERE ElementId in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " )";

            sql += @")
	                            /*select * from EventsEnd  */
	                            ,Result as ( 
		                            /*calculo de fechas de inicio y fin de acuerdo al mes de calculo*/
                                    SELECT eventId, InstructionTime, OccurrenceTime, ConfirmationTime, ElementId, ActionName,[type], ConsignmentId, NewAvailability, movement, typeMovement, 
			                            ISNULL(EndOccurrenceTime, DATEADD(s, -1, DATEADD(day,1,convert(datetime,convert(date,@pFechaFinMes))))) EndOccurrenceTime, 
										ScheduledStartDate, 
			                            ISNULL(EndOccurrenceTime, DATEADD(s, -1, DATEADD(day,1,convert(datetime,convert(date,@pFechaFinMes))))) ScheduledEndDate,
			                            CASE WHEN ev.OccurrenceTime < convert(datetime,convert(date,@pFechaFinMes)) 
										THEN convert(datetime,convert(date,@pFechaFinMes)) 
										ELSE ev.OccurrenceTime END AS ValidFrom, 
			                            CASE WHEN DATEADD(s, -1, DATEADD(day,1,convert(datetime,convert(date,@pFechaFinMes)))) >= @pFechaFinMes 
										THEN DATEADD(s, -1, DATEADD(day,1,convert(datetime,convert(date,@pFechaFinMes)))) 
										ELSE EndOccurrenceTime END AS ValidTo, EspElementId,CauseOrigin , FuelCEN , PlantCEN,ElementCausingId
									    ,ElementCompanyShortName
                                    FROM EventsEnd ev
								    )
		                        /*select * from Result  */
	                            SELECT eventId, InstructionTime, OccurrenceTime, ConfirmationTime, 
								ElementId, ActionName, [type], ConsignmentId, NewAvailability, 
			                        movement, typeMovement, EndOccurrenceTime ,ScheduledStartDate, ScheduledEndDate, ValidFrom, ValidTo, CauseOrigin , FuelCEN , PlantCEN,ElementCausingId
								    ,ElementCompanyShortName	
                                FROM Result 
                                WHERE movement in (SELECT m FROM mov)  ";

            sql = sql + " and ElementId in ( " + string.Concat("'", string.Join("','", elemts), "'") + " )";


            DbCommand command = db.GetSqlStringCommand(sql);
            command.CommandTimeout = TimeoutTransaction;
            //db.AddInParameter(command, "@pFechaInicioMes", SqlDbType.DateTime, fechaInicioMes);
            db.AddInParameter(command, "@pFechaFinMes", SqlDbType.DateTime, fechaFinMes);

            //Si CodActivo es Nulo lo convertimos en % para traer todos los datos
            //codigoActivo = String.IsNullOrEmpty(codigoActivo) ? "%" : codigoActivo;
            //db.AddInParameter(command, "@pCodActivo", SqlDbType.NVarChar, codigoActivo);

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
                    ElementCausingId = reader.IsDBNull(19) ? null : reader.GetString(19),
                    ElementCompanyShortName = reader.IsDBNull(20) ? null : reader.GetString(20),
                    ActionType = CalculationConstants.CRPCC

                });
            }

            return actions;
        }

        public List<Topology> GetTopologies(DateTime fechaFinMes, List<string> codigoActivo)
        {
            List<Topology> topologies = new List<Topology>();

            SqlDatabase db = new SqlDatabase(ManeuverOriginConnectionString);

            var sql = @" SELECT t.id  as TopologyId, CONCAT(TopologyName,' ',convert(varchar(3),NumberTopology)) Topology, 
                         elementid ,Cpcc,StartDate,EndDate, Types.valor value
                         FROM [dbo].[SioRaccTopologies] t
                         inner join [dbo].[SioRaccTopologyAvailableElements] te on 
                         t.id=te.TopologyId
                         inner join ( SELECT tv.id, tv.Value valor, t.Name AS tipo FROM [dbo].[TypeValues] Tv 
		                              INNER JOIN[dbo].[Types] T on tv.TypeId= t.Id 
                                    )Types on Types.id=Te.AvailableTypeId
                         WHERE t.StartDate <= @pFechaFinMes and ( t.EndDate>=@pFechaFinMes or t.EndDate is null)  ";

           // if (codigoActivo.Count() > 0)
           //     sql = sql + " WHERE ElementId in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " )";
           

            DbCommand command = db.GetSqlStringCommand(sql);
            command.CommandTimeout = TimeoutTransaction;
            //db.AddInParameter(command, "@pFechaInicioMes", SqlDbType.DateTime, fechaInicioMes);
            db.AddInParameter(command, "@pFechaFinMes", SqlDbType.DateTime, fechaFinMes);

            //Si CodActivo es Nulo lo convertimos en % para traer todos los datos
            //codigoActivo = String.IsNullOrEmpty(codigoActivo) ? "%" : codigoActivo;
            //db.AddInParameter(command, "@pCodActivo", SqlDbType.NVarChar, codigoActivo);

            var reader = db.ExecuteReader(command);

            while (reader.Read())
            {
                topologies.Add(new Topology()
                {
                    TopologyId = reader.GetGuid(0),
                    TopologyName = reader.GetString(1),
                    ElementId = reader.GetString(2) ,
                    CPCC = reader.GetDecimal(3),
                    StartDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                    EndDate = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                    Value = reader.IsDBNull(6) ? null : reader.GetString(6)                   

                });
            }

            return topologies;
        }

    }
}
