#region Derechos Reservados
// ===================================================
// Desarrollado Por	    : Juan Esteban Giraldo Gómez
// Fecha de Creación	: 2015-02-06
// Modificado Por       : Juan Esteban Giraldo Gómez
// Fecha Modificación   : 2015-02-28
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
    /// Clase que representa la fuente de información de un servicio web
    /// </summary>
    public class ServicioTO : FuenteTO
    {
        /// <summary>
        /// Crea una nueva instancia del tipo <see cref="MVM.ProcessEngine.TO.ServicioTO"/>
        /// </summary>
        public ServicioTO()
        {
            Tipo = TipoFuente.Servicio;
        }

        /// <summary>
        /// Obtiene o establece la dirección wsdl del servicio
        /// </summary>
        /// <remarks>Debe ser la dirección del wsdl</remarks>
        /// <example>http://mvmsop01/CalculoService.svc?wsdl</example>
        [XmlAttribute("wsdl")]
        public string Wsdl { get; set; }
        /// <summary>
        /// Obtiene o establece el método del servicio a consumir
        /// </summary>
        [XmlAttribute("metodo")]
        public string Metodo { get; set; }
    }
}
