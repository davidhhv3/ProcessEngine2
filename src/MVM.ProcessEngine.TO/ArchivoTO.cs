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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.TO
{
    /// <summary>
    /// Clase que representa una fuente de información de archivo
    /// </summary>
    [DataContract]
    [Serializable]
    public class ArchivoTO : FuenteTO
    {
        /// <summary>
        /// Obtiene o establece un valor indicando el tipo de archivo
        /// </summary>
        [DataMember]
        public TipoArchivo TipoArchivo { get; set; }
        /// <summary>
        /// Obtiene o establece un valor indicando la ruta del archivo
        /// </summary>
        [DataMember]
        public string Ruta { get; set; }
    }
}
