using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using MVM.ProcessEngine.Extension.SIOIndicator.Constants;
using MVM.ProcessEngine.Extension.SIOIndicator.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Action = MVM.ProcessEngine.Extension.SIOIndicator.Domain.Action;

namespace MVM.ProcessEngine.Extension.SIOIndicator.Repositories
{
    public class MHAIA_STNRepository : SQLRepository
    {
        public MHAIA_STNRepository(string tenant) : base(tenant) { }

        public List<Element> GetElements(DateTime fechaFinMes, List<string> codigoActivo)
        {
            List<Element> elementList = new List<Element>();

            SqlDatabase db = new SqlDatabase(ElementOriginConnectionString);
            var sql = @"with ElementList as (
	                        SELECT distinct t0.objID, 
		                        b51.objID1 as SubSistema, 
		                        CASE
			                        WHEN LineasT.LinT is not null THEN 'T_' + b51.objID1 
			                        ELSE b51.objID2 
		                        END AS Agregador,
		                        CASE  
			                        WHEN Barras.objid1 is not null THEN Barras.objid1 
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
                            WHERE b.objID = 'Bir0251' and c.unidades='Horas'
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
	                        ) encapsulado on e.objID = encapsulado.elemento 
                            inner join constante c ON c.defid = CASE WHEN encapsulado.elemento IS NULL THEN 'Def0111' ELSE 'Def0236' end 
                                AND(@pFechaFinMes >= c.fechaIni) AND(c.fechaFin IS NULL OR c.fechaFin > @pFechaFinMes) 
                        )
                        select distinct isnull(e.Subsistema, e.Elemento) as SubsystemId, isnull(agregador, e.Elemento) as AggregatorId, e.Elemento as ElementId, 
	                        isnull(e.tipo, 'SinConf') as ElementType, isnull(uc, 'NA') as UCtype, compensa as Compensate, isnull(M.MHAI, MA.MHAI) MHAI 
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
                            ,b.objID1  companyId
							,c.nombre companyName
							,c.sigla as shortName
                        from ElementList e 
                        left join MaestroObj ms on e.Subsistema = ms.objID 
                        left join MaestroObj ag on e.agregador = ag.objID 
                        left join MaestroObj ee on e.Elemento = ee.objID 
                        left JOIN MHAI M ON M.subsistema = e.Elemento 
						left JOIN MHAI MA ON MA.subsistema = e.Agregador and e.Agregador like 'Beq%'
                        Left JOIN TiempoManiobra T ON t.elemento = e.Elemento 
                        left join encapsulado encapsulado on e.Elemento = encapsulado.Elemento 
                        left join linea l on l.objID=e.Elemento AND(@pFechaFinMes >= l.fechaIni) AND(l.fechaFin IS NULL OR l.fechaFin > @pFechaFinMes)
                        left join 
									(
									select * from Birrelacion where objID = 'Bir0014'
									AND(@pFechaFinMes >= fechaIni) AND(fechaFin IS NULL OR fechaFin > @pFechaFinMes)		
									)  b on B.objid2=e.elemento or B.objid2=e.Agregador
						   left join (
									select * from  Compania  
									where (@pFechaFinMes >= fechaIni) AND(fechaFin IS NULL OR fechaFin > @pFechaFinMes) 	
							 )c on c.objID=b.objID1
";

