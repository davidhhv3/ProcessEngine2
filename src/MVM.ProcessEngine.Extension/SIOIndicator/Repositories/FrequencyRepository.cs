using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using MVM.ProcessEngine.Extension.SIOIndicator.Domain;
using Action = MVM.ProcessEngine.Extension.SIOIndicator.Domain.Action;
using MVM.ProcessEngine.Extension.SIOIndicator.Constants;

namespace MVM.ProcessEngine.Extension.SIOIndicator.Repositories
{
    public class FrequencyRepository:SQLRepository
    {
		public FrequencyRepository(string tenant) : base(tenant) { }
		private List<Affectation> affectations = new List<Affectation>();
		public List<Consignment> GetConsignments(DateTime fechaInicioMes, DateTime fechaFinMes, List<string> codigoActivo)
		{
			List<Consignment> consignments = new List<Consignment>();

			SqlDatabase db = new SqlDatabase(ConsignmentOriginConnectionString);

			var sql = @" SELECT Consecutive,c.ElementId,origen.MaintenanceOriginName, 
                        NULL CauseName,	ScheduledStartDate,ScheduledEndDate ,
                       RealStartDate,RealEndDate, NULL penalDate, NULL ActivePenal 
                        , ElementCompanyShortName 
                        FROM [dbo].[Consignments] c 
                        INNER JOIN 
                       (SELECT [value] as MaintenanceOriginName, id from [dbo].[TypeValues]                            
                       ) origen 
                       ON origen.Id = c.MaintenanceOriginId 
					   inner join (SELECT [value] as Cause, id from [dbo].[TypeValues]                           
                      ) cause on c.causeStatusChangeid=cause.Id
					  inner join (SELECT [value] as type, id from [dbo].[TypeValues]                           
                      ) tipo on c.EntryTypeId=tipo.Id
                       where
					   Consecutive is not null
					   and c.ElementId is not null
					   and MaintenanceOriginName not in
					   ('CatastrofesNaturales_O_ActosTerroristas','Expansion',
					   'InstruccionCND','MtoMayor','ObrasEntidadesEstatales_ModificacionesPOT',
					   'Vida humana'
					   )and
					   tipo.type <>'Emergencia'
            ";
			//   "  AND ElementId like @pCodActivo ";

			if (codigoActivo.Count() > 0)
				sql = sql + " AND c.ElementId in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " )";


			DbCommand command = db.GetSqlStringCommand(sql);
			command.CommandTimeout = TimeoutTransaction;

