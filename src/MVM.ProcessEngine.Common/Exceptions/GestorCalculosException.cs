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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVM.ProcessEngine.Common.Helpers;
using MVM.ProcessEngine.TO;

namespace MVM.ProcessEngine.Common.Exceptions
{
    /// <summary>
    /// Clase que especiliza las excepciones generadas por el servicio de gestor de cálculos.
    /// </summary>
    public class GestorCalculosException : ApplicationException
    {
        #region Constructores
        /// <summary>
        /// Crea una nueva instancia del tipo GestorCalculosException
        /// </summary>
        /// <param name="messageKey">El mensaje de la excepción</param>
        public GestorCalculosException(String messageKey) : base(BitacoraMensajesHelper.ObtenerMensajeRecursos(messageKey)) { }

        /// <summary>
        /// Crea una nueva instancia del tipo GestorCalculosException
        /// </summary>
        /// <param name="messageKey">El mensaje de la excepción</param>
        /// <param name="exception">Inner exception</param>
        public GestorCalculosException(String messageKey, Exception exception)
            : base(BitacoraMensajesHelper.ObtenerMensajeRecursos(messageKey), exception) { }

        /// <summary>
        /// Crea una nueva instancia del tipo GestorCalculosException
        /// </summary>
        /// <param name="messageKey">El mensaje de la excepción</param>        
        /// <param name="parameters">Parámetros de sustitución para el mensaje</param>
        public GestorCalculosException(String messageKey, params object[] parameters)
            : base(BitacoraMensajesHelper.ObtenerMensajeRecursos(messageKey, parameters)) { }

        /// <summary>
        /// Crea una nueva instancia del tipo GestorCalculosException
        /// </summary>
        /// <param name="messageKey">El mensaje de la excepción</param>        
        /// <param name="traducir">Indica si traduce el mensaje o no</param>
        public GestorCalculosException(String messageKey, bool traducir)
            : base((traducir) ? BitacoraMensajesHelper.ObtenerMensajeRecursos(messageKey) : messageKey) { }

        /// <summary>
        /// Crea una nueva instancia del tipo GestorCalculosException
        /// </summary>
        /// <param name="messageKey">El mensaje de la excepción</param>        
        /// <param name="traducir">Indica si traduce el mensaje o no</param>
        public GestorCalculosException(String messageKey, bool traducir, Exception exception)
            : base((traducir) ? BitacoraMensajesHelper.ObtenerMensajeRecursos(messageKey) : messageKey, exception) { }

        /// <summary>
        /// Crea una nueva instancia del tipo GestorCalculosException
        /// </summary>
        /// <param name="messageKey">Identificador del recurso para el mensaje a generar</param>
        /// <param name="exception">Excepción interna</param>
        /// <param name="parameters">Parámetros a reemplazar en el mensaje</param>
        public GestorCalculosException(string messageKey, Exception exception, params object[] parameters)
            : base(BitacoraMensajesHelper.ObtenerMensajeRecursos(messageKey, parameters), exception)
        {
            this.Data.Add("TipoError", TipoError.Tecnico.ToString());
            this.Data.Add("Detalle", this.StackTrace);
        }

        /// <summary>
        /// Crea una nueva instancia del tipo GestorCalculosException
        /// </summary>
        /// <param name="errorCodeKey">Identificador del recurso para el código del error a generar</param>
        /// <param name="messageKey">Identificador del recurso para el mensaje a generar</param>
        /// <param name="exception">Excepción interna</param>
        /// <param name="parameters">Parámetros a reemplazar en el mensaje</param>
        public GestorCalculosException(string errorCodeKey, string messageKey, Exception exception, params object[] parameters)
            : base(BitacoraMensajesHelper.ObtenerMensajeRecursos(messageKey, parameters), exception)
        {
            this.Data.Add("CodigoError", BitacoraMensajesHelper.ObtenerMensajeRecursos(errorCodeKey));
            this.Data.Add("TipoError", TipoError.Tecnico.ToString());
            this.Data.Add("Detalle", this.StackTrace);
        }

        /// <summary>
        /// Crea una nueva instancia del tipo GestorCalculosException
        /// </summary>
        /// <param name="messageKey">Identificador del recurso para el mensaje a generar</param>
        /// <param name="tipo">Tipo del error</param>
        /// <param name="exception">Excepción interna</param>
        /// <param name="parameters">Parámetros a reemplazar en el mensaje</param>
        public GestorCalculosException(string messageKey, TipoError tipo, Exception exception, params object[] parameters)
            : base(BitacoraMensajesHelper.ObtenerMensajeRecursos(messageKey, parameters), exception)
        {
            this.Data.Add("TipoError", tipo.ToString());
            this.Data.Add("Detalle", this.StackTrace);
        }

