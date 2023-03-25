#region Derechos Reservados
// ===================================================
// Creado Por       : Juan Esteban Giraldo G.
// Fecha Creación   : 2010-05-10  
// Empresa		    : MVM Ingeniería de Software S.A.
// ===================================================
#endregion

#region Referencias
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
#endregion

namespace MVM.ProcessEngine.TO
{
    /// <summary>
    /// Representa un mensaje en el proceso
    /// </summary>
    public class MensajeProcesoTO
    {
        /// <summary>
        /// Obtiene o establece el identificador del proceso al que pertenece el mensaje
        /// </summary>
        public string IdentificadorProceso { get; set; }
        /// <summary>
        /// Obtiene o establece el mensaje o un campo clave el cual debe ser mapeado contra una hoja de recursos
        /// </summary>
        public string TextoMensaje { get; set; }
        /// <summary>
        /// Obtiene o establece un valor indicando si se debe traducir el texto de mensaje contra una hoja de recursos
        /// </summary>
        public bool Traducir { get; set; }
    }
}