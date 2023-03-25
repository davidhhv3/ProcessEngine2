#region Derechos Reservados
// ===================================================
// Desarrollado Por	    : Juan Esteban Giraldo Gómez
// Fecha de Creación	: 2015-02-06
// Modificado Por       :
// Fecha Modificación   :
// Empresa		        : MVM S.A.S
// ===================================================
#endregion

#region Referencias
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization; 
#endregion

namespace MVM.ProcessEngine.TO
{
    /// <summary>
    /// Data contract para retornar fallas en integración
    /// </summary>
    [DataContract]
    public class GestorCalculosIntegrationFault
    {
        /// <summary>
        /// Obtiene o establece el código de error
        /// </summary>
        [DataMember]
        public string CodigoError { get; set; }
        /// <summary>
        /// Obtiene o establece el mensaje del error
        /// </summary>
        [DataMember]
        public string Mensaje { get; set; }
        /// <summary>
        /// Obtiene o establece el tipo de error
        /// </summary>
        [DataMember]
        public string TipoError { get; set; }
        /// <summary>
        /// Obtiene o establece el detalle del error
        /// </summary>
        [DataMember]
        public string Detalle { get; set; }
    }
}
