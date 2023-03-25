using MVM.ProcessEngine.Interfaces;
using MVM.ProcessEngine.TO;
using Spring.Context.Support;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Common.Helpers
{


    /// <summary>
    /// Clase utilizada para gestionar los mensajes que arroja el proceso de cálculo
    /// </summary>
    public class BitacoraMensajesHelper
    {
        #region Localización, cultura y mensajes
        private static readonly string _timeZoneSetting = "TimeZoneKey";
        /// <summary>
        /// id del proceso generado al encolar el proceso. Se utiliza posteriormente en cada operación
        /// </summary>
        private string _idLog;
        /// <summary>
        /// maneja el mecanismo de persistencia de los mensajes 
        /// </summary>
        private static IMensajesManager mensajesManager;

        public BitacoraMensajesHelper(string idLog)
        {
            _idLog = idLog;
        }
        /// <summary>
        /// operación que registra el proceso en la bitácora y permite obtener el id.
        /// </summary>
        /// <param name="nombreSistema"></param>
        /// <param name="nombreProceso"></param>
        public void EncolarProceso(string tenant, string nombreSistema, string nombreProceso)
        {
            mensajesManager = ContextRegistry.GetContext()["mensajesManager"] as IMensajesManager;
            mensajesManager.RegistrarProceso(tenant,_idLog, nombreSistema, nombreProceso);
        }

        /// <summary>
        /// Cierra la ejecución con éxito o error y usando la fecha actual
        /// </summary>
        /// <param name="esExito"></param>
        /// <param name="gui"></param>
        public void FinalizarProceso(string tenant, bool esExito, string mensajeFinal)
        {
            string timeZoneKey = GestorCalculosHelper.GetMetadataValue(tenant, _timeZoneSetting, true);
            DateTime? messageDate = string.IsNullOrEmpty(timeZoneKey) ? DateTime.Now : GetTimeZoneDate(timeZoneKey, DateTime.Now);
            string estadoFinal = (esExito ? "Exito" : "Error");
            mensajesManager.ActualizarEstadoProceso(tenant, _idLog, estadoFinal, mensajeFinal, messageDate);
        }

        /// <summary>
        /// Inicializa la lista de mensajes de un proceso
        /// </summary>
        /// <param name="nombreProceso">Nombre del proceso</param>
        public void InicializarMensajes(string tenant, string nombreProceso)
        {
            mensajesManager.ActualizarEstadoProceso(tenant, _idLog, "En curso", ObtenerMensajeRecursos("GestorCalculosInfo_InicioProceso", nombreProceso), null);

        }

        /// <summary>
        /// Inserta un mensaje en la lista de mensajes del cargador
        /// </summary>
        /// <param name="mensaje">Mensaje a escribir. Si el mensaje empieza con '@', 
        /// se entiende que se está enviando el nombre de un key del archivo de recursos</param>
        /// <param name="parametros">Parámetros de reemplazo en el mensaje</param>
        public void InsertarMensaje(string tenant, string mensaje, params string[] parametros)
        {
            string timeZoneKey = GestorCalculosHelper.GetMetadataValue(tenant, _timeZoneSetting, true);
            DateTime? messageDate = string.IsNullOrEmpty(timeZoneKey) ? DateTime.Now : GetTimeZoneDate(timeZoneKey, DateTime.Now);

            //Si el desarrollador envía una '@' al principio del mensaje, se entiende que se está enviando
            //el nombre de un key del archivo de recursos, por lo que se consulta
            if (mensaje.StartsWith("@"))
                mensaje = ObtenerMensajeRecursos(mensaje.Remove(0, 1), parametros);

            Task.Factory.StartNew(() =>
                RegistrarMensaje(tenant, string.Format("{0} - {1}", messageDate.ToString(), mensaje)),
                TaskCreationOptions.LongRunning);
        }


        /// <summary>
        /// Finaliza la lista de mensajes del cargador
        /// </summary>
        /// <param name="descripcionProceso"></param>
        public void FinalizarMensajes(string tenant,string descripcionProceso)
        {
            InsertarMensaje(tenant,"@GestorCalculosInfo_FinProceso", descripcionProceso);
        }

        /// <summary>
        /// Retorna todos los meensajes registrados para el proceso vinculado a esta bitácora.
        /// </summary>
        /// <returns></returns>
        public string ObtenerMensajes(string tenant)
        {
            return mensajesManager.ObtenerMensajesProceso(tenant,_idLog);
        }

        /// <summary>
        /// Establece en el hilo actual la información de localización que se encuentra
        /// definida en el archivo de configuración del contexto de la aplicación.
        /// </summary>
        public static void UpdateCurrentCulture(string tenant)
        {
            CultureInfo culture = new CultureInfo(GestorCalculosHelper.GetMetadataValue(tenant,"Cultura", true));
            String formatoHora = GestorCalculosHelper.GetMetadataValue(tenant, "FormatoHoraSistema", true);
            String formatoFecha = GestorCalculosHelper.GetMetadataValue(tenant, "FormatoFechaSistema", true);
            String separadorDecimal = GestorCalculosHelper.GetMetadataValue(tenant, "SeparadorDecimalSistema", true);
            String separadorMiles = GestorCalculosHelper.GetMetadataValue(tenant, "SeparadorMilesSistema", true);

            culture.DateTimeFormat.ShortDatePattern = formatoFecha;
            culture.DateTimeFormat.ShortTimePattern = formatoHora;
            culture.DateTimeFormat.LongTimePattern = formatoHora;
            culture.DateTimeFormat.LongDatePattern = formatoFecha + " " + formatoHora;
            culture.NumberFormat.NumberDecimalSeparator = separadorDecimal == "ç" ? "," : separadorDecimal;
            culture.NumberFormat.CurrencyDecimalSeparator = separadorDecimal == "ç" ? "," : separadorDecimal;
            culture.NumberFormat.NumberGroupSeparator = separadorMiles == "ç" ? "," : separadorMiles;
            culture.NumberFormat.CurrencyGroupSeparator = separadorMiles == "ç" ? "," : separadorMiles;
            culture.NumberFormat.CurrencySymbol = GestorCalculosHelper.GetMetadataValue(tenant, "FormatoMoneda", true);
            Thread.CurrentThread.CurrentCulture = culture;
        }

        /// <summary>
        /// Obtiene un mensaje del archivo de recursos
        /// </summary>
        /// <param name="key">Nombre del mensaje a traducir.</param>
        /// <returns>Un texto con el mensaje traducido, o la clave en caso que no exista.</returns>
        public static string ObtenerMensajeRecursos(string key)
        {
            ResourceManager rm = new ResourceManager("MVM.ProcessEngine.Common.Messages", Assembly.GetExecutingAssembly());
            var mess = rm.GetString(key);

            return mess;

        }

        /// <summary>
        /// Obtiene un mensaje del archivo de recursos
        /// </summary>
        /// <param name="key">Key del archivo de recursos</param>
        /// <param name="parametros">Parámetros de reemplazo en el mensaje</param>
        /// <returns>Mensaje del archivo de recursos</returns>
        public static string ObtenerMensajeRecursos(string key, params object[] parametros)
        {
            string mensaje = ObtenerMensajeRecursos(key);
            string mensajeTraducido = (parametros != null) ? string.Format(mensaje, parametros) : mensaje;
            return mensajeTraducido;
        }

        /// <summary>
        /// Operación para registrar un mensaje en la bitácora usando el manager
        /// </summary>
        /// <param name="mensaje"></param>
        private void RegistrarMensaje(string tenant, string mensaje)
        {
            mensajesManager.RegistrarMensaje(tenant, _idLog, mensaje);
        }

        /// <summary>
        /// Retorna una fecha ajustada a un uso horario específico
        /// </summary>
        /// <param name="timeZoneKey">Uso horario</param>
        /// <param name="date">Fecha a modificar</param>
        /// <returns></returns>
        public DateTime? GetTimeZoneDate(string timeZoneKey, DateTime? date)
        {
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneKey);
            DateTime? timeZoneDate = date.HasValue ? TimeZoneInfo.ConvertTime(date.Value, timeZoneInfo) : date;
            return timeZoneDate;
        }
        #endregion
    }
}
