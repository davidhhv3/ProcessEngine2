#region Derechos Reservados
// ===================================================
// Desarrollado Por	    : esteban.giraldo
// Fecha de Creación	: 2015-02-06
// Modificado Por       :
// Fecha Modificación   :
// Empresa		        : MVM S.A.S
// ===================================================
#endregion

using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using MVM.ProcessEngine.TO;
using System.Globalization;
using Spring.Context.Support;
using Spring.Caching;
using System.Threading;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using System.Diagnostics;
using System.Security.Principal;
using System.Web;
using System.ServiceModel;
using System.Net;
using System.ServiceModel.Channels;
using MVM.ProcessEngine.Common.Exceptions;
using System.Xml.Serialization;
using System.IO; 

namespace MVM.ProcessEngine.Common.Helpers
{
    /// <summary>
    /// Clase que contiene métodos utilitarios para el servicio del gestor de cálculos
    /// </summary>
    public static partial class GestorCalculosHelper
    {
       
        #region Logging
        private const string APPDOMAINNAME = "-";
        private const string PROCESSID = "0";
        private const string PROCESSNAME = "0";

        /// <summary>
        /// Método utilitario que permite escribir un registro en el log
        /// </summary>
        /// <param name="entry">Entrada a registrar en el log</param>
        /// <remarks>El registro de log se hace por defecto en base de datos. En caso de excepción
        /// el registro se lleva a cabo en un archivo de texto.</remarks>
        public static void WriteLog(LogEntry entry)
        {            
            try
            {
                //Se hace el registro en el log dependiendo de las categorías
                Logger.Write(entry);
            }
            catch (Exception)
            {
                entry.Categories.Clear();
                entry.Categories.Add(LogCategory.LogErrors.GetDescription());
                //A través de la categoría LogErrors, se fuerza a realizar el registro en archivo plano
              //  Logger.Write(entry); DA-Log
            }
        }

        /// <summary>
        /// Método utilitario que permite escribir un registro en el log
        /// </summary>
        /// <param name="message">Mensaje a registrar</param>
        /// <param name="tipoAccion">Tipo de acción a registrar</param>
        /// <param name="category">Categoría a la que pertenece el mensaje que se va a registrar</param>
        /// <param name="traceEventType">Tipo de evento trace a registrar</param>
        /// <param name="args">Argumentos a remplazar en el mensaje</param>
        public static void WriteLog(string message, TiposAccion tipoAccion, LogCategory category, TraceEventType traceEventType, params object[] args)
        {
            LogEntry entry = CreateDefaultLogEntry();
            entry.Title = category.GetDescription();
            entry.Categories.Add(category.GetDescription());
            entry.Message = (args == null) ? BitacoraMensajesHelper.ObtenerMensajeRecursos(message) : BitacoraMensajesHelper.ObtenerMensajeRecursos(message, args);
            entry.MachineName = ObtenerUsuarioId();
            entry.TimeStamp = DateTime.Now;
            entry.Severity = traceEventType;
            entry.ProcessName = tipoAccion.GetDescription();
            WriteLog(entry);
        }

        /// <summary>
        /// Permite escribir en el log información de seguimiento
        /// </summary>
        /// <param name="message">Mensaje a escribir en el log</param>
        public static void WriteDebug(string message)
        {
            LogEntry entry = CreateDefaultLogEntry();
            entry.Title = LogCategory.Debug.ToString();
            entry.Categories.Add(LogCategory.Debug.GetDescription());
            entry.Message = message;
            entry.TimeStamp = DateTime.Now;

            WriteLog(entry);
        }

        /// <summary>
        /// Permite escribir en el log información.
        /// </summary>
        /// <param name="message">Mensaje de información a escribir en el log</param>
        public static void WriteInfo(string message, TiposAccion tipoAccion)
        {
            WriteInfo(message, tipoAccion, null);
        }

        /// <summary>
        /// Permite escribir en el log información.
        /// </summary>
        /// <param name="message">Mensaje de información a escribir en el log</param>
        public static void WriteInfo(string message, TiposAccion tipoAccion, params object[] args)
        {
            WriteLog(message, tipoAccion, LogCategory.Information, TraceEventType.Information, args);
        }

        /// <summary>
        /// Permite escribir en el log información.
        /// </summary>
        /// <param name="message">Mensaje de información a escribir en el log</param>
        public static void WriteInfo(string message, TiposAccion tipoAccion, int idOrden, string idSolicitud, params object[] args)
        {
            WriteLog(message, tipoAccion, LogCategory.Information, TraceEventType.Information, idOrden, idSolicitud, args);
        }

