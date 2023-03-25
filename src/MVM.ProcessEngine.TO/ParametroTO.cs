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
using System.Xml.Serialization;

namespace MVM.ProcessEngine.TO
{
    /// <summary>
    /// Representa un parámetro en la fuente de información
    /// </summary>
    [DataContract]
    [Serializable]
    public class ParametroTO
    {
        /// <summary>
        /// Constructor por defecto.
        /// </summary>
        public ParametroTO()
        {
            Direccion = DireccionValor.Entrada;
        }

        /// <summary>
        /// Obtiene o establece el nombre del parámetro
        /// </summary>
        [DataMember]
        [XmlAttribute("nombre")]
        public string Nombre { get; set; }
        /// <summary>
        /// Obtiene o establece el valor del parámetro
        /// </summary>
        /// <remarks>Cuando es enviado un substring de este tipo [PARAMETRO(X)], es por que 
        /// se esta enviando el valor del parámetro desde la invocación del servicio en la posición que indique la variable X</remarks>
        [DataMember]
        [XmlAttribute("valor")]
        public string Valor { get; set; }
        /// <summary>
        /// Obtiene o establece el valor del parámetro por defecto
        /// </summary>
        /// <remarks>Cuando es enviado un substring de este tipo [PARAMETRO(X)], es por que 
        /// se esta enviando el valor del parámetro desde la invocación del servicio en la posición que indique la variable X</remarks>
        [DataMember]
        [XmlAttribute("defecto")]
        public string Defecto { get; set; }
        /// <summary>
        /// Obtiene o establece un valor indicando el tipo de formato a aplicar a una cadena de texto
        /// </summary>
        /// <example>YYYY/MM/DD HH:mm:SS</example>
        [DataMember]
        [XmlAttribute("formato")]
        public string Formato { get; set; }
        /// <summary>
        /// Obtiene o establece un valor indicando el tipo de dato del parámetro
        /// </summary>
        [DataMember]
        [XmlAttribute("tipoDato")]
        public TipoDato TipoDato { get; set; }
        /// <summary>
        /// Obtiene o establece la dirección del parámetro
        /// </summary>
        /// <example>Entrada</example>
        [DataMember]
        [XmlAttribute("direccion")]
        public DireccionValor Direccion { get; set; }
        /// <summary>
        /// Obtiene o establece el número de digitos flotantes para truncar un decimal
        /// </summary>
        [DataMember]
        [XmlIgnore]
        public int? DigitosFlotantes { get; set; }

        [XmlAttribute("digitosFlotantes")]
        public string DigitosFlotantesAsText {
            get
            {
                return (DigitosFlotantes.HasValue) ? DigitosFlotantes.ToString() : null;
            }
            set {
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
        /// Obtiene o establece el tamaño del campo
        /// </summary>
        [DataMember]
        [XmlAttribute("tamano")]
        public int Tamano { get; set; }
        /// <summary>
        /// Obtiene o establece un valor indicando si permite nulos
        /// </summary>
        [DataMember]
        [XmlAttribute("permitirNulos")]
        public bool PermitirNulos { get; set; }
    }
}
