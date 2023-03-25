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
    /// Representa un los valor del archivo que al cargarse debería reemplazarse por otro
    /// </summary>
    [DataContract]
    public class EquivalenciaTO
    {
        /// <summary>
        /// Valor de alguno de los campos del archivo
        /// </summary>
        [DataMember]
        [XmlAttribute("valorOriginal")]
        public string ValorOriginal { get; set; }
        /// <summary>
        /// Valor por el que se debe reemplazar al insertarse en la tabla destino
        /// </summary>
        [DataMember]
        [XmlAttribute("valorNuevo")]
        public string ValorNuevo { get; set; }
    }
}