        /// <summary>
        /// Permite registrar en el log mensajes de seguimiento y notificación
        /// </summary>
        /// <param name="message">Mensaje a registrar</param>
        public static void WriteTrace(string message)
        {
            WriteTrace(message, null);
        }

        /// <summary>
        /// Permite registrar en el log mensajes de seguimiento y notificación
        /// </summary>
        /// <param name="message">Mensaje a registrar</param>
        public static void WriteTrace(string message, int idOrden, string idSolicitud)
        {
            WriteTrace(message, idOrden, idSolicitud, null);
        }

        /// <summary>
        /// Permite registrar en el log mensajes de seguimiento y notificación
        /// </summary>
        /// <param name="message">Mensaje a registrar</param>
        /// <param name="args">Argumentos a remplazar en el mensaje</param>
        public static void WriteTrace(string message, params object[] args)
        {
            WriteLog(message, TiposAccion.Notificacion, LogCategory.Information, TraceEventType.Verbose, args);
        }

        /// <summary>
        /// Permite crear la instancia por defecto del LogEntry
        /// </summary>
        /// <returns>Retorna una nueva instancia del tipo LogEntry</returns>
        public static LogEntry CreateDefaultLogEntry()
        {
            AppDomain domain = AppDomain.CurrentDomain;
            Process process = Process.GetCurrentProcess();

            LogEntry entry = new LogEntry();
            entry.Severity = TraceEventType.Information;
            entry.Title = LogCategory.General.GetDescription();
            entry.TimeStamp = DateTime.Now;
            entry.MachineName = ObtenerUsuarioId();
            entry.AppDomainName = (domain != null) ? domain.FriendlyName : APPDOMAINNAME;
            entry.ProcessId = (process != null) ? process.Id.ToString() : PROCESSID;
            entry.ProcessName = (process != null) ? process.ProcessName : PROCESSNAME;

            return entry;
        }
        #endregion

