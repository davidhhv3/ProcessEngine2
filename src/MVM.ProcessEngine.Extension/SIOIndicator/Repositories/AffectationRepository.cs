using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using MVM.ProcessEngine.Extension.SIOIndicator.Domain;
using Affectation = MVM.ProcessEngine.Extension.SIOIndicator.Domain.Affectation;

namespace MVM.ProcessEngine.Extension.SIOIndicator.Repositories
{
    public class AffectationRepository:SQLRepository
    {
		public AffectationRepository(string tenant) : base(tenant) { }
		private List<Affectation> affectations = new List<Affectation>();
		public List<Affectation> GetAffectation(DateTime fechaFinMes, List<string> codigoActivo,string indicatorType)
		{
			

			var StartDate = fechaFinMes.AddSeconds(1).AddMonths(-1);
			var consignments = GetConsignments(StartDate, fechaFinMes, codigoActivo);
			var elements = GetElements(indicatorType);
			var csg = consignments.Select(cng => cng.Consecutive);
			var ele = elements.Select(e => e.ElementId).Union(elements.Select(e=>e.AggregatorId)).ToList();
			var BarYuc = elements.Where(e => e.UCtype == "UC15" || e.ElementType == "Barras" || e.ElementType == "Lineas en T").Select(e => e.ElementId);
			SqlDatabase db = new SqlDatabase(ManeuverOriginConnectionString);

			var sql = @" select id,ConsignmentId,IncomeType
					  ,Origin,AffectationTypeId
					  ,MainElementId,MainElementName
					  ,AffectationElementId,AffectationElementName
					  ,ProgrammedStartedDate,ProgrammedFinishedDate
					  , case when FinalConsignmentStatus='3082E1C5-B7C1-4E8C-8552-5997A1D4F4E9' then 'Reprogramada' 
							 when FinalConsignmentStatus='963D6FA7-9CC8-48DD-9032-D1C4E641E5EE' then 'Cancelada' 
							 end FinalConsignmentStatus
					  ,case when CauseStatusChangeId='7E3C24FC-2356-4325-8A78-88C043723AD9' then 'Causa relacionada al agente' end CauseStatusChangeId
					  from dbo.AffectationsTimes
					  where (ProgrammedStartedDate between DATEADD(MONTH, -1, DATEADD(s, 1, @pFechaFinMes)) and @pFechaFinMes 
					        or ProgrammedFinishedDate between DATEADD(MONTH, -1, DATEADD(s, 1, @pFechaFinMes)) and @pFechaFinMes 
							)
					  and FinalConsignmentStatus in('963D6FA7-9CC8-48DD-9032-D1C4E641E5EE','3082E1C5-B7C1-4E8C-8552-5997A1D4F4E9')
				      and CauseStatusChangeId='7E3C24FC-2356-4325-8A78-88C043723AD9'
					  and AffectationTypeId in ('RD','AP','D','DA','DD','DP'
												,'Desenergizado'
												,'DesenergizadoAterrizado'
												,'DesenergizadoDespejado'
												,'DesenergizadoParcial'
												)
					  and origin in ('Normal'
									,'CatastrofesNaturales_O_ActosTerroristas'
									,'Expansion'
									,'InstruccionCND'
									,'MtoMayor' 
									,'ObrasEntidadesEstatales_ModificacionesPOT'
									, 'Vida humana'
					     			)

					";
			if (codigoActivo.Count() > 0)
			{
				var listAct = elements.Where(e=> codigoActivo.Contains(e.ElementId)).Select(e => e.ElementId).Union(elements.Where(e => codigoActivo.Contains(e.ElementId)).Select(e => e.AggregatorId)).ToList();
				sql = sql + " and MainElementId in ( " + string.Concat("'", string.Join("','", listAct), "'") + " )";
			}


			DbCommand command = db.GetSqlStringCommand(sql);
			command.CommandTimeout = TimeoutTransaction;
			db.AddInParameter(command, "@pFechaFinMes", SqlDbType.DateTime, fechaFinMes);

			var reader = db.ExecuteReader(command);

			while (reader.Read())
			{
				affectations.Add(new Affectation()
				{
					Id = reader.GetGuid(0),
					ConsignmentId = reader.GetString(1),
					IncomeType = reader.GetString(2),
					Origin = reader.GetString(3),
					AffectationTypeId= reader.GetString(4),
					MainElementId= reader.GetString(5),
					MainElementName = reader.GetString(6),
					AffectationElementId = reader.GetString(7),
					AffectationElementName = reader.GetString(8),
					ProgrammedStartedDate = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9),
					ProgrammedFinishedDate= reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
					FinalConsignmentStatus= reader.GetString(11)
				});
			}

			affectations = affectations.Where(aff => csg.Contains(aff.ConsignmentId)).ToList();
			affectations = affectations.Where(aff => ele.Contains(aff.AffectationElementId)).ToList();
			affectations = affectations.Where(aff => aff.MainElementId == aff.MainElementId || BarYuc.Contains(aff.AffectationElementId) || BarYuc.Contains(aff.MainElementId)).ToList();

			return affectations;
		}

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
			genSec= genSec.Where(a => aff.Contains(a.Consecutive)).ToList();

			return genSec;
		}
		public List<Element> GetElements(string indicatorType)
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

	}
}