            if (codigoActivo.Count() > 0)
            {
                sql = sql + " where e.elemento in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " )";
                sql = sql + " OR agregador in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " )";

            }
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
                    OperatorCompanyId = reader.IsDBNull(19) ? null : reader.GetString(19),
                    OperatorCompanyName = reader.IsDBNull(20) ? null : reader.GetString(20),
                    OperatorCompanyShortName = reader.IsDBNull(21) ? null : reader.GetString(21),
                    IndicatorType = IndicatorConstants.STN
                });
                ;
            }
            //elementList.AddRange(batteriesList);

            return elementList;
        }


        //public async Task<List<Element>> GetBatteries(DateTime fechaFinMes, List<string> codigoActivo)
        public async Task<List<Element>> GetBatteries(DateTime fechaFinMes, List<string> codigoActivo)
        {
            int MHAIValue = GetMhai();
            var client = new WebClient();
            await GetToken(client);
            string url = UrlBatteries;
            string jsonBody = @"{
                   ""onlyParameterNamesBasic"": [
                       ""ElementId"",
                       ""ElementMRID"",
                       ""ScadaCode"",
                       ""ElementName"",
                       ""ValidFrom"",
                       ""*""
                   ],
                   ""onlyParameterNamesBattery"": [
                       ""SaebMRID"",
                       ""MinimumPercentCharge"",
                       ""MaximumChargeSpeed"",
                       ""AdjustedChargeSpeed"",
                       ""AdjustedDischargeSpeed"",
                       ""*""
                   ],
                   ""onlyParameterNamesXMBatteryUnit"": [
                       ""*""
                    ],
                    ""filterBasicRequest"": {
                       ""isActive"": true
                    }
                }";
            client.Headers[HttpRequestHeader.ContentType] = "application/json";

            string response = client.UploadString(url, "POST", jsonBody);

            BatteryData batteryData = JsonConvert.DeserializeObject<BatteryData>(response);
            List<Element> batteriesList = new List<Element>();
            if (batteryData != null && batteryData.Items != null)
            {
                foreach (var item in batteryData.Items)
                {
                    if (item.BasicAttributes.TransmissionSystemTypeName == null)
                        item.BasicAttributes.TransmissionSystemTypeName = string.Empty;
                    //if (item.BasicAttributes.TransmissionSystemTypeName.ToString() == "Uso STN")
                    //{
                    string subsystemId = item?.BasicAttributes?.CodeMdc.ToString();
                    string subsystemName = item?.BasicAttributes?.ElementName?.ToString();
                    Element element = new Element();
                    List<string> SubsistmeIdName = await GetSubsystems(item.BasicAttributes.ElementMRID);//Intermedio Mhaia
                    if (SubsistmeIdName.Count == 0)
                    {
                        element = await CreateBatery(subsystemId, subsystemName, item, MHAIValue);
                        batteriesList.Add(element);
                    }
                    else
                    {
                        for (int i = 0; i < SubsistmeIdName.Count; i += 2)
                        {
                            subsystemId = SubsistmeIdName[i];
                            subsystemName = SubsistmeIdName[i + 1];
                            element = await CreateBatery(subsystemId, subsystemName, item, MHAIValue);
                            batteriesList.Add(element);
                        }
                    }
                    //}
                }
            }
            return batteriesList;
        }
        public async Task<Element> CreateBatery(string subsystemId, string subsystemName, Battery item, int MHAIValue)
        {
            string CompanyShortName = await GetCompanyShortName(item?.BasicAttributes?.OperatorCompanyCode?.ToString());
            string Compensate = GetCompensate(item?.BasicAttributes?.CodeMdc.ToString());
            double? ManeuverTime = GetManeuverTime(item?.BasicAttributes?.CodeMdc.ToString());
            Element element = new Element();
            element.SubsystemId = subsystemId;
            element.AggregatorId = item?.BasicAttributes?.CodeMdc.ToString();
            element.ElementId = item?.BasicAttributes?.CodeMdc.ToString();
            element.ElementType = item?.BasicAttributes?.ElementType?.ToString();
            element.UCtype = "SinConf";
            element.Compensate = Compensate;
            element.MHAI = MHAIValue;
            element.MHAIAUnit = "Horas";
            element.ManeuverTime = ManeuverTime;
            element.ManeuverUnit = null;
            element.MaintenanceTime = null;
            element.MaintenanceUnit = null;// ??????   
            element.MaintenanceStartDate = null;
            element.MaintenanceEndDate = null;
            element.IsCenterCut = null;
            element.SegmentLength = null;
            element.SubsystemName = subsystemName;
            element.AggregatorName = item?.BasicAttributes?.ElementName?.ToString();
            element.ElementName = item?.BasicAttributes?.ElementName?.ToString();
            element.OperatorCompanyId = item?.BasicAttributes?.OperatorCompanyCode?.ToString();
            element.OperatorCompanyName = item?.BasicAttributes?.OperatorCompanyName?.ToString();
            element.OperatorCompanyShortName = CompanyShortName;
            element.IndicatorType = item?.BasicAttributes?.TransmissionSystemTypeName?.ToString() == "Uso STR" ? Constants.IndicatorConstants.STR : Constants.IndicatorConstants.STN;
            return element;
        }
        public async Task<List<string>> GetSubsystems(int elementMRID)
        {
            try
            {
                List<string> SubsistmeIdName = new List<string>(); var client = new WebClient();
                await GetToken(client); int limit = 2;
                int numberPage = 1;
                int pageCount = 1; while (numberPage <= pageCount)
                {
                    SubsystemAllData subsystemAllData = await GetSubsystemsByPagination(limit, numberPage, client);
                    foreach (var item in subsystemAllData.items)
                    {
                        SubsystemData subsystemData = await GetSubsystem(item.basicAttributes.GroupMRID.ToString()); List<string> SubsistmeIdNameResult = await IsBatterySubsystem(subsystemData, elementMRID);//
                        if (SubsistmeIdNameResult.Count > 0)
                            SubsistmeIdName.AddRange(SubsistmeIdNameResult);
                    }
                    if (pageCount == 1)
                        pageCount = subsystemAllData.metadata.pagination.pageCount;
                    numberPage++;
                }
                return SubsistmeIdName;
            }
            catch (Exception ex)
            {
                throw new Exception("Error En GetElements - MHAIA - STN" + ex.Message);
            }
        }
        public async Task<SubsystemAllData> GetSubsystemsByPagination(int limit, int numberPage, WebClient client)
        {
            string companyUrl = UrlGroups + "GetAll/";
            string companyEndpoint = $"{companyUrl}?limit={limit}&numberPage={numberPage}"; var requestData = new
            {
                onlyParameterNamesBasic = new string[] { "*" },
                onlyParameterNamesGroup = new string[] { "*" },
                filterGroupRequest = new
                {
                    enumGroupMRID = 2
                },
                filterSubsystemGroupRequest = new
                {
                    SubsystemType = 499
                }
            };
            client.Headers[HttpRequestHeader.ContentType] = "application/json";
            string jsonBody = JsonConvert.SerializeObject(requestData);
            string response = client.UploadString(companyEndpoint, "POST", jsonBody);
            SubsystemAllData subsystemAllData = JsonConvert.DeserializeObject<SubsystemAllData>(response);//*****
            return subsystemAllData;
        }
        public async Task<SubsystemData> GetSubsystem(string groupMRID)
        {
            var client = new WebClient();
            await GetToken(client);
            string batteryEndpoint = UrlGroups + groupMRID;
            string response = client.DownloadString(batteryEndpoint);
            SubsystemData subsystemData = JsonConvert.DeserializeObject<SubsystemData>(response);
            return subsystemData;
        }
        public async Task<List<string>> IsBatterySubsystem(SubsystemData subsystemData, int elementMRID)
        {
            List<string> SubsistmeIdName = new List<string>();
            foreach (var XmElement in subsystemData.XmElementGroup)
            {
                if (XmElement.ElementType == "BaterÃ­a SAEB")
                {
                    if (elementMRID == XmElement.ElementMRID)
                    {
                        SubsistmeIdName.Add(subsystemData.BasicAttributes.CodeMDC);
                        SubsistmeIdName.Add(subsystemData.BasicAttributes.GroupName);
                    }
                }
            }
            return SubsistmeIdName;
        }


        public int GetMhai()
        {
            try
            {
                int mhai = 0;
                SqlDatabase db = new SqlDatabase(DestinationConnectionString);
                var sql = @" select Value from [dbo].[SAEBgoals] where Name= 'MHAIP'"; using (DbCommand cmd = db.GetSqlStringCommand(sql))
                {
                    object result = db.ExecuteScalar(cmd);
                    if (result != null && result != DBNull.Value)
                    {
                        mhai = Convert.ToInt32(result);
                    }
                }
                return mhai;
            }
            catch (Exception ex)
            {
                throw new Exception("Error En GetElements - MHAIA - STN" + ex.Message);
            }
        }
        public string GetCompensate(string elementId)
        {
            string Compensate = "";
            try
            {
                SqlDatabase db = new SqlDatabase(ManeuverOriginConnectionString); 
                var sql = $"select Compensation from [dbo].[SettingSAEBs] where ElementId = '{elementId}'"; using (DbCommand cmd = db.GetSqlStringCommand(sql))
                {
                    object result = db.ExecuteScalar(cmd);
                    if (result != null && result != DBNull.Value)
                    {
                        Compensate = Convert.ToString(result);
                    }
                }
                if (Compensate == "True")
                    Compensate = "Compensa";
                else
                    Compensate = "No Compensa";
                return Compensate;
            }
            catch (Exception ex)
            {
                throw new Exception("Error En SettingSAEBsConnect - MHAIA - STN" + ex.Message);
            }
        }
        public double? GetManeuverTime(string elementId)
        {
            double ManeuverTime = 0;
            try
            {
                SqlDatabase db = new SqlDatabase(ManeuverOriginConnectionString);
                var sql = $"select ManeuverTime from [dbo].[SettingSAEBs] where ElementId = '{elementId}'"; using (DbCommand cmd = db.GetSqlStringCommand(sql))
                {
                    object result = db.ExecuteScalar(cmd);
                    if (result != null && result != DBNull.Value)
                        return ManeuverTime = Convert.ToDouble(result);
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error En SettingSAEBsConnect - MHAIA - STN" + ex.Message);
            }
        }

        public async Task<string> GetCompanyShortName(string OperatorCompanyId)
        {
            var client = new WebClient();
            await GetToken(client); int limit = 50;
            int numberPage = 1; string companyShortName = "";
            while (companyShortName == "" || companyShortName == "Compañía no encontrada")
            {
                companyShortName = await getCompanyData(OperatorCompanyId, limit, numberPage, client);
                numberPage++;
            }
            if (companyShortName == "Excedió límite de páginas")
                return ""; return companyShortName;
        }

        async Task<string> getCompanyData(string OperatorCompanyId, int limit, int numberPage, WebClient client)
        {
            string companyUrl = UrlCompanias;
            string companyEndpoint = $"{companyUrl}Search?limit={limit}&numberPage={numberPage}"; string response = client.DownloadString(companyEndpoint);
            CompanyData companyData = JsonConvert.DeserializeObject<CompanyData>(response); if (companyData != null && companyData.items != null)
            {
                foreach (var item in companyData.items)
                {
                    if (OperatorCompanyId == item.codigoMid && item.nombreCorto != null)
                    {
                        return item.nombreCorto;
                    }
                }
                return "Compañía no encontrada";
            }
            else
                return "Excedió límite de páginas";
        }


        public async Task GetToken(WebClient client)
        {
            var url = UrlAuthentication;
            var requestBody = $"{{\"validacion\": 1,\"usuario\": \"{UserAuthentication}\",\"contrasena\": \"{PasswordAuthentication}\"}}";
            var apiVersion = "1"; client.Headers.Add("Content-Type", "application/json");
            client.Headers.Add("Api-version", apiVersion);
            string response = client.UploadString(url, "POST", requestBody);
            dynamic jsonResponse = JsonConvert.DeserializeObject(response);
            string token = jsonResponse.token;
            string authorizationHeader = "Bearer " + token;
            client.Headers[HttpRequestHeader.Authorization] = authorizationHeader;
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
                                                'AdicionPSM', 'CancelaCsgIngFueraPSM', 'CancelaCsgPSM', 'ModFechaAdicionAP',
                                                'ModFechaAdicionEA', 'ModFechaCambiaSemana', 'ModFechaCancelaAP',
                                                'ModFechaCancelaEA', 'ModFechaDuracion', 'ReprogramaCsg'
                                                 )) causa on p.PenaltyTypeId = causa.id
                                            where p.active = 1  and date between @pFechaInicioMes
                                            and @pFechaFinMes
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
                sql = sql + " and c.ElementId in ( " + string.Concat("'", string.Join("','", codigoActivo), "'") + " )";



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
                    IndicatorType = Constants.IndicatorConstants.STN
                });
            }

            return consignments;
        }

        public List<Action> GetActions(DateTime fechaInicioMes, DateTime fechaFinMes, List<string> codigoActivo)
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
                           AND VALUE IN('Abrir', 'Cerrar','Bajar potencia activa','Subir potencia activa')
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
                       END[Action],Causa.value as Type, CauseOrigin
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
                    ElementId = reader.GetString(3),
                    ActionName = reader.GetString(4),
                    Type = reader.GetString(5),
                    CauseOrigin = reader.IsDBNull(6) ? null : reader.GetString(6),
                    ElementCompanyShortName = reader.IsDBNull(7) ? null : reader.GetString(7),
                    ActionType = CalculationConstants.MHAIA_STN
                });
            }

            return actions;
        }
    }
}