        #region Seguridad
        /// <summary>
        /// Obtiene la IP del cliente que llama métodos del servicio de capa norte.
        /// </summary>
        /// <returns>La ip de la máquina cliente que llama al servicio de capa norte</returns>
        public static string ObtenerIPCliente()
        {
            var props = OperationContext.Current.IncomingMessageProperties;
            var endpointProperty = props[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            if (endpointProperty != null)
            {
                var ip = endpointProperty.Address;
                IPAddress[] ipInfo = Dns.GetHostAddresses(ip);
                IPAddress[] ipReal = Dns.GetHostEntry(ipInfo[0]).AddressList;

                IPAddress ipV6 = ipReal.Where(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6).FirstOrDefault();
                IPAddress ipV4 = ipReal.Where(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault();

                return ipV4.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Permite obtener el usuario actual autenticado en el contexto de la aplicación
        /// </summary>
        /// <returns>Retorna el id del usuario sin el dominio</returns>
        public static string ObtenerUsuarioId()
        {
            string usuarioId = string.Empty;

            IIdentity identity = null;

            if (HttpContext.Current != null)
            {
                identity = HttpContext.Current.User.Identity;
            }
            else
            {
                usuarioId = Thread.CurrentThread.Name;
            }

            if (identity != null)
            {
                usuarioId = identity.Name;
            }

            if (string.IsNullOrEmpty(usuarioId))
            {
                identity = WindowsIdentity.GetCurrent();
                if (identity != null)
                {
                    usuarioId = identity.Name;
                }
            }
            int posDominio = usuarioId.IndexOf(@"\");

            if (posDominio != -1)
            {
                usuarioId = usuarioId.Substring(posDominio + 1);
            }

            return usuarioId;
        }
        #endregion

        #region Generales

        /// <summary>
        /// Permite obtener el valor de la constante
        /// </summary>
        /// <param name="nombreArchivo">Nombre del archivo</param>
        /// <param name="nombreHoja">Nombre de la hoja de excel</param>
        /// <param name="nombreConfiguracion">Nombre de la configuración</param>
        /// <param name="valor">Valor actual</param>
        /// <param name="parametros">Parámetros dinámicos</param>
        /// <param name="buffer">Buffer de parametros en memoria dinámico</param>
        /// <param name="idGestor">Identificador del proceso.</param>
        /// <returns>Retorna el valor de la constante</returns>
        public static object ObtenerValorConstante(string nombreConfiguracion, object valor, List<object> parametros, List<object> buffer, string idGestor)
        {
            //Se validan las expresiones
            if (valor != null && valor.ToString().ToUpper() == "[FECHA]")
                return DateTime.Now;
            if (valor != null && valor.ToString().ToUpper() == "[CONFIGURACION]")
                return nombreConfiguracion;
            if (valor != null && valor.ToString().ToUpper() == "[ID_GESTOR]")
                return idGestor;

            if (valor != null && valor.ToString().ToUpper().StartsWith("[PARAMETRO("))
            {
                valor = ObtenerValorParametro(valor, parametros);
            }

            if (valor != null && valor.ToString().ToUpper().StartsWith("[BUFFER("))
            {
                valor = ObtenerValorParametro(valor, buffer);
            }

            if (valor != null && valor.ToString().ToUpper().StartsWith("[SUBCADENA("))
            {
                try
                {
                    //Se toma en una cadena el contenido de los paréntesis
                    int primerParentesis = valor.ToString().IndexOf("(");
                    int segundoParentesis = valor.ToString().IndexOf(")");
                    string interiorParentesis = valor.ToString().Substring(primerParentesis + 1,
                                                                           segundoParentesis - primerParentesis - 1);
                    string[] operadores = interiorParentesis.Split(',');

                    string origen = operadores[0];
                    int desde = Convert.ToInt32(operadores[1]);
                    int? longitud = null;

                    if (operadores.Length == 3)
                        longitud = Convert.ToInt32(operadores[2]);

                    return (longitud.HasValue) ? ObtenerValorConstante(nombreConfiguracion, origen, null, null, null).ToString().Substring(
                        desde, longitud.Value) : ObtenerValorConstante(nombreConfiguracion, origen, null, null, null).ToString().Substring(
                            desde);
                }
                catch (Exception)
                {
                    throw new GestorCalculosException("GestorCalculosError_ConstanteSubcadena");
                }
            }

            return valor;
        }

        /// <summary>
        /// Obtiene el valor de una equivalencia según las expresiones predefinidas
        /// </summary>
        /// <param name="valorAnterior"></param>
        /// <param name="valorNuevo"></param>
        /// <param name="nombreConfiguracion"></param>
        /// <param name="buffer"></param>
        /// <param name="parametros"></param>
        /// <param name="idGestor">Identificador del proceso.</param>
        /// <returns></returns>
        public static object ObtenerValorEquivalencia(string nombreConfiguracion, object valorAnterior, object valorNuevo, List<object> parametros, List<object> buffer, string idGestor)
        {
            //Se validan las expresiones
            if (valorNuevo.ToString().ToUpper() == "[FECHA]")
                return DateTime.Now;
            if (valorNuevo.ToString().ToUpper() == "[CONFIGURACION]")
                return nombreConfiguracion;
            if (valorNuevo.ToString().ToUpper() == "[ID_GESTOR]")
                return idGestor;

            if (valorNuevo.ToString().ToUpper().StartsWith("[PARAMETRO("))
            {
                valorNuevo = ObtenerValorParametro(valorNuevo, parametros);
            }

            if (valorNuevo.ToString().ToUpper().StartsWith("[BUFFER("))
            {
                valorNuevo = ObtenerValorParametro(valorNuevo, buffer);
            }

            if (valorNuevo.ToString().ToUpper().StartsWith("[SUBCADENA("))
            {
                try
                {
                    //Se toma en una cadena el contenido de los paréntesis
                    int primerParentesis = valorNuevo.ToString().IndexOf("(");
                    int segundoParentesis = valorNuevo.ToString().IndexOf(")");
                    string interiorParentesis = valorNuevo.ToString().Substring(primerParentesis + 1,
                                                                                segundoParentesis - primerParentesis - 1);
                    string[] operadores = interiorParentesis.Split(',');
                    int desde = Convert.ToInt32(operadores[0]);
                    int? longitud = null;

                    if (operadores.Length == 2)
                        longitud = Convert.ToInt32(operadores[1]);

                    return (longitud.HasValue) ? valorAnterior.ToString().Substring(desde, longitud.Value) :
                        valorAnterior.ToString().Substring(desde);
                }
                catch (Exception)
                {
                    //Si se presenta algún error, se asume que la expresión está mal conformada y se devuelve el 
                    //valor tal cual éste llega
                    return valorAnterior;
                }
            }

            return valorNuevo;
        }

        /// <summary>
        /// Obtiene un valor de la lista de parámetros
        /// </summary>
        /// <param name="valorCadena">valor de la cadena</param>
        /// <param name="parametros">Lista de parámetros</param>
        /// <returns>Dato obtenido de los parámetros</returns>
        public static object ObtenerValorParametro(object valorCadena, List<object> parametros)
        {
            object valor = null;

            if (parametros != null || parametros.Count > 0)
            {
                try
                {
                    int indiceComienzo = valorCadena.ToString().IndexOf("(") + 1;
                    int indiceFin = valorCadena.ToString().IndexOf(")");
                    int longitud = indiceFin - indiceComienzo;

                    int indiceBaseCero = int.Parse(valorCadena.ToString().Substring(indiceComienzo, longitud));
                    valor = parametros[indiceBaseCero];
                }
                catch (Exception)
                {
                    throw new GestorCalculosException("GestorCalculosError_ConstanteParametro", valorCadena);
                }
            }

            return valor;
        }

        /// <summary>
        /// Permite validar el tipo de dato enviado
        /// </summary>
        /// <param name="valor">Valor a validar</param>
        /// <param name="tipo">Tipo de dato a validar</param>
        /// <param name="cultura">cultura en la cual se debe validar el tipo de dato</param>
        /// <returns>True si el tipo es válido</returns>
        public static bool ValidarTipoDato(object valor, TipoDato tipo, string cultura)
        {
            bool esValido = true;

            switch (tipo)
            {
                case TipoDato.Integer:
                    int resultadoInt;
                    if (!int.TryParse(valor.ToString(), out resultadoInt))
                        esValido = false;
                    break;
                case TipoDato.Decimal:
                    decimal resultadoDecimal;
                    
                    if (string.IsNullOrEmpty(cultura))
                    {
                        if (!decimal.TryParse(valor.ToString(), out resultadoDecimal))
                            esValido = false;
                    }else
                    {
                        if (!decimal.TryParse(valor.ToString(), NumberStyles.Number, new CultureInfo(cultura), out resultadoDecimal))
                            esValido = false;
                    }

                    break;
                case TipoDato.DateTime:
                    DateTime resultadoDateTime;

                    if (string.IsNullOrEmpty(cultura))
                    {
                        if (!DateTime.TryParse(valor.ToString(), out resultadoDateTime))
                            esValido = false;
                    }
                    else
                    {
                        if (!DateTime.TryParse(valor.ToString(), new CultureInfo(cultura), DateTimeStyles.None, out resultadoDateTime))
                            esValido = false;
                    }
                    break;
                case TipoDato.Boolean:
                    bool resultadoBool;
                    if (!bool.TryParse(valor.ToString(), out resultadoBool))
                        esValido = false;
                    break;
                default:
                    esValido = true;
                    break;
            }

            return esValido;
        }

        /// <summary>
        /// Obtiene el numero de horas del mes de una fecha.
        /// </summary>
        /// <param name="mesCalculo">Fecha para calcular las horas del mes.</param>
        /// <returns>Numero de horas del mes.</returns>
        public static int ObtenerHorasMes(DateTime mesCalculo)
        {
            return (DateTime.DaysInMonth(mesCalculo.Year, mesCalculo.Month) * 24);
        }

        /// <summary>
        /// Retorna el valor decimal del texto o nulo si no se puede convertir.
        /// </summary>
        /// <param name="value">El texto a convertir.</param>
        /// <returns>Un valor decimal o nulo.</returns>
        public static decimal? DecimalOrNull(string value)
        {
            decimal testValue = -1;
            if (decimal.TryParse(value, out testValue))
            {
                return testValue;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Retorna el valor entero del texto o nulo si no se puede convertir.
        /// </summary>
        /// <param name="value">El texto a convertir.</param>
        /// <returns>Un valor entero o nulo.</returns>
        public static int? IntOrNull(string value)
        {
            int testValue = -1;
            if (int.TryParse(value, out testValue))
            {
                return testValue;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Retorna el valor fecha o nulo si no se puede convertir
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime? DateTimeOrNull(string value)
        {
            DateTime testValue;
            if (DateTime.TryParse(value, out testValue))
            {
                return testValue;
            }
            return null;
        }

        /// <summary>
        /// Permite obtener un valor desde la cache
        /// </summary>
        /// <param name="key">clave del objeto que se obtiene</param>
        /// <returns>retorna el objeto</returns>
        public static object GetDataFromCache(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            object result = null;

            //Se obtiene la cache de spring
            var cache = ContextRegistry.GetContext().GetObject("GestorCalculosCache") as ICache;

            if (cache != null)
                result = cache.Get(key);

            return result;
        }

        /// <summary>
        /// Permite almacenar los datos en la cache
        /// </summary>
        /// <param name="key">Identificador con la información a almacenear en Cache</param>
        /// <param name="data">Datos a almacenar en cache</param>
        public static void SaveDataInCache(string key, object data)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            object result = null;

            //Se obtiene la cache de spring
            var cache = ContextRegistry.GetContext().GetObject("GestorCalculosCache") as ICache;

            if (cache == null)
                throw new ArgumentNullException("cache");

            result = cache.Get(key);

            if (result != null)
            {
                cache.Remove(key);
            }

            cache.Insert(key, data);
        }

        /// <summary>
        /// Permite eliminar un elmento desde la cachce
        /// </summary>
        /// <param name="key">Clave del elemento a eliminar</param>
        public static void RemoveDataFromCache(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            //Se obtiene la cache de spring
            var cache = ContextRegistry.GetContext().GetObject("GestorCalculosCache") as ICache;

            if (cache != null)
                cache.Remove(key);
        }

        /// <summary>
        /// Retorna el valor de configuración para la llave enviada.
        /// </summary>
        /// <param name="llaveAtributo">String con el valor de llave.</param>
        /// <param name="esObligatorio">Indica si el valor de configuración es obligatorio.</param>
        /// <returns>Valor de configuración para la llave enviada.</returns>
        /// <remarks>Lanza excepción de configuracion cuando no encuentra valor de la llave y el dato es obligatorio.</remarks>
        //public static string ObtenerAtributoDeConfiguracion(string llaveAtributo, bool esObligatorio)
        //{
        //    //string valor = ConfigurationManager.AppSettings.Get(llaveAtributo);
        //    string valor = GestorCalculosServiceLocator.GetService<AppSetting>("appSetting").Keys.Where(w => w.Name == llaveAtributo).FirstOrDefault().Value;

        //    if (valor == null && esObligatorio)
        //    {
        //        throw new GestorCalculosException("GestorCalculosError_MensajeValorConfiguracion", llaveAtributo);
        //    }

        //    return valor;
        //}

        /// <summary>
        /// Get Metadata Value for a Tenant
        /// </summary>
        /// <param name="llaveAtributo"></param>
        /// <param name="llaveAtributo"></param>
        /// <param name="esObligatorio"></param>
        /// <returns></returns>
        public static string GetMetadataValue(string tenant, string key, bool mandatory)
        {
            string value = GestorCalculosServiceLocator.GetService<AppSetting>("appSetting").Metadata[tenant].Where(w => w.Name == key).FirstOrDefault().Value;

            if (value == null && mandatory)
            {
                throw new GestorCalculosException("GestorCalculosError_MensajeValorConfiguracion", key);
            }

            return value;
        }


        /// <summary>
        /// Convierte un valor a un entero nullable. Si el valor llega vacío, se retorna un null
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static int? ConvertirAEnteroNullable(object o)
        {
            if (string.IsNullOrEmpty(o.ToString()))
                return null;
            return int.Parse(o.ToString());
        }

        /// <summary>
        /// Permite formatear el tipo de dato
        /// </summary>
        /// <param name="valor">Valor a formatear</param>
        /// <param name="tipoDato">Tipo de dato a formatear</param>
        /// <param name="cultura">Cultura que se debe utilizar a la hora de formatear el dato</param>
        /// <returns>Retorna el valor formateado</returns>
        public static object FormatearTipoDato(string valor, TipoDato tipoDato, string cultura)
        {
            object valorConvertido = DBNull.Value;

            CultureInfo proveedorCultura = new CultureInfo(cultura);

            switch (tipoDato)
            {
                case TipoDato.String:
                    valorConvertido = valor;
                    break;
                case TipoDato.Integer:
                    valorConvertido = Int16.Parse(valor);
                    break;
                case TipoDato.Decimal:
                    valorConvertido = Decimal.Parse(valor, proveedorCultura);
                    break;
                case TipoDato.DateTime:
                    valorConvertido = DateTime.Parse(valor, proveedorCultura);
                    break;
                case TipoDato.Boolean:
                    valorConvertido = Boolean.Parse(valor);
                    break;
                default:
                    valorConvertido = valor;
                    break;
            }

            return valorConvertido;
        }

        /// <summary>
        /// Permite deserializar el XML contenido en el string y lo retorna en un objeto
        /// </summary>
        /// <returns>El objeto con la información deserializada</returns>
        /// <param name="xml">Xml a deserializar</param>
        public static T DeserializeXml<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                throw new ArgumentNullException("xml");

            T resultado = default(T);
            var serializer = new XmlSerializer(typeof(T));
            using (var sr = new StringReader(xml))
            {
                resultado = (T)serializer.Deserialize(sr);
            }

            return resultado;
        }

        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        #endregion
    }
}