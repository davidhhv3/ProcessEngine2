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
using System.Xml.Serialization;

namespace MVM.ProcessEngine.TO
{
    /// <summary>
    /// Clase que representa la fuente de información
    /// </summary>
    [DataContract]
    [Serializable]
    public class FuenteTO
    {
        /// <summary>
        /// Obtiene o establece un valor indicando el identificador de la fuente de información
        /// </summary>
        [DataMember]
        [XmlAttribute("id")]
        public string ID { get; set; }
        /// <summary>
        /// Obtiene o establece un valor indicando el nombre de la fuente de información
        /// </summary>
        [DataMember]
        [XmlAttribute("nombre")]
        public string Nombre { get; set; }
        /// <summary>
        /// Obtiene o establece la lista de parámetros
        /// </summary>
        [DataMember]
        [XmlArray("parametros")]
        [XmlArrayItem("parametro", typeof(ParametroTO))]
        public List<ParametroTO> Parametros { get; set; }
        
        /// <summary>
        /// Obtiene o establece el tipo de fuente de dato
        /// </summary>
        [DataMember]
        public TipoFuente Tipo { get; set; }
    }
}
