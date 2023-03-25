#region Derechos Reservados
// ===================================================
// Desarrollado Por	    : esteban.giraldo
// Fecha de Creación	: 2015-02-06
// Modificado Por       : esteban.giraldo
// Fecha Modificación   : 2015-02-26  
// Empresa		        : MVM S.A.S
// ===================================================
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using AopAlliance.Intercept;
using MVM.ProcessEngine.Common.Exceptions;
using MVM.ProcessEngine.Common.Helpers;
using MVM.ProcessEngine.TO;

namespace MVM.ProcessEngine.Common.AOP
{
    /// <summary>
    /// Interceptor utilizado para ejecutar lógica antes y despues de la invocación de un método
    /// </summary>
    public class GestorCalculosMethodInterceptor : IMethodInterceptor
    {
        /// <summary>
        /// Permite invocar el método
        /// </summary>
        /// <param name="invocation">Método a invocar</param>
        /// <returns>Retorna el resultado de la invocación</returns>
        public object Invoke(IMethodInvocation invocation)
        {
            string method = invocation.Method.Name;
            string logger = invocation.Proxy.ToString();

            try
            {
                //Se sobreescribe la localización del hilo de ejecución
                //BitacoraMensajesHelper.UpdateCurrentCulture();

                DateTime startDateTime = DateTime.Now;
                GestorCalculosHelper.WriteDebug(BitacoraMensajesHelper.ObtenerMensajeRecursos("GestorCalculosDebug_RegistroInicioMetodo", method, startDateTime.ToShortDateString(), startDateTime.ToShortTimeString()));

                //Ejecuta el llamado y obtiene el resultado
                Object result = invocation.Proceed();

                DateTime endDateTime = DateTime.Now;
                GestorCalculosHelper.WriteDebug(BitacoraMensajesHelper.ObtenerMensajeRecursos("GestorCalculosDebug_RegistroFinMetodo", method, endDateTime.ToShortDateString(), endDateTime.ToShortTimeString()));

                return result;
            }
            catch (System.Exception e)
            {
                var exception = e as GestorCalculosException;

                if (exception == null)
                    exception = new GestorCalculosException("GestorCalculosError_ExcepcionNoControlada", e, method);

                var integrationFault = new GestorCalculosIntegrationFault();
                integrationFault.CodigoError = BitacoraMensajesHelper.ObtenerMensajeRecursos("GestorCalculosError_CodigoGenerico");
                integrationFault.Mensaje = exception.Message;

                if (exception.Data != null && exception.Data.Count > 0)
                {
                    if (exception.Data.Contains("CodigoError"))
                    {
                        integrationFault.CodigoError = exception.Data["CodigoError"].ToString();
                    }

                    integrationFault.TipoError = exception.Data.Contains("TipoError") ? exception.Data["TipoError"].ToString() : TipoError.Tecnico.ToString();
                    integrationFault.Detalle = exception.Data.Contains("Detalle") && exception.Data["Detalle"] != null ? exception.Data["Detalle"].ToString() : e.Message;
                }

                //Se hace la escritura de la excepción en el log
                exception.WriteLog(logger, method);

                throw new FaultException<GestorCalculosIntegrationFault>(integrationFault, new FaultReason(new FaultReasonText(integrationFault.Mensaje)), new FaultCode(integrationFault.CodigoError));
            }
        }
    }
}