			//db.AddInParameter(command, "@pFechaInicioMes", SqlDbType.DateTime, fechaInicioMes);
			//db.AddInParameter(command, "@pFechaFinMes", SqlDbType.DateTime, fechaFinMes);

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
						IndicatorType = Constants.IndicatorConstants.STR
					});
				}

			}
			catch (Exception ex)
			{

				throw new Exception("Error En HID_STR - met consg " + ex.Message);
			}
			return consignments;
		}
    	public List<SecurityGeneration> GetGenSec(DateTime fechaFinMes)
		{
			List<SecurityGeneration> genSec = new List<SecurityGeneration>();
			var aff = affectations.Select(cng => cng.ConsignmentId);

			SqlDatabase db = new SqlDatabase(ElementOriginConnectionString);

			var sql = @" Select csgID,fecha,periodo01,periodo02
						,periodo03,periodo04,periodo05,periodo06
						,periodo07,periodo08,periodo09,periodo10
						,periodo11,periodo12,periodo13,periodo14
						,periodo15,periodo16,periodo17,periodo18
						,periodo19,periodo20,periodo21,periodo22
						,periodo23,periodo24,getdate()
						from GenSeguridadConsig
						where fecha between DATEADD(MONTH, -1, DATEADD(s, 1, @pFechaFinMes))
						and @pFechaFinMes ";

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
					genSec.Add(new SecurityGeneration()
					{
						Consecutive = reader.GetString(0),
						Date = reader.GetDateTime(1),
						Period01 = reader.IsDBNull(2) ? (Boolean?)null : reader.GetBoolean(2),
						Period02 = reader.IsDBNull(3) ? (Boolean?)null : reader.GetBoolean(3),
						Period03 = reader.IsDBNull(4) ? (Boolean?)null : reader.GetBoolean(4),
						Period04 = reader.IsDBNull(5) ? (Boolean?)null : reader.GetBoolean(5),
						Period05 = reader.IsDBNull(6) ? (Boolean?)null : reader.GetBoolean(6),
						Period06 = reader.IsDBNull(7) ? (Boolean?)null : reader.GetBoolean(7),
						Period07 = reader.IsDBNull(8) ? (Boolean?)null : reader.GetBoolean(8),
						Period08 = reader.IsDBNull(9) ? (Boolean?)null : reader.GetBoolean(9),
						Period09 = reader.IsDBNull(10) ? (Boolean?)null : reader.GetBoolean(10),
						Period10 = reader.IsDBNull(11) ? (Boolean?)null : reader.GetBoolean(11),
						Period11 = reader.IsDBNull(12) ? (Boolean?)null : reader.GetBoolean(12),
						Period12 = reader.IsDBNull(13) ? (Boolean?)null : reader.GetBoolean(13),
						Period13 = reader.IsDBNull(14) ? (Boolean?)null : reader.GetBoolean(14),
						Period14 = reader.IsDBNull(15) ? (Boolean?)null : reader.GetBoolean(15),
						Period15 = reader.IsDBNull(16) ? (Boolean?)null : reader.GetBoolean(16),
						Period16 = reader.IsDBNull(17) ? (Boolean?)null : reader.GetBoolean(17),
						Period17 = reader.IsDBNull(18) ? (Boolean?)null : reader.GetBoolean(18),
						Period18 = reader.IsDBNull(19) ? (Boolean?)null : reader.GetBoolean(19),
						Period19 = reader.IsDBNull(20) ? (Boolean?)null : reader.GetBoolean(20),
						Period20 = reader.IsDBNull(21) ? (Boolean?)null : reader.GetBoolean(21),
						Period21 = reader.IsDBNull(22) ? (Boolean?)null : reader.GetBoolean(22),
						Period22 = reader.IsDBNull(23) ? (Boolean?)null : reader.GetBoolean(23),
						Period23 = reader.IsDBNull(24) ? (Boolean?)null : reader.GetBoolean(24),
						Period24 = reader.IsDBNull(25) ? (Boolean?)null : reader.GetBoolean(25),
						CreationTime = reader.GetDateTime(26)

					});
				}

			}
			catch (Exception ex)
			{

				throw new Exception("Error En HID_STR - met gen sg " + ex.Message);
			}
			genSec = genSec.Where(a => aff.Contains(a.Consecutive)).ToList();

			return genSec;
		}
		public List<Element> GetElementsSTR(DateTime fechaFinMes)
		{
			List<Element> elementList = new List<Element>();

			SqlDatabase db = new SqlDatabase(ElementOriginConnectionString);
			var sql = @"With ElementList as ( 
							select b1.objID, b1.objID1 as SubSistema,  
                            CASE 
                               WHEN Anillo.ObjID1 is not null then Anillo.ObjID1 
                               ELSE b1.objID2 
                              END AS Agregador, 
                            CASE  
                               WHEN Anillo.ObjID1 is not null then Anillo.ObjID2 
                               ELSE b1.objID2 
                              END AS Elemento, 
                            CASE 
                               WHEN Anillo.ObjID1 is not null then 'Anillo' 
							   WHEN uc.objid is not null THEN 'Int. y Medio' 
                               ELSE Null 
                              END AS Tipo,  
							CASE WHEN Anillo.UC is not null then Anillo.UC
							   ELSE 'NA'
							END UC,
                            CASE 
                               WHEN Col96.subconjunto is null or Col96.subconjunto = 'NoCompensa' then 'NoCompensa' 
                               ELSE 'Compensa' 
                              END AS Compensa, 
                            CASE 
                             WHEN Col96.subconjunto = 'CorteCentral' then CAST(1 AS BIT) 
                             END AS IsCenterCut 
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
									INNER JOIN Maestrocol col ON col.objid = Inte.objid2 AND col.subconjunto <> 'NoCompensa' --AND col.colid = 'Col0096'
									AND(@pFechaFinMes >= col.fechaIni) AND(col.fechaFin IS NULL OR col.fechaFin > @pFechaFinMes)
									INNER JOIN Maestrocol col1 ON col1.objid = Inte.objid1 AND col1.subconjunto <> 'NoCompensa' --AND col1.colid = 'Col0096'
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
                            left join[dbo].[MaestroCol] Col96 on Col96.objID = b1.ObjID2 --and colID = 'Col0096' 
                            AND(@pFechaFinMes >= Col96.fechaIni) AND(Col96.fechaFin IS NULL OR Col96.fechaFin > @pFechaFinMes) 
                            where b1.objid = 'Bir0051' 
                            and b1.objID2 not in ( 
                                                select DISTINCT Inte.OBJID2												
												from birrelacion Inte
												INNER JOIN Maestrocol col ON col.objid = Inte.objid2 AND col.subconjunto <> 'NoCompensa' --AND col.colid = 'Col0096'
												AND(@pFechaFinMes >= col.fechaIni) AND(col.fechaFin IS NULL OR col.fechaFin > @pFechaFinMes)
												INNER JOIN Maestrocol col1 ON col1.objid = Inte.objid1 AND col1.subconjunto <> 'NoCompensa' --AND col1.colid = 'Col0096'
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
												WHERE cc.subconjunto = 'CorteCentral' --AND cc.ColId = 'Col0096'
												AND(@pFechaFinMes >= cc.fechaIni) AND(cc.fechaFin IS NULL OR cc.fechaFin > @pFechaFinMes) 
												AND(@pFechaFinMes >= Bah.fechaIni) AND(Bah.fechaFin IS NULL OR Bah.fechaFin > @pFechaFinMes) 
												AND(@pFechaFinMes >= tBah.fechaIni) AND(tBah.fechaFin IS NULL OR tBah.fechaFin > @pFechaFinMes) 
												AND(@pFechaFinMes >= Unc.fechaIni) AND(Unc.fechaFin IS NULL OR Unc.fechaFin > @pFechaFinMes) 
												AND(@pFechaFinMes >= Def.fechaIni) AND(Def.fechaFin IS NULL OR Def.fechaFin > @pFechaFinMes) 
                             				) 
											AND(@pFechaFinMes >= b1.fechaIni) AND(b1.fechaFin IS NULL OR b1.fechaFin > @pFechaFinMes) 
                            ), 
						   Result as (
                           select distinct e.Subsistema as SubsystemId,agregador as AggregatorId,e.Elemento as ElementId, 
                           isnull(e.tipo, 'SinCon') as ElementType,isnull(uc, 'NA') as UCtype,compensa as Compensate--, MHAI 
                           ,l.longitud as SegmentLength 
                            from ElementList e                            
                            left join linea l on l.objID=e.Elemento AND(@pFechaFinMes >= l.fechaIni) AND(l.fechaFin IS NULL OR l.fechaFin > @pFechaFinMes) 
                            Where Compensa='Compensa'
						   )	 ,
						   Q1  as
							(
								select distinct b.objID as Bir,b.objID1 as Rel1,b.objID2 as Rel2, 
								mm.ultnombre as NomRel1,m.ultnombre as NomRel2
								from birrelacion  b	
								inner join maestroobj m on b.objid2 =m.objid
								inner join maestroobj mm on b.objid1 =mm.objid
								where	b.objid in ('Bir0028','Bir0506')
								and (objid2 like '%Bah%')
								AND(@pFechaFinMes >=b.fechaIni) AND(b.fechaFin IS NULL OR b.fechaFin > @pFechaFinMes)
	
							), 
							Bahias as (
							select distinct p1.*, mcb.subconjunto from Q1 P1
							inner join maestrocol mc on p1.rel1=mc.objid
							inner join maestrocol mcb on p1.rel2=mcb.objid
							where (p1.rel1 like '%Trf%')
							and mc.colid='Col0096' 
							and mcb.colid in ('Col0091') 
							AND(@pFechaFinMes >= mcb.fechaIni) AND(mcb.fechaFin IS NULL OR mcb.fechaFin > @pFechaFinMes)
							AND(@pFechaFinMes >= mc.fechaIni) AND(mc.fechaFin IS NULL OR mc.fechaFin > @pFechaFinMes)
							),
							resultSTN as (
							select bh.rel1 SubsystemId,bh.nomRel1 SubsystemName,bh.Rel1 UCType, 
							b.objid1 AggregatorId,m.ultnombre AggregatorName,bh.Rel2  elementID,bh.NomRel2 as ElementName
							from bahias bh
							inner join birrelacion  b on bh.rel2=b.objid2
							inner join maestroobj m on b.objid1 =m.objid
							where b.objid='Bir0027'
							--and b.objid1='Sub0179'
							AND(@pFechaFinMes >=b.fechaIni) AND(b.fechaFin IS NULL OR b.fechaFin > @pFechaFinMes)
							),
							CasoSTN as (
								select rr.aggregatorid ,r.elementid
								,case when count(rr.aggregatorid)=1 then 'SinCon' else 'Int. y Medio' end as ElementType
								,r.subsystemid,'CasoSTN' caso,'FREC_STN'IndicatorType
								,r.AggregatorName,r.ElementName,r.SubsystemName
								from resultSTN rr
								inner join resultSTN r on rr.AggregatorId=r.AggregatorId
								and rr.subsystemid=r.subsystemid								
								group by rr.aggregatorid,r.subsystemid,r.elementid,r.UCType,r.SubsystemName,r.AggregatorName,r.ElementName
							)													
						   ,						   
						   Caso1 as (
								select aggregatorid,elementid,ElementType,bh.linea UCType, 'Caso1' Compensate,'FREC_STR' IndicatorType 
								from result r
								inner join (
										select b.objid1 linea,objid2 bahia 
										from birrelacion b
										inner join MaestroCol mc on b.objID1=mc.objid
										where b.objid='Bir0028'
										and mc.colid='Col0096'

										AND(@pFechaFinMes >= b.fechaIni) AND(b.fechaFin IS NULL OR b.fechaFin > @pFechaFinMes) 
										AND(@pFechaFinMes >= mc.fechaIni) AND(mc.fechaFin IS NULL OR mc.fechaFin > @pFechaFinMes) 
										and (b.objid1 like 'Lin%'  or b.objid1 like 'Trf%')	
																				
								)bh on r.elementid=bh.bahia
								where ElementType not in ('Anillo','Int. y Medio')
								and ElementId not in (select ElementId from CasoSTN where ElementType = 'Int. y Medio')
								)
							
								,
						   caso2 as (
								select aggregatorid,elementid,ElementType,bh.linea UCType, 'Caso2' Compensate,'FREC_STR' IndicatorType 
								from result r
								inner join (
										select objid1 linea,objid2 bahia 
										from birrelacion b
										inner join result r on b.objid2=r.AggregatorId
										inner join MaestroCol mc on b.objID1=mc.objid
										where ElementType in ('Anillo')
										and b.objid='Bir0028'
										and mc.colid='Col0096'
										AND(@pFechaFinMes >= b.fechaIni) AND(b.fechaFin IS NULL OR b.fechaFin > @pFechaFinMes) 
										and (objid1 like 'Lin%' or objid1 like 'Trf%' )
										
								)bh on r.AggregatorId=bh.bahia
								
									 
							 )
						   ,
						   caso3a as (
									select b2.objid1 LinTrf, uc.CorteCentral as aggregadorID, uc.objid elementid
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
										inner join birrelacion b2  on  uc.objid=b2.objid2
										where b1.objid = 'Bir0051'
											  AND(@pFechaFinMes >= b1.fechaIni) AND(b1.fechaFin IS NULL OR b1.fechaFin > @pFechaFinMes) 
											  and uc.objid is not null
											  and b2.objid ='Bir0028'
											  and (b2.objid1 like ('Lin%') or b2.objid1 like ('Trf%') )

						   ),caso3 as (
								select LinTrf aggregatorid,aggregadorID elementid,'Int. y Medio' ElementType,LinTrf UCType,'Caso3' Compensate,'FREC_STR' IndicatorType  from caso3a
								union
								select LinTrf aggregatorid,elementid,'Int. y Medio' ElementType,LinTrf UCType,'Caso3' Compensate,'FREC_STR' IndicatorType  from caso3a
								
						   )			   
						  , Res as
						   (
						   select distinct r.*,ag.ultNombre agg ,ee.ultNombre elem ,lin.ultNombre lin 
						    from caso1 r
						    left join MaestroObj ag on r.AggregatorId = ag.objID 
                            left join MaestroObj ee on r.ElementId = ee.objID 
							left join MaestroObj lin on r.uctype = lin.objID 
							
							union 
							select distinct r.*,ag.ultNombre agg ,ee.ultNombre elem ,lin.ultNombre lin
							 from caso2 r	
							  left join MaestroObj ag on r.AggregatorId = ag.objID 
                            left join MaestroObj ee on r.ElementId = ee.objID 
							left join MaestroObj lin on r.uctype = lin.objID 
							
							union
						   select distinct r.*,ag.ultNombre agg ,ee.ultNombre elem ,lin.ultNombre lin 
						    from caso3 r
						    left join MaestroObj ag on r.AggregatorId = ag.objID 
                            left join MaestroObj ee on r.ElementId = ee.objID 
							left join MaestroObj lin on r.uctype = lin.objID 
							
							union 
							select * from CasoSTN							
							)
							select UCType,aggregatorid,ElementId, ElementType,'FREC_STR'IndicatorType,
							lin as subsystemName,agg as agregatorName,elem as elementName  
							from res
                      ";

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
					IndicatorType= reader.GetString(4),
					SubsystemName = reader.IsDBNull(5) ? null : reader.GetString(5),
					AggregatorName = reader.IsDBNull(6) ? null : reader.GetString(6),
					ElementName = reader.IsDBNull(7) ? null : reader.GetString(7)

				});
				;
			}

			return elementList;
		}

		public List<Element> GetElementsSTN(DateTime fechaFinMes)
		{
			List<Element> elementList = new List<Element>();

			SqlDatabase db = new SqlDatabase(ElementOriginConnectionString);
			var sql = @"with Q1  as
							(
								select distinct b.objID as Bir,b.objID1 as Rel1,b.objID2 as Rel2, 
								mm.ultnombre as NomRel1,m.ultnombre as NomRel2
								from birrelacion  b	
								inner join maestroobj m on b.objid2 =m.objid
								inner join maestroobj mm on b.objid1 =mm.objid
								where	b.objid in ('Bir0028','Bir0506')
								and (objid2 like '%Bah%')
								AND(@pFechaFinMes >=b.fechaIni) AND(b.fechaFin IS NULL OR b.fechaFin > @pFechaFinMes)
	
							), 
							Bahias as (
							select distinct p1.*, mcb.subconjunto from Q1 P1
							inner join maestrocol mc on p1.rel1=mc.objid
							inner join maestrocol mcb on p1.rel2=mcb.objid
							where (p1.rel1 like '%Trf%' or p1.rel1 like '%Lin%')
							and mc.colid='Col0091' 
							and mcb.colid in ('Col0091','Col0096') 
							AND(@pFechaFinMes >= mcb.fechaIni) AND(mcb.fechaFin IS NULL OR mcb.fechaFin > @pFechaFinMes)
							AND(@pFechaFinMes >= mc.fechaIni) AND(mc.fechaFin IS NULL OR mc.fechaFin > @pFechaFinMes)
							),
							result as (
							select bh.rel1 SubsystemId,bh.nomRel1 SubsystemName,bh.Rel1 UCType, 
							b.objid1 AggregatorId,m.ultnombre AggregatorName,bh.Rel2  elementID,bh.NomRel2 as ElementName
							from bahias bh
							inner join birrelacion  b on bh.rel2=b.objid2
							inner join maestroobj m on b.objid1 =m.objid
							where b.objid='Bir0027'
							AND(@pFechaFinMes >=b.fechaIni) AND(b.fechaFin IS NULL OR b.fechaFin > @pFechaFinMes)

							)--select * from result
							select r.subsystemid,rr.aggregatorid ,r.elementid
							,case when count(rr.aggregatorid)=1 then 'SinCon' else 'Int. y Medio' end as ElementType
							,'FREC_STN'IndicatorType
							,r.SubsystemName,r.AggregatorName,r.ElementName
							from result rr
							inner join result r on rr.AggregatorId=r.AggregatorId
							and rr.subsystemid=r.subsystemid
							group by rr.aggregatorid,r.subsystemid,r.elementid,r.UCType,r.SubsystemName,r.AggregatorName,r.ElementName

                      ";

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
					IndicatorType = reader.GetString(4),
					SubsystemName = reader.IsDBNull(5) ? null : reader.GetString(5),
					AggregatorName = reader.IsDBNull(6) ? null : reader.GetString(6),
					ElementName = reader.IsDBNull(7) ? null : reader.GetString(7)

				});
				;
			}

			return elementList;
		}

		public List<Action> GetActions(DateTime fechaFinMes, string IndicatorType)
		{
			fechaFinMes = new DateTime(fechaFinMes.Year, fechaFinMes.Month, 1).AddSeconds(-1);

			List<Element> elements=new List<Element>();
			if (IndicatorType == "STR")
				elements = GetElementsSTR(fechaFinMes);
			else
				elements = GetElementsSTN(fechaFinMes);
			

			var ele = elements.Select(e => e.ElementId).Union(elements.Select(e => e.SubsystemId));
			List<Action> actions = new List<Action>();

			SqlDatabase db = new SqlDatabase(ManeuverOriginConnectionString);

			var sql = @"WITH mov AS (
                            SELECT m
                            FROM (SELECT 'Disponible' M
                                  UNION SELECT 'Indisponible' M
								  UNION SELECT 'No Operativo' M
								  UNION SELECT 'Abrir' M
								  UNION SELECT 'No Operativo' M
								  UNION SELECT 'Evento No Programado Otro Sistema' M
								  UNION SELECT 'STN' M
								  UNION SELECT 'Evento' M
								  ) MM
                            )
	                        /*select * from mov*/
                            ,Tipos AS( 
		                        SELECT tv.id, tv.Value valor, t.Name AS tipo FROM [dbo].[TypeValues] Tv 
		                        INNER JOIN[dbo].[Types] T on tv.TypeId= t.Id 
                            ), 
							/*select * from tipos  */
						   estados AS( 
                           SELECT id FROM[dbo].[TypeValues] 
                           WHERE typeid in 
                           ( 
                           SELECT[Id] 
                           FROM [dbo].[Types] 
                           WHERE NAME  LIKE '%Estado de Acciones de Maniobra%' 
                           ) 
                           and VALUE IN('Validada') 
						   ), 
                            EventList AS( 
			                        
                                SELECT Id, ElementId, ConsignmentId, NewAvailability, causeoperationalid, ActionTypeId, InstructionTime, OccurrenceTime , ConfirmationTime, ScheduledStartDate, EspElementId, CauseOrigin, FuelCEN , PlantCEN,ElementCausingId,ElementCompanyShortName
                                FROM [dbo].[ActionsAgents] a 
                                WHERE (a.OccurrenceTime  > dateadd(MONTH,-12, @pFechaFinMes)  
								and a.OccurrenceTime  <= @pFechaFinMes) 
								AND StatustypeID IN(SELECT * FROM estados) 
							)
	                        /*select * from EventList 
							where ElementId='Bah0658'
							*/
	                        ,EventListValues AS( 
		                        SELECT e.Id, e.ElementId, e.ConsignmentId, e.NewAvailability, e.causeoperationalid, e.ActionTypeId, e.InstructionTime, e.OccurrenceTime , e.ConfirmationTime, 
			                        e.ScheduledStartDate ,causa.valor AS [type], causa.tipo ActionName, tipoMovimiento.valor AS movimiento , tipoMovimiento.tipo tipoMovimiento, EspElementId, CauseOrigin , FuelCEN , PlantCEN,ElementCausingId,ElementCompanyShortName
                                ,case when tipoMovimiento.tipo in ('Abrir','Indisponible')
												 and tipoMovimiento.tipo not in 
												 ('Forzado','Catástrofe Natural','Actos terroristas',
												  'STN','Evento No programado Otro Sistema')
												 then 0 else 1 end excl
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
                                FROM EventListValues ev 
								where movimiento in (SELECT m FROM mov)
								and excl=1 
								     
								)
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
		                        select eventId, InstructionTime, OccurrenceTime, ConfirmationTime, ElementId, ActionName, [type], ConsignmentId, NewAvailability, 
										movement, typeMovement, EndOccurrenceTime, ScheduledStartDate, ScheduledEndDate,ValidFrom, ValidTo, CauseOrigin , FuelCEN , PlantCEN,ElementCausingId
										,ElementCompanyShortName 
								from Result 
								where elementid is not null
