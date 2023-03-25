using System;
using System.Collections.Generic;
using MVM.ProcessEngine.TO;
using System.Data;
using System.Threading.Tasks;

namespace MVM.ProcessEngine.Interfaces
{
    /// <summary>
    /// Interface que contiene la firma de las operaciones del proceso de cálculos
    /// </summary>
    public interface ICalculoService
    {
        /// <summary>
        /// Ejecuta el proceso de cálculo y retorna un identificador
        /// </summary>
        /// <param name="nombreConfiguracionArchivo">Nombre del archivo de configuración que contiene el o los cálculos a procesar</param>
        /// <param name="idGrupo">Nombre del identificador de grupo específico a procesar</param>
        /// <param name="idCalculo">Nombre del cálculo específico a procesar</param>
        /// <param name="modoAsincrono">Indica si se habilita o no la posibilidad de procesar los cálculos de modo asíncrono</param>
        /// <param name="procesarCalculosEnParalelo">Indica si se habilita o no la posibilidad de procesar los cálculos en paralelo de acuerdo a las dependencias</param>
        /// <param name="interrumpirCargaPorExcepcion">True si se debe interrumpir el proceso de cálculo en caso de una excepción, de lo contrario el sistema
        /// continua el proceso de cálculo e identifica los registros que no pudieron ser procesados</param>
        /// <param name="parametros">Arreglo de parámetros para el cálculo</param>
        ///<returns>Retorna cadena de caracteres con el identificador asignado al cálculo</returns>
        string EjecutarCalculo(string tenant, string nombreConfiguracionArchivo, string idGrupo, string idCalculo, bool modoAsincrono, bool procesarCalculosEnParalelo, bool interrumpirCalculoPorExcepcion, params object[] parametros);

        /// <summary>
        /// Cancela el proceso del cálculo
        /// </summary>
        /// <param name="identificador">Identificador del cálculo a cancelar</param>
        /// <remarks>Esta operación se puede utizar, solo en caso de que el proceso de cálculo se ejecute de forma asíncrona</remarks>
        void CancelarCalculo(string tenant,string identificador);


        /// <summary>
        /// Obtiene data de un repositorio especifico, luego de ejecutado el proceso, usado para consultar Salidas y Entradas generales un calculo
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="configuracion"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        DataTable ObtenerDataRepositorio(string tenant, RepositorioTO repository, ConfiguracionTO configuracion, List<object> buffer);

        /// <summary>
        /// Ejecuta un repositorio específico.
        /// </summary>
        /// <param name="repository">Repositorio que se quiere ejecutar.</param>
        /// <param name="configuracion">Archivo xml serializado.</param>
        /// <param name="buffer">Buffer de parámetros.</param>
        void EjecutarRepositorio(string tenant, RepositorioTO repository, ConfiguracionTO configuracion, List<object> buffer);

        Task<string> GuardarDataRepositorio(string tenant, string activityName, RepositorioTO repository, ConfiguracionTO configuracion, List<object> buffer);
    }
}