        /// <summary>
        /// Crea una nueva instancia del tipo GestorCalculosException
        /// </summary>
        /// <param name="errorCodeKey">Identificador del recurso para el código del error a generar</param>
        /// <param name="messageKey">Identificador del recurso para el mensaje a generar</param>
        /// <param name="tipo">Tipo del error</param>
        /// <param name="exception">Excepción interna</param>
        /// <param name="parameters">Parámetros a reemplazar en el mensaje</param>
        public GestorCalculosException(string errorCodeKey, string messageKey, TipoError tipo, Exception exception, params object[] parameters)
            : base(BitacoraMensajesHelper.ObtenerMensajeRecursos(messageKey, parameters), exception)
        {
            this.Data.Add("CodigoError", BitacoraMensajesHelper.ObtenerMensajeRecursos(errorCodeKey));
            this.Data.Add("TipoError", tipo.ToString());
            this.Data.Add("Detalle", this.StackTrace);
        }

        /// <summary>
        /// Crea una nueva instancia del tipo GestorCalculosException
        /// </summary>
        /// <param name="errorCodeKey">Identificador del recurso para el código del error a generar</param>
        /// <param name="messageKey">Identificador del recurso para el mensaje a generar</param>
        /// <param name="detalle">Detalle a mostrar</param>
        /// <param name="tipo">Tipo del error</param>
        /// <param name="exception">Excepción interna</param>
        /// <param name="parameters">Parámetros a reemplazar en el mensaje</param>
        public GestorCalculosException(string errorCodeKey, string messageKey, string detalle, TipoError tipo, Exception exception, params object[] parameters)
            : base(BitacoraMensajesHelper.ObtenerMensajeRecursos(messageKey, parameters), exception)
        {
            this.Data.Add("CodigoError", BitacoraMensajesHelper.ObtenerMensajeRecursos(errorCodeKey));
            this.Data.Add("TipoError", tipo.ToString());
            this.Data.Add("Detalle", detalle);
        }

        /// <summary>
        /// Crea una nueva instancia del tipo GestorCalculosException
        /// </summary>
        /// <param name="errorCodeKey">Identificador del recurso para el código del error a generar</param>
        /// <param name="messageKey">Identificador del recurso para el mensaje a generar</param>
        /// <param name="traducir">Indica si los mensajes de la excepción se traduciran</param>
        /// <param name="tipo">Tipo del error</param>
        /// <param name="exception">Excepción interna</param>
        /// <param name="parameters">Parámetros a reemplazar en el mensaje</param>
        public GestorCalculosException(string errorCodeKey, string messageKey, bool traducir, TipoError tipo, Exception exception, params object[] parameters)
            : base((traducir) ? BitacoraMensajesHelper.ObtenerMensajeRecursos(messageKey) : messageKey, exception)
        {
            this.Data.Add("CodigoError", traducir ? BitacoraMensajesHelper.ObtenerMensajeRecursos(errorCodeKey) : errorCodeKey);
            this.Data.Add("TipoError", tipo.ToString());
            this.Data.Add("Detalle", this.StackTrace);
        }

        /// <summary>
        /// Crea una nueva instancia del tipo GestorCalculosException
        /// </summary>
        /// <param name="errorCodeKey">Identificador del recurso para el código del error a generar</param>
        /// <param name="messageKey">Identificador del recurso para el mensaje a generar</param>
        /// <param name="detalle">Detalle a mostrar</param>
        /// <param name="traducir">Indica si los mensajes de la excepción se traduciran</param>
        /// <param name="tipo">Tipo del error</param>
        /// <param name="exception">Excepción interna</param>
        /// <param name="parameters">Parámetros a reemplazar en el mensaje</param>
        public GestorCalculosException(string errorCodeKey, string messageKey, string detalle, bool traducir, TipoError tipo, Exception exception, params object[] parameters)
            : base((traducir) ? BitacoraMensajesHelper.ObtenerMensajeRecursos(messageKey) : messageKey, exception)
        {
            this.Data.Add("CodigoError", traducir ? BitacoraMensajesHelper.ObtenerMensajeRecursos(errorCodeKey) : errorCodeKey);
            this.Data.Add("TipoError", tipo.ToString());
            this.Data.Add("Detalle", detalle);
        }
        #endregion
    }
}
