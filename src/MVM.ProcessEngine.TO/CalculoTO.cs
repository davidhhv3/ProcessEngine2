#region Derechos Reservados
// ===================================================
// Desarrollado Por	    : Juan Esteban Giraldo Gómez
// Fecha de Creación	: 2015-02-06
// Modificado Por       : Juan Esteban Giraldo Gómez
// Fecha Modificación   : 2015-02-04
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
using System.ComponentModel;

namespace MVM.ProcessEngine.TO
{
    /// <summary>
    /// Clase que representa un cálculo
    /// </summary>
    [DataContract]
    [Serializable]
    public class CalculoTO
    {
        /// <summary>
        /// Obtiene o establece el identificador del cálculo
        /// </summary>
        [DataMember]
        [XmlAttribute("id")]
        public string ID { get; set; }
        /// <summary>
        /// Obtiene o establece el nombre del cálculo
        /// </summary>
        [DataMember]
        [XmlAttribute("nombre")]
        public string Nombre { get; set; }
        /// <summary>
        /// Obtiene o establece la fórmula del cálculo
        /// </summary>
        /// <remarks>Los valores encerrados en llave, indican variables que deben estar especificadas en la lista de variables 
        /// asociadas al cálculo</remarks>
        /// <example>{PREFACPR}={CMPPBR}*{FACNODCP}</example>
        [DataMember]
        [XmlAttribute("formula")]
        public string Formula { get; set; }
        /// <summary>
        /// Obtiene o establece un valor indicando una dependencia con un cálculo anterior
        /// </summary>
        [DataMember]
        [XmlAttribute("idDependencia")]
        public string IdDependencia { get; set; }
        /// <summary>
        /// Obtiene o establece un valor indicando una dependencia con un grupo específico
        /// </summary>
        [DataMember]
        [XmlAttribute("idDependenciaGrupo")]
        public string IdDependenciaGrupo { get; set; }
        /// <summary>
        /// Obtiene o establece el identificador del grupo
        /// </summary>
        [DataMember]
        [XmlAttribute("idGrupo")]
        public string IdGrupo { get; set; }
        /// <summary>
        /// Obtiene o establece el orden de ejecución del cálculo
        /// </summary>
        [DataMember]
        [XmlAttribute("orden")]
        public int Orden { get; set; }
        /// <summary>
        /// Obtiene o establece las variables del cálculo
        /// </summary>
        [DataMember]
        [XmlArray("variables")]
        [XmlArrayItem("variable", typeof(VariableTO))]
        public List<VariableTO> Variables { get; set; }
        /// <summary>
        /// Obtiene o establece el identificador de grupo
        /// </summary>
        [DataMember]
        [XmlIgnore]
        public GrupoTO Grupo { get; set; }
        /// <summary>
        /// Obtiene o establece un valor indicando si el cálculo fue exitoso o no
        /// </summary>
        /// <remarks>Propiedad interna para el manejo de las dependencias</remarks>
        [XmlIgnore]
        public bool Exitoso { get; set; }
        /// <summary>
        /// Obtiene o establece un valor indicando si se debe ejecutar el cálculo o no
        /// </summary>
        [DataMember]
        [DefaultValue(true), XmlAttribute("ejecutar")]
        public bool Ejecutar { get; set; }

        /// <summary>
        /// Obtiene o establece el tipo de bulkcopy (Semilla o Cálculo)
        /// </summary>
        /// <example>Semilla|Calculo</example>
        [DataMember]
        [XmlAttribute("bulkCopy")]
        public TipoBulkCopy BulkCopy { get; set; }

        /// <summary>
        /// Obtiene o establece el tipo de bulkcopy (Semilla o Cálculo)
        /// </summary>
        /// <example>Semilla|Calculo</example>
        [DataMember]
        [XmlAttribute("fuenteConsultaSalida")]
        public string FuenteConsultaSalida { get; set; }
    }
}
