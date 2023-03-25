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
using System.ComponentModel;

namespace MVM.ProcessEngine.TO
{
    /// <summary>
    /// Clase que representa una variable determinada en un cálculo
    /// </summary>
    [DataContract]
    [Serializable]
    public class VariableTO
    {
        /// <summary>
        /// Inicia una nueva instancia de <see cref="MVM.ProcessEngine.TO.VariableTO"/>
        /// </summary>
        public VariableTO()
        {
            //Default Values - Deserialización
            EjecutarSemillaEnParalelo = false;
            HabilitarReporte = true;
        }

        /// <summary>
        /// Obtiene o establece el identificador de la variable
        /// </summary>
        [DataMember]
        [XmlAttribute("id")]
        public string ID { get; set; }
        /// <summary>
        /// Obtiene o establece el nombre de la variable
        /// </summary>
        [DataMember]
        [XmlAttribute("nombre")]
        public string Nombre { get; set; }
        /// <summary>
        /// Obtiene o establece el tipo de dato del campo
        /// </summary>
        [DataMember]
        [XmlAttribute("tipoDato")]
        public TipoDato TipoDato { get; set; }
        /// <summary>
        /// Obtiene o establece el valor por defecto a ingresar si en el campo viene nulo
        /// </summary>
        [DataMember]
        [XmlAttribute("valorDefecto")]
        public string ValorDefecto { get; set; }
        /// <summary>
        /// Obtiene o establece un valor constante para la variable
        /// </summary>
        [DataMember]
        [XmlAttribute("constante")]
        public string Constante { get; set; }
        /// <summary>
        /// Obtiene o establece el número de digitos flotantes para truncar un decimal
        /// </summary>
        [DataMember]
        [XmlIgnore]
        public int? DigitosFlotantes { get; set; }

        [XmlAttribute("digitosFlotantes")]
        public string DigitosFlotantesAsText
        {
            get
            {
                return (DigitosFlotantes.HasValue) ? DigitosFlotantes.ToString() : null;
            }
            set
            {
                DigitosFlotantes = !string.IsNullOrEmpty(value) ? int.Parse(value) : default(int?);
            }
        }
        /// <summary>
        /// Obtiene o establece el tipo de redondeo para truncar un decimal
        /// </summary>
        [DataMember]
        [XmlAttribute("tipoRedondeo")]
        public TipoRedondeo TipoRedondeo { get; set; }
        /// <summary>
        /// Indica si la variable admite núlos
        /// </summary>
        [DataMember]
        [XmlAttribute("permitirNulos")]
        public bool PermitirNulos { get; set; }
        /// <summary>
        /// Obtiene o establece un valor indicando el tipo de formato a aplicar a una cadena de texto
        /// </summary>
        /// <example>YYYY/MM/DD HH:mm:SS</example>
        [DataMember]
        [XmlAttribute("formato")]
        public string Formato { get; set; }
        /// <summary>
        /// Obtiene o establece el tipo de dirección de la variable
        /// </summary>
        /// <example>Entrada|Salida</example>
        [DataMember]
        [XmlAttribute("direccion")]
        public DireccionValor Direccion { get; set; }
        /// <summary>
        /// Obtiene o establece un valor indicando si la variable se debe o no almacenar en la fuente especificada
        /// </summary>
        [DataMember]
        [XmlAttribute("almacenar")]
        public bool Almacenar { get; set; }
        /// <summary>
        /// Obtien o establece el identificador de la fuente de información
        /// </summary>
        [DataMember]
        [XmlAttribute("fuente")]
        public string IdFuente { get; set; }
        /// <summary>
        /// Obtiene o establece la lista de equivalencias asociadas a la variable
        /// </summary>
        [DataMember]
        [XmlArray("equivalencias")]
        [XmlArrayItem("equivalencia", typeof(EquivalenciaTO))]
        public List<EquivalenciaTO> Equivalencias { get; set; }
        /// <summary>
        /// Obtiene o establece el tipo de variable
        /// </summary>
        /// <remarks>Cuando se itera, los tipos de variables Globales no se vuelven a consultar desde el repositorio</remarks>
        [DataMember]
        [XmlAttribute("tipoVariable")]
        public TipoVariable TipoVariable { get; set; }
        /// <summary>
        /// Obtiene o establece el tipo de función
        /// </summary>
        [DataMember]
        [XmlAttribute("funcion")]
        public TipoFuncion TipoFuncion { get; set; }
        /// <summary>
        /// Indica si se debe ejecutar el cálculo por iteración
        /// </summary>
        /// <remarks>Aplica cuando el tipo de variable es semilla</remarks>
        [DataMember]
        [XmlAttribute("ejecutarCalculoPorIteracion")]
        public bool EjecutarPorIteracion { get; set; }
        /// <summary>
        /// Obtiene o establece una formula interna que se evaluara por variable
        /// </summary>
        [DataMember]
        [XmlAttribute("formulaInterna")]
        public string FormulaInterna { get; set; }
        /// <summary>
        /// Indica si se debe ejecutar el cálculo por iteración
        /// </summary>
        /// <remarks>Aplica cuando el tipo de variable es semilla</remarks>
        [DataMember]
        [XmlAttribute("ejecutarSemillaEnParalelo")]
        public bool EjecutarSemillaEnParalelo { get; set; }
        /// <summary>
        /// Indica si se debe habilitar la impresión del resultado de la semilla o no
        /// </summary>
        /// <remarks>Aplica cuando el tipo de variable es semilla</remarks>
        [DataMember]
        [XmlAttribute("habilitarReporte")]
        public bool HabilitarReporte { get; set; }

        /// <summary>
        /// Define el nombre del repositorio de las entradas.
        /// </summary>
        [DataMember]
        [XmlAttribute("fuenteConsultaEntrada")]
        public string FuenteConsultaEntrada { get; set; }

        /// <summary>
        /// Define el nombre de una funcion Externa. Llevada a cabo en un proyecto plugin o Extension
        /// </summary>
        [DataMember]
        [XmlAttribute("funcionExterna")]
        public string FuncionExterna { get; set; }
    }
}
