#region Derechos Reservados
// ===================================================
// Desarrollado Por	    : Juan Esteban Giraldo Gómez
// Fecha de Creación	: 2015-02-24
// Modificado Por       : Juan Esteban Giraldo Gómez
// Fecha Modificación   : 2015-02-24
// Empresa		        : MVM S.A.S
// ===================================================
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.TO
{
    /// <summary>
    /// Clase que representa el objeto semilla
    /// </summary>
    [DataContract]
    [Serializable]
    public class SemillaTO
    {
        /// <summary>
        /// Obtiene o establece el valor principal de la semilla
        /// </summary>
        [DataMember]
        public string ValorPrincipal { get; set; }
        /// <summary>
        /// Obtiene o establece el valor secundario de la semilla
        /// </summary>
        [DataMember]
        public string ValorSecundario { get; set; }

        public override string ToString()
        {
            return "[" + ValorPrincipal + "," + ValorSecundario + "]";
        }
    }
}
