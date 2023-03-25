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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MVM.ProcessEngine.Common.Exceptions;
using Spring.Context;
using Spring.Context.Support;

namespace MVM.ProcessEngine.Common.Helpers
{
    /// <summary>
    /// Clase utilizada para obtener la referencia a los servicios disponibles
    /// </summary>
    /// <typeparam name="T">Tipo de servicio a obtener</typeparam>
    public class GestorCalculosServiceLocator
    {
       
         static GestorCalculosServiceLocator() {
            XmlApplicationContext ctx = new XmlApplicationContext("assembly://MVM.ProcessEngine.Common/MVM.ProcessEngine.Common/spring.xml");
            ContextRegistry.RegisterContext(ctx);
        }
        /// <summary>
        /// Retorna la instancia del servicio en el contexto
        /// </summary>
        /// <param name="nombreServicio">El nombre del servicio en el contenedor</param>
        /// <returns>Instancia del servicio solicitado</returns>
        public static T GetService<T>()
        {
            return GetService<T>(ContextRegistry.GetContext(), false, null);
        }

        /// <summary>
        /// Retorna la instancia del servicio en el contexto
        /// </summary>
        /// <param name="target">El nombre del servicio en el contenedor</param>
        /// <returns>Instancia del servicio solicitado</returns>
        public static T GetService<T>(string target)
        {
            
            return GetService<T>(ContextRegistry.GetContext(), false, target);
        }

        /// <summary>
        /// Retorna la instancia del servicio en el contexto
        /// </summary>
        /// <param name="nombreServicio">El nombre del servicio en el contenedor</param>
        /// <param name="target">Nombre del servicio específico a buscar</param>
        /// <returns>Instancia del servicio solicitado</returns>
        public static T GetService<T>(IApplicationContext context, bool throwException, string target)
        {
            object result = GetService(typeof(T), context, throwException, target);

            //object result = context.GetObject(target);


            if (result != null)
                return (T)result;
            else
                return default(T);
        }

        /// <summary>
        /// Retorna la instancia del servicio acorde al tipo
        /// </summary>
        /// <param name="serviceType">Tipo a buscar</param>
        /// <param name="context">Contexto a utilizar</param>
        /// <param name="throwException">Indica si debe arrojar excepción en caso de no encontrar el servicio</param>
        /// <param name="target">Nombre del servicio específico a buscar</param>
        /// <returns>Instancia del servicio solicitado</returns>
        public static object GetService(Type serviceType, IApplicationContext context, bool throwException, string target)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (serviceType == null)
                throw new ArgumentNullException("serviceType");

            IDictionary dictionary = context.GetObjectsOfType(serviceType);

            if (dictionary != null && dictionary.Count > 0)
            {
                if (dictionary.Count == 1)
                {
                    //retorna el primero de los objetos de ese tipo ya que no se da el caso una misma interface registrada dos veces
                    IEnumerator enumerator = dictionary.Values.GetEnumerator();
                    enumerator.MoveNext();
                    return enumerator.Current;
                }
                else
                {
                    if (string.IsNullOrEmpty(target))
                        throw new GestorCalculosException("GestorCalculosError_ServicioNoEncontrado", new ArgumentNullException("target"), serviceType.Name);

                    //si hay mas de un servicio registrado con la misma interface se procede a buscar el objeto cuyo nombre
                    //contenga la palabra indicada en el parámetro target
                    foreach (object key in dictionary.Keys)
                    {
                        var serviceName = (string)key;
                        if (serviceName.Contains(target))
                        {
                            return dictionary[key];
                        }
                    }
                }
            }

            //Si no se encontro ningun servicio, se verifica si se arroja una excepción
            if (throwException)
            {
                throw new GestorCalculosException("GestorCalculosError_ServicioNoEncontrado", serviceType.Name);
            }
            else
            {
                return null;
            }
        }
    }
}
