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
    public class MHAIA_STRRepository: SQLRepository
    {
        public MHAIA_STRRepository(string tenant) : base(tenant) 
        {
        }
                
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fechaFinMes"></param>
        /// <param name="codigoActivo"></param>
        /// <returns></returns>
        public List<Element> GetElements(DateTime fechaFinMes, List<string> codigoActivo)
        {
            try
            {
                List<Element> elementList = new List<Element>();

                SqlDatabase db = new SqlDatabase(ElementOriginConnectionString);
                var sql = @"With ElementList as ( 
                            select b1.objID, b1.objID1 as SubSistema,  
                            CASE
                               WHEN Anillo.ObjID1 is not null then Anillo.ObjID1
                               WHEN IntM.Agrupadora is not null THEN IntM.Agrupadora
                               WHEN LineasT.LinT is not null THEN 'T_' + b1.objID1
                               ELSE b1.objID2
                              END AS Agregador,
                            CASE
                               WHEN Anillo.ObjID1 is not null then Anillo.ObjID2
                               WHEN IntM.Agrupadora is not null THEN b1.objID2
                               WHEN Barras.objid1 is not null THEN Barras.objid2
                               WHEN LineasT.LinT is not null THEN LineasT.LinT
                               ELSE b1.objID2
                              END AS Elemento,
                            CASE
                               WHEN Anillo.ObjID1 is not null then 'Anillo'
                               WHEN IntM.Agrupadora is not null and b3.objID1 is null THEN 'Int. y Medio'
                               WHEN IntM.Agrupadora is not null and b3.objID1 is not null THEN 'Int. y Medio UC15'
                               WHEN LineasT.LinT is not null THEN 'Lineas en T'
                               WHEN Barras.objid1 is not null THEN 'Barras'
                               ELSE Null
                              END AS Tipo,  
                            CASE
                               WHEN b2.objID1 is not null then 'UC14'
                               WHEN b3.objID1 is not null THEN 'UC15'
                               ELSE Null
                              END AS UC, 
                            CASE
                               WHEN Col96.subconjunto is null or Col96.subconjunto = 'NoCompensa' then 'NoCompensa'
                               ELSE 'Compensa'
                              END AS Compensa, 
                            CASE
                             WHEN Col96.subconjunto = 'CorteCentral' then CAST(1 AS BIT) 
                             END AS IsCenterCut
                            from birrelacion b1
                            left join birrelacion b2 on b2.objID2 = b1.objID2 and b2.objid = 'Bir0054' and b2.objID1 = 'Unc0014'
                            AND(@pFechaFinMes >= b1.fechaIni) AND(b1.fechaFin IS NULL OR b1.fechaFin > @pFechaFinMes)
                            AND(@pFechaFinMes >= b2.fechaIni) AND(b2.fechaFin IS NULL OR b2.fechaFin > @pFechaFinMes)
                            left join birrelacion b3 on b3.objID2 = b1.objID2 and b3.objid = 'Bir0054' and b3.objID1 = 'Unc0015'
                            AND(@pFechaFinMes >= b3.fechaIni) AND(b3.fechaFin IS NULL OR b3.fechaFin > @pFechaFinMes)
                            left join(select b4.objID1 as ObjID1,b4.objID2 as ObjID2 from birrelacion b4
                            inner
                                                                                     join birrelacion b5 on b5.objID2 = b4.objID1
                                                   
                                                                               and b5.objid1 in ('Unc0174', 'Unc0175', 'Unc0597', 'Unc0202', 'Unc0203')
                            and b5.objID = 'Bir0054'
                           AND(@pFechaFinMes >= b4.fechaIni) AND(b4.fechaFin IS NULL OR b4.fechaFin > @pFechaFinMes)
                           AND(@pFechaFinMes >= b5.fechaIni) AND(b5.fechaFin IS NULL OR b5.fechaFin > @pFechaFinMes)
                           where b4.objid = 'Bir0028' and b4.fechaFin is null
                                     ) Anillo on Anillo.ObjID1 = b1.objID2
                            left join(
                                       select b1.objid1 as SubSistema, b1.objid2 Agrupadora, intMedio.ObjID2 IntMedio from birrelacion b1
                                       inner
                                                                                                                      join (
                                            select b4.objID1 as ObjID1,b4.objID2 as ObjID2 from birrelacion b4
                                       inner
                                                                                           join birrelacion b5 on b5.objID2 = b4.objID1
                                              
                                                                          AND(@pFechaFinMes >= b4.fechaIni) AND(b4.fechaFin IS NULL OR b4.fechaFin > @pFechaFinMes)
                             AND(@pFechaFinMes >= b5.fechaIni) AND(b5.fechaFin IS NULL OR b5.fechaFin > @pFechaFinMes)
                             and b5.objid1 in ('Unc0172', 'Unc0173', 'Unc0199', 'Unc0200', 'Unc0201') and b5.objID = 'Bir0054'
                                           where b4.objid = 'Bir0506'
                                            ) intMedio on intMedio.ObjID1 = b1.objID2
                                            where b1.objid = 'Bir0051'
                                             AND(@pFechaFinMes >= b1.fechaIni) AND(b1.fechaFin IS NULL OR b1.fechaFin > @pFechaFinMes)
                                      ) as IntM on IntM.SubSistema = b1.objID1
                            and(IntM.Agrupadora = b1.objID2 or IntM.IntMedio = b1.objID2)
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
                            select b4.objID2 as ObjID2 from birrelacion b4
                                               inner
                                                       join birrelacion b5 on b5.objID2 = b4.objID1 and
                                               b5.objid1 in ('Unc0174', 'Unc0175', 'Unc0597', 'Unc0202', 'Unc0203')
                                                and b5.objID = 'Bir0054'
                                                 AND(@pFechaFinMes >= b5.fechaIni) AND(b5.fechaFin IS NULL OR b5.fechaFin > @pFechaFinMes)
                                                where b4.objid = 'Bir0028'
                                   AND(@pFechaFinMes >= b4.fechaIni) AND(b4.fechaFin IS NULL OR b4.fechaFin > @pFechaFinMes)
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
                           select e.elemento, CASE WHEN encapsulado.elemento IS NULL THEN 'Def0111' ELSE 'Def0237' end definicion, c.valor,c.unidades,C.fechaIni,C.fechaFin
                           from ElementList e
                            left join(
                                    SELECT distinct M.objID AS 'elemento',  'encapsulado' as encapsulado
                                     FROM MaestroCol M
                                    WHERE colid = 'Col0175' and subconjunto = 'STR'
                                    AND(@pFechaFinMes >= M.fechaIni) AND(M.fechaFin IS NULL OR M.fechaFin > @pFechaFinMes)
                           )encapsulado on e.objID = encapsulado.elemento
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
                            ,IsCenterCut
                            ,l.longitud as SegmentLength
                           ,ms.ultNombre as subsystemName
                            , ag.ultNombre as aggregatorName
                          	,ee.ultNombre as ElementName
							,b.objID1 companyId
                            , c.nombre companyName
                             , c.sigla as shortName
                            from ElementList e
                            inner
                            join MaestroObj ms on e.Subsistema = ms.objID

                      left
                            join MaestroObj ag on e.agregador = ag.objID

                       left
                            join MaestroObj ee on e.Elemento = ee.objID

                      left JOIN MHAI M ON M.subsistema = e.SubSistema
                           Left JOIN TiempoManiobra T ON t.elemento = e.Elemento
                           left join encapsulado encapsulado on e.Elemento = encapsulado.Elemento
                           left join linea l on l.objID = e.Elemento AND(@pFechaFinMes >= l.fechaIni) AND(l.fechaFin IS NULL OR l.fechaFin > @pFechaFinMes)
                           left join
                                    (
                                    select* from Birrelacion where objID = 'Bir0014'

                                    AND(@pFechaFinMes >= fechaIni) AND(fechaFin IS NULL OR fechaFin > @pFechaFinMes)
									)  b on B.objid2 = e.elemento --or B.objid2 = e.Agregador

                           left join(
                                    select* from  Compania
                                    where (@pFechaFinMes >= fechaIni) AND(fechaFin IS NULL OR fechaFin > @pFechaFinMes)
							 )c on c.objID = b.objID1

                           Where Compensa = 'Compensa' ";


                if (codigoActivo.Count() > 0)
                {
                    sql = sql + " and e.elemento in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " )";
                    sql = sql + " OR agregador in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " )";
                }



                DbCommand command = db.GetSqlStringCommand(sql);
                command.CommandTimeout = TimeoutTransaction;
                db.AddInParameter(command, "@pFechaFinMes", SqlDbType.DateTime, fechaFinMes);

                //Si CodActivo es Nulo lo convertimos en % para traer todos los datos
                //codigoActivo = String.IsNullOrEmpty(codigoActivo) ? "%" : codigoActivo;
                //db.AddInParameter(command, "@pCodActivo", SqlDbType.NVarChar, string.Concat("'", string.Join("','", codigoActivo), "'"));


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
                        OperatorCompanyId = reader.IsDBNull(19) ? null : reader.GetString(19),
                        OperatorCompanyName = reader.IsDBNull(20) ? null : reader.GetString(20),
                        OperatorCompanyShortName = reader.IsDBNull(21) ? null : reader.GetString(21),
                        IndicatorType = IndicatorConstants.STR
                    });
                    ;
                }

                return elementList;
            }
            catch (Exception ex)
            {

                throw new Exception("Error En GetElements - MHAIA - STR" + ex.Message);
            }
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
            try
            {
                List<Consignment> consignments = new List<Consignment>();

                SqlDatabase db = new SqlDatabase(ConsignmentOriginConnectionString);

                var sql = @"  with Penalizacion as ( 
                                            select penal.ConsignmentId, penal.date, causa.causa, p.active ActivePenal
                                            from(
                                            select ConsignmentId, max(date) date
                                            FROM[dbo].[Penalties] p
                                            inner join(select id, value causa   from[dbo].[TypeValues] 
                                            where typeid in ( 
										    SELECT[Id] FROM[dbo].[Types] 
                                            where name like '%Tipo Penalizacion%') 
                                                and value IN('CancelaCsgEmergencia', 'CsgEmergencia', 
                                                'AdicionPSM','CancelaCsgIngFueraPSM','CancelaCsgPSM','ModFechaAdicionAP', 
                                                'ModFechaAdicionEA', 'ModFechaCambiaSemana','ModFechaCancelaAP',  
									            'ModFechaCancelaEA', 'ModFechaDuracion', 'ReprogramaCsg' 
									             )) causa on p.PenaltyTypeId = causa.id 
                                            where p.active = 1  and date between @pFechaInicioMes
                                            and  @pFechaFinMes
                                           group by  ConsignmentId
                                          )penal
                                         inner join[dbo].[Penalties] p on penal.ConsignmentId = p.ConsignmentId
                                         and p.date = penal.date
                                        inner join(select id, value causa   from[dbo].[TypeValues]
                                        where typeid in (
                                        SELECT[Id] FROM[dbo].[Types]
                                           where name like '%Tipo Penalizacion%') 
                                    and value IN('CancelaCsgEmergencia', 'CsgEmergencia',
                                    'AdicionPSM', 'CancelaCsgIngFueraPSM', 'CancelaCsgPSM', 'ModFechaAdicionAP',
                                    'ModFechaAdicionEA', 'ModFechaCambiaSemana', 'ModFechaCancelaAP',
                                    'ModFechaCancelaEA', 'ModFechaDuracion', 'ReprogramaCsg'
                                     )) causa on p.PenaltyTypeId = causa.id                 
                                  ), 
                            consignaciones as ( 
                                    SELECT c.id ,Consecutive,ElementId,ScheduledStartDate,ScheduledEndDate ,origen.MaintenanceOriginName,ElementCompanyShortName 
                                    FROM[dbo].[Consignments] c 
                                   inner join 
                                   (select[value] as MaintenanceOriginName, id    from[dbo].[TypeValues] 
                                       where typeid in ( 
                                           SELECT Id FROM[dbo].[Types] 
                                           where name like '%Origen Mantenimiento%' 
                                       and value IN('Normal'))) origen 
                                  on origen.Id = c.MaintenanceOriginId 
                            ) 
                            select distinct c.Consecutive,c.ElementId,c.MaintenanceOriginName ,p.causa AS CauseName, 
                                    c.ScheduledStartDate,c.ScheduledEndDate, p.ActivePenal 
                            , ElementCompanyShortName 
                           from Penalizacion p 
                            inner join consignaciones c on p.ConsignmentId = c.Id  
                             ";
                //"  where(c.ScheduledStartDate  between @pFechaInicioMes  and  @pFechaFinMes " +
                //"      or c.ScheduledEndDate  between @pFechaInicioMes  and  @pFechaFinMes) ";
                //    "  and ElementId like @pCodActivo ";

                if (codigoActivo.Count() > 0)
                    sql = sql + " and C.ElementId in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " )";



                DbCommand command = db.GetSqlStringCommand(sql);
                command.CommandTimeout = TimeoutTransaction;

                db.AddInParameter(command, "@pFechaInicioMes", SqlDbType.DateTime, fechaInicioMes);
                db.AddInParameter(command, "@pFechaFinMes", SqlDbType.DateTime, fechaFinMes);

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
                        CauseName = reader.GetString(3),
                        ScheduledStartDate = reader.GetDateTime(4),
                        ScheduledEndDate = reader.GetDateTime(5),
                        ActivePenal = reader.GetBoolean(6),
                        ElementCompanyShortName = reader.IsDBNull(7) ? null : reader.GetString(7),
                        IndicatorType = Constants.CalculationConstants.MHAIA_STR
                    });
                }

                return consignments;
            }
            catch (Exception ex)
            {

                throw new Exception("Error En GetConsignments - MHAIA - STR" + ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fechaInicioMes"></param>
        /// <param name="fechaFinMes"></param>
        /// <param name="codigoActivo"></param>
        /// <returns></returns>
        public List<Action> GetActions(DateTime fechaInicioMes, DateTime fechaFinMes, List<string> codigoActivo)
        {
            try
            {
                List<Action> actions = new List<Action>();

                SqlDatabase db = new SqlDatabase(ManeuverOriginConnectionString);

                var sql = @"WITH TipoAccion AS ( 
                       SELECT Id FROM[dbo].[TypeValues]  
                       WHERE VALUE IN('Bajar TAP', 'Subir TAP')  
                       UNION 
                       SELECT Id FROM[dbo].[TypeValues] 
                               WHERE typeid IN 
                                   ( 
                                   SELECT[Id] 
                                   FROM[dbo].[Types] 
                                   WHERE NAME LIKE '%Acciones Cambio Operativo%' 
                                   ) 
                           AND VALUE IN('Abrir', 'Cerrar') 
                           ), 
                       estados AS( 
                           SELECT id FROM[dbo].[TypeValues] 
                           WHERE typeid in 
                           ( 
                           SELECT[Id] 
                           FROM [dbo].[Types] 
                           WHERE NAME  LIKE '%Estado de Acciones de Maniobra%' 
                           ) 
                           and VALUE IN('Ejecutada', 'Validada', 'Editada') 
                       ), 
                       causaEvento AS( 
                           SELECT* FROM [dbo].[TypeValues] 
                           WHERE typeid IN 
                           ( 
                           SELECT[Id] 
                           FROM [dbo].[Types] 
                           WHERE NAME  LIKE '%Causa Cambio Operativo Maniobra%' 
                           ) 
                           AND VALUE IN('cond. operativa', 'Evento') 
                       ), 
                       causaManiobra AS( 
                           SELECT * FROM [dbo].[TypeValues] 
                           WHERE typeid in 
                           ( 
							   SELECT[Id] 
							   FROM [dbo].[Types] 
							   WHERE NAME  LIKE '%Causa Cambio Operativo Maniobra%' 
                           ) 
                           AND VALUE IN('Mantenimiento', 'Instrucción CND') 
                       ) 
                       SELECT a.id, [OccurrenceTime],[ConfirmationTime],elementid, 
                       CASE WHEN causeoperationalid IN(SELECT id FROM causaEvento) THEN 'Evento' 
                       WHEN causeoperationalid IN(SELECT id FROM causaManiobra) THEN 'Maniobra' 
                       END [Action],Causa.value as Type, isnull(CauseOrigin,'') CauseOrigin
                       ,ElementCompanyShortName
                       FROM Actions a 
                       inner join( 
                               SELECT id, value FROM causaEvento 
                               UNION 
                               SELECT id, value FROM causaManiobra 
                               ) Causa on a.causeoperationalid = Causa.Id 
                       WHERE ActionTypeId IN(SELECT * FROM TipoAccion) 
                       AND StatustypeID IN(SELECT * FROM estados) 
                       AND DATEDIFF(MI, [OccurrenceTime], [ConfirmationTime]) > 0 
                       AND(a.OccurrenceTime  > dateadd(MONTH,-12, @pFechaFinMes)  
							and a.OccurrenceTime  <= @pFechaFinMes) ";
                //                      "  AND ElementId LIKE @pCodActivo";

                if (codigoActivo.Count() > 0)
                    sql = sql + " AND ElementId in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " )";


                DbCommand command = db.GetSqlStringCommand(sql);
                command.CommandTimeout = TimeoutTransaction;
                db.AddInParameter(command, "@pFechaInicioMes", SqlDbType.DateTime, fechaInicioMes);
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
                        OccurrenceTime = reader.GetDateTime(1),
                        ConfirmationTime = reader.GetDateTime(2),
                        ElementId = reader.IsDBNull(3) ? null : reader.GetString(3),
                        ActionName = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Type = reader.IsDBNull(5) ? null : reader.GetString(5),
                        //CauseOrigin = reader.GetString(6), // reader.IsDBNull(6) ? null : reader.GetString(6),
                        ElementCompanyShortName= reader.IsDBNull(7) ? null : reader.GetString(7),
                        ActionType = CalculationConstants.MHAIA_STR
                    });
                }

                return actions;
            }
            catch (Exception ex)
            {

                throw new Exception( "Error En GetActions - MHAIA - STR" + ex.Message) ;
            }
        }

    }
}
