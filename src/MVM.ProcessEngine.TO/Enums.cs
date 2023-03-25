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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.TO
{
    /// <summary>
    /// Determina la dirección del parámetro
    /// </summary>
    public enum DireccionValor
    { 
        Ninguno,
        Entrada,
        Salida
    }

    /// <summary>
    /// Determina el tipo de variable
    /// </summary>
    public enum TipoVariable
    { 
        Ninguno,
        Global,
        Local,
        Semilla,
        Interna,
        SemillaInterna
    }

    /// <summary>
    /// Define el tipo de fuente de datos
    /// </summary>
    public enum TipoFuente
    { 
        Ninguno,
        Repositorio,
        Servicio,
        Archivo
    }

    /// <summary>
    /// Define el tipo de función
    /// </summary>
    public enum TipoFuncion
    {
        Ninguno,
        Sumar,
        Promedio,
        Maximo,
        Minimo,
        DiasMes,
        DiasMesPeriodos
    }

    /// <summary>
    /// Determina el tipo de dato
    /// </summary>
    public enum TipoDato
    {
        Object,
        String,
        Integer,
        Decimal,
        DateTime,
        Boolean,
        DateTimeString,
        ListString,
        DictionaryStringDecimal,
        DictionaryString
    }

    /// <summary>
    /// Indica el tipo de proveedor de base de datos utilizado
    /// </summary>
    public enum Proveedor
    {
        SQL,
        Oracle
    }

    /// <summary>
    /// Indica el tipo de condicional
    /// </summary>
    public enum Condicional
    {
        AND,
        OR
    }

    /// <summary>
    /// Indica el tipo de operador en una operación
    /// </summary>
    public enum Operador
    {
        Igual,
        Diferente,
        Mayor,
        Menor,
        MayorIgual,
        MenorIgual,
        Empieza,
        Termina,
        Contiene
    }

    /// <summary>
    /// Identifica el tipo de procesamiento de los cálculos
    /// </summary>
    public enum TipoProcesamiento
    {
        Sincrono,
        Asincrono
    }

    /// <summary>
    /// Identifica la alineación de un texto en un archivo de salida
    /// </summary>
    public enum AlineacionTexto
    {
        Izquierda,
        Derecha
    }

    /// <summary>
    /// Indica el estado de un proceso de cálculo
    /// </summary>
    public enum EstadoProceso
    {
        Ejecutando,
        Completado,
        Cancelado
    }

    /// <summary>
    /// Indica los tipos de archivos disponibles
    /// </summary>
    public enum TipoArchivo
    {
        Plano,
        Excel,
        XML,
        Personalizado
    }

    /// <summary>
    /// Indica la versión de excel
    /// </summary>
    public enum VersionExcel
    {
        XLS,
        XLSX
    }

    /// <summary>
    /// Indica un tipo de redondeo en caso de valores decimales
    /// </summary>
    public enum TipoRedondeo
    {
        Defecto,
        Mayor,
        Menor
    }

    /// <summary>
    /// Determina el momoento de realizar el bulkCopy, si al finalizar un calculo o una semilla.
    /// </summary>
    public enum TipoBulkCopy
    {
        Ninguno,
        Semilla,
        Calculo
    }

    #region Logging
    /// <summary>
    /// Categorías de log
    /// </summary>
    public enum LogCategory
    {
        [Description("Errors")]
        Error,
        [Description("LogErrors")]
        LogErrors,
        [Description("Warnings")]
        Warning,
        [Description("Information")]
        Information,
        [Description("Debug")]
        Debug,
        [Description("Audit")]
        Audit,
        [Description("General")]
        General,
        /// <summary>
        /// Para los tipos de acción de traza del sistema.
        /// </summary>
        [Description("Trace")]
        Trace
    }

    /// <summary>
    /// Severidad en el registro de log
    /// </summary>
    public enum LogSeverity
    {
        [Description("Critical")]
        Critical = 1,
        [Description("Error")]
        Error = 2,
        [Description("Warning")]
        Warning = 4,
        [Description("Information")]
        Information = 8,
        [Description("Verbose")]
        Verbose = 16,
        [Description("Start")]
        Start = 256,
        [Description("Stop")]
        Stop = 512,
        [Description("Suspend")]
        Suspend = 1024,
        [Description("Resume")]
        Resume = 2048,
        [Description("Transfer")]
        Transfer = 4096
    }
    /// <summary>
    /// Tipos de acciones para registro de logs.
    /// </summary>
    public enum TiposAccion
    {
        /// <summary>
        /// Para los tipos de acción de consulta.
        /// </summary>
        [Description("Consulta")]
        Consulta,
        /// <summary>
        /// Para los tipos de acción de creación.
        /// </summary>
        [Description("Creacion")]
        Creacion,
        /// <summary>
        /// Para los tipos de acción de eliminación.
        /// </summary>
        [Description("Eliminacion")]
        Eliminacion,
        /// <summary>
        /// Para los tipos de acción de modificación.
        /// </summary>
        [Description("Modificacion")]
        Modificacion,
        /// <summary>
        /// Para los tipos de acción de notificación.
        /// </summary>
        [Description("Notificacion")]
        Notificacion
    }

    /// <summary>
    /// Indica los tipos de error
    /// </summary>
    public enum TipoError
    {
        Negocio,
        Tecnico
    }
    #endregion
}