";		
			

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
						ConsignmentId = reader.IsDBNull(7) ? null : reader.GetString(7),
						NewAvailability = reader.IsDBNull(8) ? (decimal?)null : reader.GetDecimal(8),
						Movement = reader.IsDBNull(9) ? null : reader.GetString(9),
						TypeMovement = reader.IsDBNull(10) ? null : reader.GetString(10),
						EndOccurrenceTime = reader.IsDBNull(11) ? (DateTime?)null : reader.GetDateTime(11),
						ScheduledStartDate = reader.IsDBNull(12) ? (DateTime?)null : reader.GetDateTime(12),
						ScheduledEndDate =reader.IsDBNull(13) ? (DateTime?)null : reader.GetDateTime(13),
						ValidFrom = reader.IsDBNull(14) ? (DateTime?)null : reader.GetDateTime(14),
						ValidTo = reader.IsDBNull(15) ? (DateTime?)null : reader.GetDateTime(15),
						CauseOrigin = reader.IsDBNull(16) ? null : reader.GetString(16),
						ElementCausingId = reader.IsDBNull(19) ? null : reader.GetString(19),
						ElementCompanyShortName = reader.IsDBNull(20) ? null : reader.GetString(20),
						ActionType = IndicatorType=="STR"?CalculationConstants.FREC_STR: CalculationConstants.FREC_STN

					});
				}
				actions = actions.Where(act => ele.Contains(act.ElementId)).ToList();

			}

			catch (Exception ex)
			{

				throw new Exception("Error En frequency - actions - " + ex.Message);
			}

			return actions;
		}

	}
}
