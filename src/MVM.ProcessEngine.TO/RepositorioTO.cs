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
    /// Clase que representa la fuente de información de un repositorio o base de datos
    /// </summary>
    [DataContract]
    [Serializable]
    public class RepositorioTO : FuenteTO
    {
        /// <summary>
        /// Crea una nueva instancia del tipo <see cref="MVM.ProcessEngine.TO.RepositorioTO"/>
        /// </summary>
        public RepositorioTO()
        {
            Tipo = TipoFuente.Repositorio;
        }

        /// <summary>
        /// Obtiene o establece el nombre de la cadena de conexión a utilizar
        /// </summary>
        /// <remarks>Esta cadena de conexión debe existir en el archivo de configuración del servicio web del gestor de cálculos</remarks>
        [DataMember]
        [XmlAttribute("nombreCadenaConexion")]
        public string NombreCadenaConexion { get; set; }
        /// <summary>
        /// Obtiene o establece la cadena de caracteres con la consulta SQL
        /// </summary>
        [DataMember]
        [XmlAttribute("sql")]
        public string Sql { get; set; }
        /// <summary>
        /// Obtiene o establece el nombre del procedimiento almacenado a utilizar
        /// </summary>
        [DataMember]
        [XmlAttribute("nombreProcedimiento")]
        public string NombreProcedimiento { get; set; }
        /// <summary>
        /// Obtiene o establece un valor indicando si se debe habilitar o no la actualización
        /// </summary>
        [DataMember]
        [XmlAttribute("habilitarActualizacion")]
        public bool HabilitarActualizacion { get; set; }
        /// <summary>
        /// Obtiene o establece la consulta SQL en caso de habilitar la actualización
        /// </summary>
        /// <remarks>Debe tener definido un valor en caso de tener la propiedad HabilitarActualizacion en true y no tener definido el procedimiento almacenado
        /// de actualización</remarks>
        [DataMember]
        [XmlAttribute("sqlActualizacion")]
        public string SqlActualizacion { get; set; }
        /// <summary>
        /// Obtiene o establece el nombre del procedimiento almacenado de actualización a utilizar
        /// </summary>
        /// <remarks>Debe tener definido un valor en caso de tener la propiedad HabilitarActualizacion en true y no tener definido el SQL de actualización</remarks>
        [DataMember]
        [XmlAttribute("nombreProcedimientoActualizacion")]
        public string NombreProcedimientoActualizacion { get; set; }
        /// <summary>
        /// Obtiene o establece un valor indicando si se debe envolver un conjunto de inserts o update en el contexto de una transacción
        /// </summary>
        [DataMember]
        [XmlAttribute("habilitarTransaccion")]
        public bool HabilitarTransaccion { get; set; }
        /// <summary>
        /// Obtiene o establece la fuente en Meoria asociada a esta fuente
        /// </summary>
        [DataMember]
        [XmlAttribute("fuenteMemoria")]
        public string IdFuenteMemoria { get; set; }
    }
}
