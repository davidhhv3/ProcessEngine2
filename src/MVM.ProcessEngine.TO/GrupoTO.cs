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
using System.Xml.Serialization;

namespace MVM.ProcessEngine.TO
{
    /// <summary>
    /// Clase que representa una entidad de grupo
    /// </summary>
    [DataContract]
    [Serializable]
    public class GrupoTO
    {
        /// <summary>
        /// Obtiene o establece el identificaador del grupo
        /// </summary>
        [DataMember]
        [XmlAttribute("id")]
        public string ID { get; set; }
        /// <summary>
        /// Obtiene o establece el nombre del grupo
        /// </summary>
        [DataMember]
        [XmlAttribute("nombre")]
        public string Nombre { get; set; }
        /// <summary>
        /// Obtiene o establece el orden de ejecución del grupo
        /// </summary>
        [DataMember]
        [XmlAttribute("orden")]
        public int Orden { get; set; }
    }
}
