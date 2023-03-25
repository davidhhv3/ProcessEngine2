using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Interfaces
{
    /// <summary>
    /// Define la interface para registrar el proceso y escribir mensajes de tracking del mismo
    /// </summary>
    public interface IMensajesManager
    {
         /// <summary>
         /// Proceso para registrar un nuevo proceso antes de iniciar el registro de mensajes.
         /// Esta operación retorna el id que permitirá registrar mensajes y actualizar el estado del proceso
         /// posteriormente
         /// </summary>
         /// <param name="sistema"></param>
         /// <param name="nombre"></param>
         /// <returns></returns>
         void RegistrarProceso(string tenant, string idProceso, string sistema, string nombre);
        /// <summary>
        /// Asocia un nuevo mensaje al proceso
        /// </summary>
        /// <param name="idProceso"></param>
        /// <param name="mensaje"></param>
         void RegistrarMensaje(string tenant, string idProceso, string mensaje);
        /// <summary>
        /// Se actualiza el estado actual del proceso
        /// </summary>
        /// <param name="idProceso"></param>
        /// <param name="estado"></param>
        /// <param name="?"></param>
        /// <param name="mensaje"></param>
        /// <param name="fechaFinal"></param>
        /// <param name="gui"></param>
         void ActualizarEstadoProceso(string tenant, string idProceso, string estado, string mensajeFinal, DateTime? fechaFinal);
        /// <summary>
        /// Se consultan todos los mensajes registrados a ese momento para el proceso
        /// </summary>
        /// <param name="idProceso"></param>
        /// <returns></returns>
         string ObtenerMensajesProceso(string tenant, string idProceso);
    }
}
