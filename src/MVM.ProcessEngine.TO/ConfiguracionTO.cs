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
    /// Clase que representa un archivo de configuración de cálculos
    /// </summary>
    [DataContract]
    [Serializable]
    [XmlRoot(ElementName="configuracion")]
    public class ConfiguracionTO
    {
        /// <summary>
        /// Obtiene o establece el nombre de la configuración
        /// </summary>
        [DataMember]
        [XmlAttribute("nombre")]
        public string Nombre { get; set; }
        /// <summary>
        /// Obtiene o establece la cultura por default que se establecera en el procesamiento de los cálculos
        /// </summary>
        /// <example>en-US</example>
        [DataMember]
        [XmlAttribute("cultura")]
        public string Cultura { get; set; }
        /// <summary>
        /// Obtiene o establece la ruta donde se encuentra el archivo de las variables globales
        /// </summary>
        [DataMember]
        [XmlAttribute("rutaArchivoVariablesGlobales")]
        public string RutaArchivoVariablesGlobales { get; set; }
        /// <summary>
        /// Obtiene o establece el tipo de proveedor de acceso a datos
        /// </summary>
        [DataMember]
        [XmlAttribute("proveedor")]
        public Proveedor TipoProveedor { get; set; }
        /// <summary>
        /// Obtiene o establece un valor indicando si se habilita o no la posibilidad de procesar los cálculos en paralelo
        /// </summary>
        [DataMember]
        [XmlAttribute("ejecutarCalculosEnParalelo")]
        public bool EjecutarCalculosEnParalelo { get; set; }
        /// <summary>
        /// Obtiene o establece las fuentes de información
        /// </summary>
        [DataMember]
        [XmlElement("fuentes")]
        public FuentesContainerTO Fuentes { get; set; }
        /// <summary>
        /// Obtiene o establece los grupos
        /// </summary>
        [DataMember]
        [XmlArray("grupos")]
        [XmlArrayItem("grupo", typeof(GrupoTO))]
        public List<GrupoTO> Grupos { get; set; }
        /// <summary>
        /// Obtiene o establece la lista de cálculos de la configuración
        /// </summary>
        [DataMember]
        [XmlArray("calculos")]
        [XmlArrayItem("calculo", typeof(CalculoTO))]
        public List<CalculoTO> Calculos { get; set; }

        /// <summary>
        /// Obtiene o establece los parametros q seran enviados al proceso de cálculo
        /// </summary>
        [DataMember]
        public List<object> Parametros { get; set; }

        /// <summary>
        /// Obtiene o establece la fuente de la configuración inicial
        /// </summary>
        [DataMember]
        [XmlAttribute("fuenteConfiguracionInicial")]
        public string FuenteConfiguracionInicial { get; set; }

        /// <summary>
        /// Obtiene o establece la fuente para actualizar la versión.
        /// </summary>
        [DataMember]
        [XmlAttribute("fuenteActualizacionVersion")]
        public string FuenteActualizacionVersion { get; set; }

        /// <summary>
        /// Obtiene o establece la fuente para relacionar la entradas con la ejecución del proceso.
        /// </summary>
        [DataMember]
        [XmlAttribute("fuenteVersionEntradas")]
        public string FuenteVersionEntradas { get; set; }
    }
}
