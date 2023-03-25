#region Derechos Reservados
// ===================================================
// Desarrollado Por	    : Juan Esteban Giraldo Gómez
// Fecha de Creación	: 2015-02-06
// Modificado Por       : Juan Esteban Giraldo Gómez
// Fecha Modificación   : 2015-03-03
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
    /// Contenedor de las fuentes de información
    /// </summary>
    [Serializable]
    [DataContract]
    public class FuentesContainerTO
    {
        /// <summary>
        /// Obtiene o establece la lista de servicios
        /// </summary>
        [DataMember]
        [XmlArray("servicios")]
        [XmlArrayItem("servicio", typeof(ServicioTO))]
        public List<ServicioTO> Servicios { get; set; }

        /// <summary>
        /// Obtiene o establece la lista de repositorios
        /// </summary>
        [DataMember]
        [XmlArray("repositorios")]
        [XmlArrayItem("repositorio", typeof(RepositorioTO))]
        public List<RepositorioTO> Repositorios { get; set; }
    }
}
