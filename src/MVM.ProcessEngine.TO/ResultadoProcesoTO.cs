#region Derechos Reservados
// ===================================================
// Desarrollado Por	    : Juan Esteban Giraldo Gómez
// Fecha de Creación	: 2015-02-06
// Modificado Por       :
// Fecha Modificación   :
// Empresa		        : MVM S.A.S
// ===================================================
#endregion
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.TO
{
    /// <summary>
    /// Clase que representa la entidad que contiene los resultados del proeso
    /// </summary>
    [DataContract]
    [Serializable]
    public class ResultadoProcesoTO
    {
        /// <summary>
        /// Indica si fue exitoso la operación
        /// </summary>
        [DataMember]
        public bool Exitoso { get; set; }
        /// <summary>
        /// Mensaje personalizado de la consulta realizada sobre el proceso
        /// </summary>
        [DataMember]
        public string Mensaje { get; set; }
        /// <summary>
        /// Indica el estado actual del proceso
        /// </summary>
        [DataMember]
        public EstadoProceso Estado { get; set; }
        /// <summary>
        /// Obtiene el detalle de los mensajes del proceso
        /// </summary>
        [DataMember]
        public string[] DetalleMensajes { get; set; }
    }
}
