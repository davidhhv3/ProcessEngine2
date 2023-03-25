#region Derechos Reservados
// ===================================================
// Desarrollado Por	    : esteban.giraldo
// Fecha de Creación	: 2015-02-06
// Modificado Por       : esteban.giraldo
// Fecha Modificación   : 2015-03-06
// Empresa		        : MVM S.A.S
// ===================================================
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCalc;
using MVM.ProcessEngine.Common.Exceptions;
using MVM.ProcessEngine.Common.Helpers;
using MVM.ProcessEngine.TO;
using System.Diagnostics;
using System.Threading;
using MVM.ProcessEngine.Core.DataAccess;
using MVM.ProcessEngine.Interfaces;
using System.Collections.Concurrent;

namespace MVM.ProcessEngine.Core
{
    /// <summary>
    /// Gestiona el proceso del cálculo
    /// </summary>
    public class GestorCalculo
    {
        #region Campos
        private const string NOMBRE_PROCESO = "EjecutarCalculos";
        private const string MENSAJE_EXITO = "OK";
        private const string MENSAJE_ERROR = "ERROR";
        private const string CONSTANTE_SUBEXPRESION = "SUBEX_";
        private const string FORMATO_RESULTADO_SEMILLA = "{0}, Semilla: {1}";
        private bool _procesoCompletado = false;
        private BitacoraMensajesHelper bitacoraMensajes;
        private bool _publicarProgresoAsincrono = true;
        private static readonly string _timeZoneSetting = "TimeZoneKey";
        #endregion

        #region Constructor
        /// <summary>
        /// Permite crear una instancia del tipo <see cref="MVM.ProcessEngine.Core.GestorCalculo"/>
        /// </summary>
        public GestorCalculo(string tenant, string nombreCalculo)
        {
            Tenant = tenant;
            Identificador = Guid.NewGuid().ToString();
            Fuentes = new List<FuenteTO>();
            bitacoraMensajes = new BitacoraMensajesHelper(Identificador);
            bitacoraMensajes.EncolarProceso(tenant, null, nombreCalculo);
            
        }

        public GestorCalculo()
        {
        }
        #endregion

        #region Propiedades
        /// <summary>
        /// Current Tenant
        /// </summary>
        public string Tenant { get; set; }
        /// <summary>
        /// Obtiene o establece el identificador del proceso
        /// </summary>
        public string Identificador { get; set; }
        /// <summary>
        /// Obtiene o establece el identificador del grupo
        /// </summary>
        public string IdGrupo { get; set; }
        /// <summary>
        /// Obtiene o establece el identificador del cálculo
        /// </summary>
        public string IdCalculo { get; set; }
        /// <summary>
        /// Obtiene o establece el nombre de la configuración
        /// </summary>
        public string NombreConfiguracion { get; set; }
        /// <summary>
        /// Obtiene o establece un valor indicando si se debe interrumpir el proceso por excepción
        /// </summary>
        public bool InterrumpirCalculoPorExcepcion { get; set; }
        /// <summary>
        /// Obtiene o establece un valor indicando si se habilita o no la posibilidad de procesar los cálculos de modo asíncrono
        /// </summary>
        public bool ModoAsincrono { get; set; }
        /// <summary>
        /// Obtiene o establece los parámetros del cálculo
        /// </summary>
        public List<object> Parametros { get; set; }
        /// <summary>
        /// Obtiene o establece el BackgroundWorker
        /// </summary>
        public BackgroundWorker Worker { get; set; }
        /// <summary>
        /// Obtiene o establece las fuentes de datos
        /// </summary>
        public List<FuenteTO> Fuentes { get; set; }
        /// <summary>
        /// Obtiene o establece un valor indicando si se habilita el registro del detalle técnico en el log de salida
        /// </summary>
        public bool HabilitarRegistroDetalleTecnico { get; set; }
        /// <summary>
        /// Obtiene o establece un valor indicando si se habilita la impresión de variables del cálculo
        /// </summary>
        public bool HabilitadoImpresionVariables { get; set; }
        /// <summary>
        /// Permite obtener la lista de mensajes
        /// </summary>
        public string MensajesCarga
        {
            get
            {
                return bitacoraMensajes.ObtenerMensajes(Tenant);
            }
        }
        /// <summary>
        /// Permite obtener la bitácora para registros de mensajes
        /// </summary>
        public BitacoraMensajesHelper BitacoraMensajes
        {
            get
            {
                return bitacoraMensajes;
            }
        }

        /// <summary>
        /// Obtiene o establece el Service Bus asociado al gestor.
        /// </summary>
        public IMessagePublisher ServiceBus { get; set; }

        /// <summary>
        /// Diccionario de DataTabla por Calculos(Repositorios de Resultado), usado para el guardado en BulkCopy
        /// </summary>
        public Dictionary<string, DataTable> dataTableDictionary { get; set; }

        /// <summary>
        /// Diccionario de Valores en Memoria para las variables de la expresion que usan Repositorio en Memoria
        /// </summary>
        Dictionary<string, Dictionary<string, decimal>> repositoriosEnMemoriaDictionary { get; set; }

        /// <summary>
        /// Obtiene o establece un valor que indica si el proceso fue cancelado.
        /// </summary>
        public bool Cancelado { get; set; }
        #endregion

        #region Métodos Públicos
        /// <summary>
        /// Permite ejecutar el proceso de cálculo
        /// </summary>
        public void EjecutarProceso()
        {
            if (!string.IsNullOrEmpty(GestorCalculosHelper.GetMetadataValue(Tenant, "PublicarProgresoAsincrono", false)))
            {
                _publicarProgresoAsincrono = bool.Parse(GestorCalculosHelper.GetMetadataValue(Tenant, "PublicarProgresoAsincrono", false));
            }

            BitacoraMensajesHelper.UpdateCurrentCulture(Tenant);
            bitacoraMensajes.InicializarMensajes(Tenant, NOMBRE_PROCESO);
            _procesoCompletado = false; //Si termina todo el proceso de cálculo sin generar excepción, se considera exitoso

            //Se consulta la información de la configuración
            try
            {
                //Valida parámetros de entrada
                if (string.IsNullOrEmpty(NombreConfiguracion))
                    throw new ArgumentNullException("NombreConfiguracion");

                bitacoraMensajes.InsertarMensaje(Tenant,"@GestorCalculosInfo_ObtenerConfiguracionPorNombre", NombreConfiguracion);
                ConfiguracionTO configuracion = GestorCalculosConfiguracion.ObtenerConfiguracionCalculo(Tenant, NombreConfiguracion, Identificador);

                if (configuracion == null)
                    throw new InvalidOperationException();

                //Se establecen los parámetros que se utilizan en los procesos de cálculo
                configuracion.Parametros = Parametros;
                //Se unifican en la instancia actual del gestor las fuentes de información
                Fuentes.AddRange(configuracion.Fuentes.Servicios);
                Fuentes.AddRange(configuracion.Fuentes.Repositorios);

                // Se restauran los diccionarios  en Memoria (Consultas y Almacenamiento)
                repositoriosEnMemoriaDictionary = new Dictionary<string, Dictionary<string, decimal>>();

                dataTableDictionary = new Dictionary<string, DataTable>();

                //Se ejecuta el proceso de configuración inicial
                if (!string.IsNullOrEmpty(configuracion.FuenteConfiguracionInicial))
                {
                    //Se obtiene la fuente del repositorio
                    FuenteTO fuente = ObtenerFuente(configuracion.FuenteConfiguracionInicial);

                    if (fuente != null)
                    {
                        switch (fuente.Tipo)
                        {
                            case TipoFuente.Repositorio:
                                GestorCalculosServiceLocator.GetService<GestorDatosSQL>(
                                    configuracion.TipoProveedor.ToString()).ExecuteNonQuery(Tenant, (RepositorioTO)fuente, configuracion, null, Identificador);
                                break;
                            case TipoFuente.Servicio:
                                break;
                            case TipoFuente.Archivo:
                                break;
                        }
                    }
                }

                //Se ejecuta el proceso para obtener la versión de las entradas.
                if (!string.IsNullOrEmpty(configuracion.FuenteVersionEntradas))
                {
                    //Se obtiene la fuente del repositorio
                    FuenteTO fuente = ObtenerFuente(configuracion.FuenteVersionEntradas);

                    if (fuente != null)
                    {
                        switch (fuente.Tipo)
                        {
                            case TipoFuente.Repositorio:
                                GestorCalculosServiceLocator.GetService<GestorDatosBase>(
                                    configuracion.TipoProveedor.ToString()).ExecuteNonQuery(Tenant, (RepositorioTO)fuente, configuracion, null, Identificador);
                                break;
                            case TipoFuente.Servicio:
                                break;
                            case TipoFuente.Archivo:
                                break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(IdGrupo)) //PROCESAMIENTO DE TODO UN GRUPO ESPECÍFICO
                {
                    EjecutarCalculo(IdCalculo, IdGrupo, configuracion);
                }
                else if (!string.IsNullOrEmpty(IdCalculo)) //PROCESAMIENTO DE UN CÁLCULO ESPECÍFICO
                {
                    EjecutarCalculo(IdCalculo, configuracion);
                }
                else //PROCESAMIENTO NORMAL DE TODO EL ARCHIVO
                {
                    List<GrupoTO> grupos = configuracion.Grupos;

                    if (grupos != null && grupos.Count > 0)
                    {
                        //Se ordenan los grupos
                        grupos = grupos.OrderBy(g => g.Orden).ToList();

                        foreach (var grupo in grupos)
                        {
                            //Se obtienen los cálculos de cada grupo
                            IEnumerable<CalculoTO> calculosConGrupo = configuracion.Calculos.Where(c => c.IdGrupo == grupo.ID);

                            if (calculosConGrupo != null)
                            {
                                ProcesarCalculos(calculosConGrupo, configuracion, grupo);
                            }
                        }
                    }

                    //Se obtienen los cálculos sin grupos y se ejecutan
                    IEnumerable<CalculoTO> calculosSinGrupo = configuracion.Calculos.Where(c => string.IsNullOrEmpty(c.IdGrupo));

                    if (calculosSinGrupo != null)
                    {
                        ProcesarCalculos(calculosSinGrupo, configuracion);
                    }
                }

                _procesoCompletado = true;
            }
            catch (Exception ex)
            {
                // doesn't publish asynchronously because it has already ended the work.
                var mensaje = new { IdProcesoGestor = Identificador, Completado = true, HayError = true, Error = ex };
                ServiceBus?.Publish(Tenant, mensaje);

                _procesoCompletado = false;
                bitacoraMensajes.InsertarMensaje(Tenant,"@GestorCalculosError_ExcepcionProceso", ex.Message);

                if (HabilitarRegistroDetalleTecnico && ex.InnerException != null)
                    bitacoraMensajes.InsertarMensaje(Tenant, "@GestorCalculosError_DetalleTecnicoError", ex.InnerException.Message);

                 throw;
            }
            finally
            {
                string timeZoneKey = GestorCalculosHelper.GetMetadataValue(Tenant, _timeZoneSetting, true);
                DateTime? messageDate = string.IsNullOrEmpty(timeZoneKey) ? DateTime.Now : bitacoraMensajes.GetTimeZoneDate(timeZoneKey, DateTime.Now);
                string mensajeFinal = BitacoraMensajesHelper.ObtenerMensajeRecursos("GestorCalculosInfo_Separador");
                mensajeFinal += BitacoraMensajesHelper.ObtenerMensajeRecursos("GestorCalculosInfo_FinProceso", NOMBRE_PROCESO);
                mensajeFinal += (_procesoCompletado) ? MENSAJE_EXITO : MENSAJE_ERROR;
                //mensajeFinal += "  " + messageDate.ToString();
                bitacoraMensajes.FinalizarProceso(Tenant, _procesoCompletado, mensajeFinal);
            }
        }
        #endregion

        #region Métodos Privados
        /// <summary>
        /// Método que permite procesar un conjunto de cálculos
        /// </summary>
        /// <param name="calculos">cálculos a procesar</param>
        /// <param name="configuracion">Archivo de configuración a utilizar en el procesamiento del calcúlo</param>
        /// <param name="grupo"></param>
        private void ProcesarCalculos(IEnumerable<CalculoTO> calculos, ConfiguracionTO configuracion, GrupoTO grupo = null)
        {
            if (calculos != null)
            {
                //se ordenan los cálculos en el grupo y se ejecutan
                List<CalculoTO> calculosOrdenados = calculos.OrderBy(c => c.Orden).ToList();

                if (configuracion.EjecutarCalculosEnParalelo) //se ejecutan en paralelo
                {
                    var options = new ParallelOptions();
                    string configNumeroProcesosParalelo = GestorCalculosHelper.GetMetadataValue(Tenant, "NumeroProcesosEnParalelo", false);
                    options.MaxDegreeOfParallelism = (!string.IsNullOrEmpty(configNumeroProcesosParalelo)) ? int.Parse(configNumeroProcesosParalelo) : 2;

                    var actual = 1;
                    Parallel.ForEach(calculosOrdenados, options, (calculo, state) =>
                        {
                            BitacoraMensajesHelper.UpdateCurrentCulture(Tenant);

                            if (Worker.CancellationPending)
                            {
                                state.Stop();
                            }

                            ValidarDependenciaCalculo(calculo, configuracion);

                            lock (calculosOrdenados)
                            {
                                PublicarProgresoGeneral(actual, calculosOrdenados.Count, calculo, grupo);
                                Interlocked.Increment(ref actual);
                            }

                            EjecutarCalculo(calculo, configuracion);

                            lock (calculosOrdenados)
                            {
                                PublicarProgresoDetalle(0, 100, calculo);
                            }
                        });
                }
                else //Se ejecutan en forma secuencial
                {
                    var actual = 1;
                    foreach (CalculoTO calculo in calculosOrdenados)
                    {
                        if (Worker.CancellationPending)
                        {
                            break;
                        }

                        ValidarDependenciaCalculo(calculo, configuracion);
                        lock (calculosOrdenados)
                        {
                            PublicarProgresoGeneral(actual, calculosOrdenados.Count, calculo, grupo);
                        }

                        EjecutarCalculo(calculo, configuracion);

                        lock (calculosOrdenados)
                        {
                            PublicarProgresoDetalle(0, 100, calculo);
                        }

                        actual++;
                    }
                }
            }
        }

        /// <summary>
        /// Permite validar la dependencia de un cálculo
        /// </summary>
        /// <param name="calculo">Cálculo a validar</param>
        /// <param name="configuracion">Archivo de configuración a utilizar en el procesamiento del calcúlo</param>
        private void ValidarDependenciaCalculo(CalculoTO calculo, ConfiguracionTO configuracion)
        {
            string idDependencia = calculo.IdDependencia;
            string idDependenciaGrupo = calculo.IdDependenciaGrupo;

            if (!string.IsNullOrEmpty(idDependencia))
            {
                CalculoTO calculoDependiente = configuracion.Calculos.Where(c => c.ID == idDependencia).FirstOrDefault();

                if (calculoDependiente == null)
                {
                    throw new GestorCalculosException("GestorCalculosError_DependenciaCalculo", calculo.ID, idDependencia);
                }

                if (!calculoDependiente.Exitoso)
                {
                    throw new GestorCalculosException("GestorCalculosError_DependenciaCalculoNoExitosa", calculo.ID, idDependencia);
                }
            }

            //Verificación de la dependencia del grupo
            if (!string.IsNullOrEmpty(idDependenciaGrupo))
            {
                //Se verifica que exista el grupo
                GrupoTO grupo = configuracion.Grupos.Where(g => g.ID == idDependenciaGrupo).FirstOrDefault();

                if (grupo == null)
                {
                    throw new GestorCalculosException("GestorCalculosError_DependenciaGropoNoExiste", calculo.ID, idDependenciaGrupo);
                }

                //Se obtienen los cálculos del grupo
                IEnumerable<CalculoTO> calculosEnGrupo = configuracion.Calculos.Where(c => c.IdGrupo == idDependenciaGrupo);

                if (calculosEnGrupo != null)
                {
                    //Se verifican que todos esten exitosos
                    foreach (var calculoGrupo in calculosEnGrupo)
                    {
                        if (!calculoGrupo.Exitoso)
                        {
                            throw new GestorCalculosException("GestorCalculosError_DependenciaGrupoNoExitosa", calculo.ID, idDependenciaGrupo, calculoGrupo.ID);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// /// Permite ejecutar un proceso de cálculo por el identificador del grupo
        /// </summary>
        /// <param name="idCalculo">Identificador del cálculo a ejecutar</param>
        /// <param name="idGrupo">Identificador del grupo a ejecutar</param>
        /// <param name="configuracion">configuración a utilizar en el proceso de cálculo</param>
        private void EjecutarCalculo(string idCalculo, string idGrupo, ConfiguracionTO configuracion)
        {
            List<GrupoTO> grupos = configuracion.Grupos;

            if (grupos != null && grupos.Count > 0)
            {
                GrupoTO grupoAEjecutar = grupos.Where(g => g.ID == idGrupo).FirstOrDefault();

                if (grupoAEjecutar == null)
                    throw new GestorCalculosException("GestorCalculosError_GrupoAEjecutar", idGrupo);

                //Se obtienen los cálculos del grupo a ejecutar
                IEnumerable<CalculoTO> calculosConGrupo = configuracion.Calculos.Where(c => c.IdGrupo == grupoAEjecutar.ID);

                if (!string.IsNullOrEmpty(idCalculo))
                {
                    CalculoTO calculoAEjecutar = calculosConGrupo.Where(c => c.ID == idCalculo).FirstOrDefault();

                    if (calculoAEjecutar == null)
                        throw new GestorCalculosException("GestorCalculosError_CalculoNoPerteneceGrupo", idCalculo, idGrupo);

                    EjecutarCalculo(calculoAEjecutar, configuracion);
                }
                else
                {
                    ProcesarCalculos(calculosConGrupo, configuracion);
                }
            }
            else
            {
                throw new GestorCalculosException("GestorCalculosError_GrupoAEjecutar", idGrupo);
            }
        }

        /// <summary>
        /// Permite ejecutar un proceso de cálculo por el identificador
        /// </summary>
        /// <param name="idCalculo">Identificador del cálculo a ejecutar</param>
        /// <param name="configuracion">configuración a utilizar en el proceso de cálculo</param>
        private void EjecutarCalculo(string idCalculo, ConfiguracionTO configuracion)
        {
            CalculoTO calculoAEjecutar = configuracion.Calculos.Where(c => c.ID == idCalculo).FirstOrDefault();

            if (calculoAEjecutar == null)
                throw new GestorCalculosException("GestorCalculosError_CalculoAEjecutar", IdCalculo);

            EjecutarCalculo(calculoAEjecutar, configuracion);
        }

        /// <summary>
        /// Permite ejecutar un proceso de cálculo
        /// </summary>
        /// <param name="calculo">cálculo a ejecutar</param>
        /// <param name="configuracion">configuración a utilizar en el proceso de cálculo</param>
        private void EjecutarCalculo(CalculoTO calculo, ConfiguracionTO configuracion)
        {
            if (Worker.CancellationPending)
            {
                return;
            }

            //Se crea el buffer del cálculo actual
            var buffer = new List<object>();
            //Almacena temporalmente los resultados de las variables del cálculo
            var diccionarioResultadoVariables = new Dictionary<string, decimal>();
            VariableTO variableSemilla = null;
            VariableTO variableAlmacenar = null;
            List<VariableTO> variablesExpresion = null;

            try
            {
                BitacoraMensajesHelper.UpdateCurrentCulture(Tenant);
                bitacoraMensajes.InsertarMensaje(Tenant, "@GestorCalculosInfo_ProcesandoCalculo", calculo.ID);
                string formula = calculo.Formula;
                List<VariableTO> variablesCalculo = calculo.Variables;

                if (variablesCalculo != null && variablesCalculo.Count > 0)
                {
                    //Se consulta si se tiene una variable semilla
                    variableSemilla = variablesCalculo.Where(v => v.TipoVariable == TipoVariable.Semilla).FirstOrDefault();
                    //Se consulta si se tiene una variable de almacenamiento que no posea formula interna
                    variableAlmacenar = variablesCalculo.Where(v => v.Almacenar && string.IsNullOrEmpty(v.FormulaInterna)).FirstOrDefault();
                    //Se filtran las variables que se encuentran en la expresión
                    variablesExpresion = variablesCalculo.Where(v => (v.TipoVariable != TipoVariable.Semilla && v.Direccion != DireccionValor.Salida && !v.Almacenar) ||
                        (v.Almacenar && !string.IsNullOrEmpty(v.FormulaInterna)) || v.TipoVariable == TipoVariable.SemillaInterna).ToList();//Se hace este filtro debido a que algunas variables con semilla y formula interna, deben ser almacenadas



                    //Si encuentra variable semilla se valida y se itera de acuerdo a la lista que devuelva la variable
                    if (variableSemilla != null)
                    {
                        //Se valida la variable semilla
                        if (variableSemilla.TipoDato != TipoDato.DictionaryString)
                            throw new GestorCalculosException("GestorCalculosError_VariableSemillaInvalida", variableSemilla.ID);

                        List<SemillaTO> diccionarioSemillas = ObtenerFuenteSemilla(variableSemilla, configuracion, buffer);

                        if (diccionarioSemillas != null)
                        {

                            bitacoraMensajes.InsertarMensaje(Tenant, "Semillas: " + diccionarioSemillas.Count);

                            // Crea Tablas Temporales (DataTables) para los repositorios de las variables de salida (resultado) que se deben almacenar por BulkCopy
                            if (!calculo.BulkCopy.Equals(TipoBulkCopy.Ninguno))
                            {
                                foreach (var variableAlmacenarBulkCopy in calculo.Variables.Where(v => v.Almacenar))
                                {
                                    CrearDataTable((RepositorioTO)ObtenerFuente(variableAlmacenarBulkCopy.IdFuente));
                                }
                            }

                            // Obtiene Numero de Procesos en Paralelo según configuración
                            string configNumeroProcesosParalelo = GestorCalculosHelper.GetMetadataValue(Tenant, "NumeroProcesosEnParalelo", false);

                            // Verifica si alguna variable tiene un repositorio tipo "DataTable" (Memoria)  // 
                            // y pobla las tablas en memoria
                            foreach (var v in variablesExpresion)
                            {
                                // Si la variable No Tiene Fuente asociada
                                if (string.IsNullOrEmpty(v.IdFuente))
                                {
                                    continue;
                                }

                                FuenteTO fuente = ObtenerFuente(v.IdFuente);

                                // Si la fuente no es tipo Repositorio 
                                if (!fuente.Tipo.Equals(TipoFuente.Repositorio))
                                {
                                    continue;
                                }

                                string idFuenteMemoria = ((RepositorioTO)fuente).IdFuenteMemoria;

                                // Si la fuente (Repositorio) Tiene Fuente en Memoria
                                if (string.IsNullOrEmpty(idFuenteMemoria))
                                {
                                    continue;
                                }

                                // Verfica que ya no este la tabla cargada en memoria por dos o mas variables tiene la misma fuente
                                if (!repositoriosEnMemoriaDictionary.ContainsKey(idFuenteMemoria))
                                {
                                    FuenteTO fuenteMemoria = ObtenerFuente(idFuenteMemoria);

                                    DataTable tablaMemoria = GestorCalculosServiceLocator.GetService<GestorDatosBase>(configuracion.TipoProveedor.ToString()).GetData(
                                        Tenant, (RepositorioTO)fuenteMemoria, configuracion, buffer, Identificador);

                                    //Valida que exitan datos de entrada! 
                                    if (tablaMemoria == null || tablaMemoria.Rows.Count == 0)
                                        throw new GestorCalculosException("GestorCalculosError_RepositorioSinDatos", idFuenteMemoria);

                                    // Ordena las Columnas del DataTable: Importante! ya que se concatenan todos los campo para identificar el registro
                                    // en el diccionario y luego se consulta con este mismo orden. 
                                    // Este metodo de guardado en memoria mejora considerablemnte el performace de cálculo
                                    // Se excluye ultima columna (Valor)
                                    var columnValue = tablaMemoria.Columns[tablaMemoria.Columns.Count - 1].ColumnName;
                                    var columnNameOrder = tablaMemoria.Columns.Cast<DataColumn>().Where(w => !w.ColumnName.Equals(columnValue)).OrderBy(o => o.ColumnName).Select(s => s.ColumnName);
                                    int columnIndex = 0;
                                    foreach (var column in columnNameOrder)
                                    {
                                        tablaMemoria.Columns[column].SetOrdinal(columnIndex); columnIndex++;
                                    }

                                    // Guarda data en Diccionario
                                    var memoryTableDic = new Dictionary<string, decimal>();
                                    foreach (var item in tablaMemoria.Select().Select(dt => dt.ItemArray).ToArray())
                                    {
                                        string key = string.Empty;
                                        for (int i = 0; i < item.Length - 1; i++)
                                        {
                                            key += item[i]?.ToString() + "-";
                                        }

                                        memoryTableDic.Add(key, Convert.ToDecimal(item[item.Length - 1].Equals(DBNull.Value) ? 0 : item[item.Length - 1]));
                                    }

                                    repositoriosEnMemoriaDictionary.Add(idFuenteMemoria, memoryTableDic);

                                    bitacoraMensajes.InsertarMensaje(Tenant, "Registros en Memoria:(" + idFuenteMemoria + "): " + memoryTableDic.Count.ToString());

                                }
                            }

                            if (variableSemilla.EjecutarSemillaEnParalelo)
                            {
                                var options = new ParallelOptions();
                                options.MaxDegreeOfParallelism = (!string.IsNullOrEmpty(configNumeroProcesosParalelo)) ? int.Parse(configNumeroProcesosParalelo) : 2;

                                var actual = 1;
                                //Se efectua el proceso de cálculo por cada elemento de la semilla en paralelo
                                Parallel.ForEach(diccionarioSemillas, options, (semilla, state) =>
                                {

                                    BitacoraMensajesHelper.UpdateCurrentCulture(Tenant);

                                    if (Worker.CancellationPending)
                                    {
                                        state.Stop();
                                    }

                                    var bufferHilo = new List<object>();
                                    var diccionarioResultadoVariablesHilo = new Dictionary<string, decimal>();

                                    try
                                    {
                                        //Se adiciona el key y el value de la semilla al buffer para su evaluación
                                        bufferHilo.Add(semilla.ValorPrincipal);//EJEMPLO: IDELEMENTO
                                        bufferHilo.Add(semilla.ValorSecundario);//EJEMPLO: IDCONTRATO
                                        //Se evalua el cálculo para la iteración actual
                                        EvaluarExpresionCalculo(calculo, configuracion, variablesExpresion, diccionarioResultadoVariablesHilo,
                                            variableAlmacenar, variableSemilla.EjecutarPorIteracion, bufferHilo, variableSemilla.HabilitarReporte);

                                        lock (diccionarioSemillas)
                                        {
                                            PublicarProgresoDetalle(actual, diccionarioSemillas.Count, calculo);
                                            Interlocked.Increment(ref actual);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _procesoCompletado = false;

                                        // Inserta a mensaje Notificando que hubo un error en la semilla
                                        PublicarErrorSemilla(calculo.Nombre);

                                        if (InterrumpirCalculoPorExcepcion)
                                            throw ex;

                                        ProcesarExcepcionSemilla(Identificador, calculo.ID, semilla, ex);
                                    }
                                    finally
                                    {
                                        //Se liberan los recursos de la ejecución del hilo
                                        bufferHilo.Clear();
                                        bufferHilo = null;
                                        diccionarioResultadoVariablesHilo.Clear();
                                        diccionarioResultadoVariablesHilo = null;

                                        if (calculo.BulkCopy.Equals(TipoBulkCopy.Semilla))
                                        {
                                            AlmacenarCalculoBulkCopy(calculo, configuracion);
                                        }
                                    }

                                });
                            }
                            else
                            {
                                var actual = 1;
                                //Se efectua el proceso de cálculo por cada elemento de la semilla de manera secuencial
                                foreach (var semilla in diccionarioSemillas)
                                {
                                    try
                                    {
                                        if (Worker.CancellationPending)
                                        {
                                            break;
                                        }

                                        //Se adiciona el key y el value de la semilla al buffer para su evaluación
                                        buffer.Add(semilla.ValorPrincipal);//EJEMPLO: IDELEMENTO
                                        buffer.Add(semilla.ValorSecundario);//EJEMPLO: IDCONTRATO
                                        //Se evalua el cálculo para la iteración actual
                                        EvaluarExpresionCalculo(calculo, configuracion, variablesExpresion, diccionarioResultadoVariables,
                                            variableAlmacenar, variableSemilla.EjecutarPorIteracion, buffer, variableSemilla.HabilitarReporte);

                                        lock (diccionarioSemillas)
                                        {
                                            PublicarProgresoDetalle(actual, diccionarioSemillas.Count, calculo);
                                            actual++;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _procesoCompletado = false;

                                        // Inserta a mensaje Notificando que hubo un error en la semilla
                                        PublicarErrorSemilla(calculo.Nombre);

                                        if (InterrumpirCalculoPorExcepcion)
                                            throw ex;

                                        ProcesarExcepcionSemilla(Identificador, calculo.ID, semilla, ex);
                                    }
                                    finally
                                    {
                                        buffer.Clear();
                                    }
                                }
                            }

                            if (calculo.BulkCopy.Equals(TipoBulkCopy.Calculo))
                            {
                                AlmacenarCalculoBulkCopy(calculo, configuracion);
                            }
                        }
                    }
                }

                //Se ejecuta el cálculo final
                //REMARKS: En algunas ocasiones, cuando se ejecuta el cálculo por la iteración de la semilla, no es necesario ejecutar el cálculo final
                if (calculo.Ejecutar && Worker.CancellationPending == false)
                {
                    /* Si existe variable semilla, y no se hizo la ejecución del cálculo en cada iteración, no es necesario volver a evaluar
                     * las variables, debido a que estas ya se encuentran almacenadas en el diccionario de variables.
                     */
                    if (variableSemilla != null && !variableSemilla.EjecutarPorIteracion)
                        variablesExpresion = null;

                    EvaluarExpresionCalculo(calculo, configuracion, variablesExpresion, diccionarioResultadoVariables,
                        variableAlmacenar, true, buffer);
                }

                calculo.Exitoso = true;
            }
            catch (Exception ex)
            {
                _procesoCompletado = false;

                // Inserta a mensaje Notificando que hubo un error en la semilla
                PublicarErrorSemilla(calculo.Nombre);

                if (InterrumpirCalculoPorExcepcion)
                    throw ex;

                calculo.Exitoso = false;
                bitacoraMensajes.InsertarMensaje(Tenant, "@GestorCalculosError_Calculo", calculo.ID, ex.Message);

                if (HabilitarRegistroDetalleTecnico && ex.InnerException != null)
                    bitacoraMensajes.InsertarMensaje(Tenant, "@GestorCalculosError_DetalleTecnicoError", ex.InnerException.Message);

            }
            finally
            {
                diccionarioResultadoVariables.Clear();
            }
        }

        /// <summary>
        /// Almacena Tabla en memoria y la limpia
        /// </summary>
        /// <param name="calculo"></param>
        /// <param name="configuracion"></param>
        private void AlmacenarCalculoBulkCopy(CalculoTO calculo, ConfiguracionTO configuracion)
        {
            // Se isertan registros de la tabla 
            lock (dataTableDictionary)
            {
                foreach (var varAlmacenar in calculo.Variables.Where(v => v.Almacenar))
                {
                    var repositorio = (RepositorioTO)ObtenerFuente(varAlmacenar.IdFuente);

                    GestorCalculosServiceLocator.GetService<GestorDatosBase>(configuracion.TipoProveedor.ToString()).
                            ExecuteNonQueryBulkCopy(Tenant, repositorio, dataTableDictionary[repositorio.ID]);

                    // Elimina registros de la tabla
                    dataTableDictionary[repositorio.ID].Clear();
                }
            }
        }

        /// <summary>
        /// Permite obtener la fuente por el id
        /// </summary>
        /// <param name="idFuente">Identificador de la fuente</param>
        /// <returns>Objeto fuente</returns>
        private FuenteTO ObtenerFuente(string idFuente)
        {
            if (string.IsNullOrEmpty(idFuente))
                throw new ArgumentNullException("idFuente");

            return Fuentes.Where(f => f.ID == idFuente).FirstOrDefault();
        }

        /// <summary>
        /// Permite obtener la fuente de la semilla
        /// </summary>
        /// <param name="variableSemilla">Variable semilla a obtener</param>
        /// <param name="configuracion">Configuración del cálculo a utilizar</param>
        /// <param name="buffer">Buffer que se utiliza para el filtro de valores</param>
        /// <returns>El diccionario de datos con la semilla</returns>
        private List<SemillaTO> ObtenerFuenteSemilla(VariableTO variableSemilla, ConfiguracionTO configuracion, List<object> buffer)
        {
            List<SemillaTO> listaSemillas = null;

            if (variableSemilla.TipoFuncion == TipoFuncion.DiasMes || variableSemilla.TipoFuncion == TipoFuncion.DiasMesPeriodos)
            {
                DateTime fechaAEvaluar = DateTime.Now;

                if (!string.IsNullOrEmpty(variableSemilla.Constante))
                {
                    //Se obtiene el valor constante y se hace una conversión al tipo DateTime
                    object valorConstante = GestorCalculosHelper.ObtenerValorConstante(configuracion.Nombre, variableSemilla.Constante, configuracion.Parametros, buffer, Identificador);

                    if (valorConstante != null)
                    {
                        DateTime.TryParse(valorConstante.ToString(), out fechaAEvaluar);
                    }
                }

                listaSemillas = (variableSemilla.TipoFuncion == TipoFuncion.DiasMes) ? fechaAEvaluar.ObtenerDiasMes(false) : fechaAEvaluar.ObtenerDiasMes(true);
            }
            else
            {
                FuenteTO fuente = ObtenerFuente(variableSemilla.IdFuente);

                if (fuente == null)
                    throw new GestorCalculosException("GestorCalculosError_VariableSemillaFuente", variableSemilla.ID);

                try
                {
                    switch (fuente.Tipo)
                    {
                        case TipoFuente.Ninguno:
                            break;
                        case TipoFuente.Repositorio:
                            DataTable tabla = GestorCalculosServiceLocator.GetService<GestorDatosBase>(
                                configuracion.TipoProveedor.ToString()).GetData(Tenant, (RepositorioTO)fuente, configuracion, buffer, Identificador);

                            if (tabla == null)
                                throw new GestorCalculosException("GestorCalculosError_VariableSemillaFuente", variableSemilla.ID);

                            listaSemillas = tabla.ToListSemilla();
                            break;
                        case TipoFuente.Servicio:
                            break;
                        case TipoFuente.Archivo:
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    throw new GestorCalculosException("GestorCalculosError_ExcepcionEnFuente", ex, fuente.ID);
                }
            }

            return listaSemillas;
        }

        /// <summary>
        /// Permite procesar la excepción de la semilla de manera centralizada
        /// </summary>
        /// <param name="identificador">Identificador del proceso</param>
        /// <param name="calculoID">Identificarod del cálculo</param>
        /// <param name="semilla">Semilla a validar</param>
        /// <param name="ex">Excepción generada</param>
        private void ProcesarExcepcionSemilla(string identificador, string calculoID, SemillaTO semilla, Exception ex)
        {
            string mensajeCalculo = string.Format(FORMATO_RESULTADO_SEMILLA, calculoID, semilla);
            bitacoraMensajes.InsertarMensaje(Tenant, "@GestorCalculosError_Calculo", mensajeCalculo, ex.Message);

            if (HabilitarRegistroDetalleTecnico && ex.InnerException != null)
                bitacoraMensajes.InsertarMensaje(Tenant, "@GestorCalculosError_DetalleTecnicoError", ex.InnerException.Message);

        }

        /// <summary>
        /// Permite evaluar la expresión actual del cálculo y ejecutarla si se requiere
        /// </summary>
        /// <param name="calculo">Elemento de configuración con la información del cálculo</param>
        /// <param name="variables">Conjunto de variables del cálculo que se evaluan</param>
        /// <param name="buffer">Buffer que permite el envío de información</param>
        /// <param name="configuracion">Configuración del cálculo</param>
        /// <param name="diccionarioVariables">Diccionario de variables que se mantienen en el cálculo</param>
        /// <param name="ejecutarCalculo">Indica si se debe ejecutar el cálculo o solo se procesan las semillas</param>
        /// <param name="variableAlmacenar">Indica la variable que se utiliza para almacenar</param>
        /// <returns>Retorna el resultado de la evaluación del cálculo</returns>
        private decimal EvaluarExpresionCalculo(CalculoTO calculo, ConfiguracionTO configuracion, List<VariableTO> variables,
            Dictionary<string, decimal> diccionarioVariables, VariableTO variableAlmacenar, bool ejecutarCalculo, List<object> buffer)
        {
            return EvaluarExpresionCalculo(calculo, configuracion, variables, diccionarioVariables, variableAlmacenar, ejecutarCalculo, buffer, true);
        }

        /// <summary>
        /// Permite evaluar la expresión actual del cálculo y ejecutarla si se requiere
        /// </summary>
        /// <param name="calculo">Elemento de configuración con la información del cálculo</param>
        /// <param name="variables">Conjunto de variables del cálculo que se evaluan</param>
        /// <param name="buffer">Buffer que permite el envío de información</param>
        /// <param name="configuracion">Configuración del cálculo</param>
        /// <param name="diccionarioVariables">Diccionario de variables que se mantienen en el cálculo</param>
        /// <param name="ejecutarCalculo">Indica si se debe ejecutar el cálculo o solo se procesan las semillas</param>
        /// <param name="variableAlmacenar">Indica la variable que se utiliza para almacenar</param>
        /// <param name="habilitarReporte">Indica si se debe registrar en el log el reporte de la ejecución de un cálculo</param>
        /// <returns>Retorna el resultado de la evaluación del cálculo</returns>
        private decimal EvaluarExpresionCalculo(CalculoTO calculo, ConfiguracionTO configuracion, List<VariableTO> variables,
            Dictionary<string, decimal> diccionarioVariables, VariableTO variableAlmacenar, bool ejecutarCalculo, List<object> buffer, bool habilitarReporte)
        {
            if (Worker.CancellationPending)
            {
                return 0;
            }

            if (calculo == null)
                throw new ArgumentNullException("calculo");

            decimal resultadoFinal = 0;

            if (variables != null && variables.Count > 0)
            {
                foreach (var variable in variables)
                {
                    if (Worker.CancellationPending)
                        break;

                    if (variable.TipoVariable == TipoVariable.Global && diccionarioVariables.ContainsKey(variable.ID))
                        continue; //La variable ya fue calculada, y no es necesario volverla a calcular

                    if (variable.TipoVariable == TipoVariable.SemillaInterna) //No se procesa independiente las semillas internas
                        continue;

                    if (!calculo.ID.StartsWith(CONSTANTE_SUBEXPRESION) && variable.TipoVariable == TipoVariable.Interna)
                        continue; //No procesa las variables que se utilizan en las subexpresiones

                    object resultadoVariable = null;

                    if (!string.IsNullOrEmpty(variable.FormulaInterna)) //Primero se verifica que la variable tenga una formula interna
                    {
                        //Se obtiene en caso de que exista una semilla interna para la subexpresión
                        VariableTO variableSemillaInterna = variables.Where(v => v.TipoVariable == TipoVariable.SemillaInterna).FirstOrDefault();
                        IEnumerable<VariableTO> variablesInternas = variables.Where(v => v.TipoVariable == TipoVariable.Interna || v.TipoVariable == TipoVariable.Global);
                        //Si se debe almacenar el cálculo de la subexpresión, se envía la misma variable con la fuente
                        VariableTO variableAlmacenamiento = (variable.Almacenar) ? variable : null;

                        if (variablesInternas != null && variablesInternas.Count() > 0)
                        {
                            try
                            {
                                var variablesSubexpresion = new List<VariableTO>();

                                foreach (var variableInterna in variablesInternas)
                                {
                                    if (variable.FormulaInterna.Contains(variableInterna.ID))
                                        variablesSubexpresion.Add(variableInterna);
                                }

                                //Se ejecuta recursivamente la evaluación de la expresión
                                var calculoSubExpresion = new CalculoTO()
                                {
                                    ID = CONSTANTE_SUBEXPRESION + variable.ID,
                                    Formula = variable.FormulaInterna
                                };

                                List<SemillaTO> diccionarioSemillasInternas = null;

                                if (variableSemillaInterna != null)
                                {
                                    //Se valida la variable semilla
                                    if (variableSemillaInterna.TipoDato != TipoDato.DictionaryString)
                                        throw new GestorCalculosException("GestorCalculosError_VariableSemillaInvalida", variableSemillaInterna.ID);

                                    diccionarioSemillasInternas = ObtenerFuenteSemilla(variableSemillaInterna, configuracion, buffer);
                                }

                                if (diccionarioSemillasInternas != null)
                                {
                                    //Se efectua el proceso de cálculo por cada elemento de la semilla
                                    foreach (var semilla in diccionarioSemillasInternas)
                                    {
                                        if (Worker.CancellationPending)
                                        {
                                            break;
                                        }

                                        try
                                        {
                                            //Se adiciona el key y el value de la semilla interna al buffer para su evaluación
                                            buffer.Add(semilla.ValorPrincipal);//EJEMPLO: IDAGENTE
                                            buffer.Add(semilla.ValorSecundario);//EJEMPLO: SALDO ACREEDOR
                                            //Se evalua el cálculo para la iteración actual
                                            EvaluarExpresionCalculo(calculoSubExpresion, configuracion, variablesSubexpresion, diccionarioVariables,
                                                variableAlmacenamiento, variableSemillaInterna.EjecutarPorIteracion, buffer, variableSemillaInterna.HabilitarReporte);
                                        }
                                        catch (Exception ex)
                                        {
                                            _procesoCompletado = false;

                                            // Inserta a mensaje Notificando que hubo un error en la semilla
                                            PublicarErrorSemilla(calculo.Nombre);

                                            if (InterrumpirCalculoPorExcepcion)
                                                throw ex;

                                            string mensajeCalculo = string.Format(FORMATO_RESULTADO_SEMILLA, calculo.ID, semilla);
                                            bitacoraMensajes.InsertarMensaje(Tenant, "@GestorCalculosError_Calculo", mensajeCalculo, ex.Message);

                                            if (HabilitarRegistroDetalleTecnico && ex.InnerException != null)
                                                bitacoraMensajes.InsertarMensaje(Tenant, "@GestorCalculosError_DetalleTecnicoError", ex.InnerException.Message);

                                        }
                                        finally
                                        {
                                            //Se limpia la información del buffer pero solo de la semilla interna
                                            int cantidadActualBuffer = buffer.Count;

                                            if (cantidadActualBuffer > 2)
                                                buffer.RemoveRange(2, cantidadActualBuffer - 2);
                                        }
                                    }
                                }
                                else
                                {
                                    resultadoVariable = EvaluarExpresionCalculo(calculoSubExpresion, configuracion, variablesSubexpresion, diccionarioVariables, variableAlmacenamiento, true, buffer);
                                }
                            }
                            catch (Exception ex)
                            {
                                _procesoCompletado = false;

                                // Inserta a mensaje Notificando que hubo un error en la semilla
                                PublicarErrorSemilla(calculo.Nombre);

                                if (InterrumpirCalculoPorExcepcion)
                                    throw ex;

                                bitacoraMensajes.InsertarMensaje(Tenant, "@GestorCalculosError_Calculo", variable.ID, ex.Message);

                                if (HabilitarRegistroDetalleTecnico && ex.InnerException != null)
                                    bitacoraMensajes.InsertarMensaje(Tenant, "@GestorCalculosError_DetalleTecnicoError", ex.InnerException.Message);

                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(variable.IdFuente)) //Se verifica la fuente de información
                    {
                        //Se obtiene la fuente de la variable
                        FuenteTO fuente = ObtenerFuente(variable.IdFuente);

                        if (fuente == null)
                            throw new GestorCalculosException("GestorCalculosError_VariableFuente", variable.ID);

                        try
                        {
                            switch (fuente.Tipo)
                            {
                                case TipoFuente.Ninguno:
                                    resultadoVariable = 0;
                                    break;
                                case TipoFuente.Repositorio:

                                    // Evalua si la consulta se realiza en memoria
                                    string idFuenteMemoria = ((RepositorioTO)fuente).IdFuenteMemoria;
                                    if (!string.IsNullOrEmpty(idFuenteMemoria))
                                    {
                                        var repository = (RepositorioTO)fuente;

                                        string keySearch = string.Empty;

                                        if (repository.Parametros != null && repository.Parametros.Count > 0)
                                        {
                                            foreach (var parametro in repository.Parametros.OrderBy(o => o.Nombre))
                                            {
                                                object valor = parametro.Valor.ObtenerValorFormateado(configuracion.Nombre,
                                                        configuracion.Cultura, parametro, configuracion.Parametros, buffer, Identificador);

                                                keySearch += valor?.ToString() + "-";
                                            }
                                        }

                                        decimal valueResul;
                                        repositoriosEnMemoriaDictionary[idFuenteMemoria].TryGetValue(keySearch, out valueResul);
                                        resultadoVariable = valueResul;
                                    }
                                    else
                                    {
                                        if (variable.TipoDato == TipoDato.Decimal)
                                        {
                                            resultadoVariable = GestorCalculosServiceLocator.GetService<GestorDatosBase>(
                                                configuracion.TipoProveedor.ToString()).ExecuteScalar(Tenant, (RepositorioTO)fuente, configuracion, buffer, Identificador);
                                        }
                                    }
                                    break;
                                case TipoFuente.Servicio:
                                    break;
                                case TipoFuente.Archivo:
                                    break;
                                default:
                                    resultadoVariable = 0;
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new GestorCalculosException("GestorCalculosError_ExcepcionEnFuente", ex, fuente.ID);
                        }
                    }
                    else if (!string.IsNullOrEmpty(variable.Constante))//Sino se verifica la constante
                    {
                        resultadoVariable = GestorCalculosHelper.ObtenerValorConstante(NombreConfiguracion, variable.Constante, Parametros, buffer, Identificador);
                    }
                    else if (!string.IsNullOrEmpty(variable.FuncionExterna)) // Funcion Externa!
                    {
                        resultadoVariable = new ExternalFunctionFactory().Execute(Tenant, Parametros, buffer);
                    }

                    //Se evalua el resultado de la variable y si tiene un valor por defecto en caso de nulo
                    if (resultadoVariable == null || resultadoVariable == DBNull.Value)
                    {
                        if (!variable.PermitirNulos)
                        {
                            if (string.IsNullOrEmpty(variable.ValorDefecto))
                                throw new GestorCalculosException("GestorCalculosError_VariableNula", variable.ID);
                            else
                                resultadoVariable = variable.ValorDefecto;
                        }
                        else
                        {
                            //Se toma como valor por defecto al permitir nulos el 0
                            resultadoVariable = 0;
                        }
                    }

                    //Se valida la variable actual
                    if (variable.TipoDato != TipoDato.Decimal)
                        throw new GestorCalculosException("GestorCalculosError_VariableInvalida", variable.ID);

                    decimal resultadoVariableIteracion = resultadoVariable.ToString().ToDecimalFormateado(configuracion.Cultura, variable.DigitosFlotantes, variable.TipoRedondeo);

                    if (diccionarioVariables.ContainsKey(variable.ID) && variable.TipoFuncion == TipoFuncion.Sumar)
                        diccionarioVariables[variable.ID] += resultadoVariableIteracion;
                    else
                        diccionarioVariables[variable.ID] = resultadoVariableIteracion;

                    //Se evalua si se debe imprimir el resultado de la variable
                    if (HabilitadoImpresionVariables)
                        bitacoraMensajes.InsertarMensaje(Tenant, "@GestorCalculosInfo_ResultadoVariable", variable.ID, resultadoVariableIteracion.ToString());
                }
            }

            if (ejecutarCalculo && Worker.CancellationPending == false)
            {
                string formula = calculo.Formula;

                if (diccionarioVariables != null && diccionarioVariables.Count > 0)
                {
                    foreach (var entrada in diccionarioVariables)
                    {
                        if (formula.Contains(entrada.Key))
                            formula = formula.Replace("{" + entrada.Key + "}", entrada.Value.ToString());
                    }
                }

                //Se hace la evaluación de la formula
                var expression = new Expression(formula, EvaluateOptions.NoCache);
                if (habilitarReporte)
                {
                    bitacoraMensajes.InsertarMensaje(Tenant, "Expression [ " + calculo.Formula + " ]: " + formula);
                }
                resultadoFinal = decimal.Parse(expression.Evaluate().ToString(), NumberStyles.Any, new CultureInfo(configuracion.Cultura));
                expression = null;

                if (variableAlmacenar != null)
                {
                    //Se valida la variable a almacenar
                    if (variableAlmacenar.TipoDato != TipoDato.Decimal)
                        throw new GestorCalculosException("GestorCalculosError_VariableInvalida", variableAlmacenar.ID);

                    //Se formatea el resultado
                    resultadoFinal = resultadoFinal.TruncarValor(variableAlmacenar.DigitosFlotantes, variableAlmacenar.TipoRedondeo);

                    buffer.Add(resultadoFinal); //Resultado
                    buffer.Add(variableAlmacenar.ID); //Concepto
                    AlmacenarCalculo(variableAlmacenar, configuracion, buffer);
                }


                if (habilitarReporte)
                {

                    string mensajeResultado = (buffer != null && buffer.Count > 2) ? string.Format(FORMATO_RESULTADO_SEMILLA, resultadoFinal.ToString(), buffer[0].ToString() + " , " + buffer[1]?.ToString()) : resultadoFinal.ToString();

                    //Se escribe el resultado en la lista de mensajes
                    bitacoraMensajes.InsertarMensaje(Tenant, "@GestorCalculosInfo_ResultadoCalculo", calculo.ID, mensajeResultado);
                }
            }

            //Retorna el resultado de la evaluación
            return resultadoFinal;
        }

        /// <summary>
        /// Permite almacenar el cálculo
        /// </summary>
        /// <param name="variableAlmacenamiento">variable utilizada para el almacenamiento del cálculo</param>
        /// <param name="configuracion">configuración utilizada en el almacenamiento de la información</param>
        /// <param name="buffer">Buffer con la información temporal a almacenar</param>
        private void AlmacenarCalculo(VariableTO variableAlmacenamiento, ConfiguracionTO configuracion, List<object> buffer)
        {
            try
            {
                //Se obtiene la fuente de la variable
                //FuenteTO fuente = ObtenerFuente(variableAlmacenamiento.IdFuente);
                RepositorioTO repositorio = (RepositorioTO)ObtenerFuente(variableAlmacenamiento.IdFuente);

                if (repositorio == null)
                    throw new GestorCalculosException("GestorCalculosError_VariableFuente", variableAlmacenamiento.ID);

                switch (repositorio.Tipo)
                {
                    case TipoFuente.Repositorio:

                        //Insercion por lotes SQLBulkCopy
                        var tipoBulkCopy = configuracion.Calculos.Where(c => c.Variables.Contains(variableAlmacenamiento)).FirstOrDefault().BulkCopy;
                        if (!tipoBulkCopy.Equals(TipoBulkCopy.Ninguno) && repositorio.HabilitarTransaccion)
                        {
                            // Add Resitro a tabla Memoria
                            AddResultadoADataTable(repositorio, configuracion, buffer);
                        }
                        //Insercion registro a registro
                        else
                        {
                            GestorCalculosServiceLocator.GetService<GestorDatosBase>(
                                configuracion.TipoProveedor.ToString()).ExecuteNonQuery(Tenant, repositorio, configuracion, buffer, Identificador);
                        }


                        break;
                    case TipoFuente.Servicio:
                        break;
                    case TipoFuente.Archivo:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new GestorCalculosException("GestorCalculosError_ExcepcionEnFuente", ex, variableAlmacenamiento.IdFuente);
            }
        }


        /// <summary>
        /// Adiciona rgistro a la tabla en Memoria de Repositorio
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="configuracion"></param>
        /// <param name="buffer"></param>
        public void AddResultadoADataTable(RepositorioTO repository, ConfiguracionTO configuracion, List<object> buffer)
        {

            string idTabla = repository.ID;

            lock (dataTableDictionary)
            {
                if (dataTableDictionary.ContainsKey(idTabla))
                {

                    // Create a new row
                    DataRow newRow = dataTableDictionary[repository.ID].NewRow();

                    foreach (var parametro in repository.Parametros)
                    {
                        if (parametro.Direccion == DireccionValor.Entrada)
                        {
                            object valor = parametro.Valor.ObtenerValorFormateado(configuracion.Nombre,
                               configuracion.Cultura, parametro, configuracion.Parametros, buffer, Identificador);

                            newRow[parametro.Nombre] = valor;
                        }
                    }

                    dataTableDictionary[idTabla].Rows.Add(newRow);
                }
            }
        }

        /// <summary>
        /// Creat Datatable temporal para almacenar los registo antes de enviarlos a la DB
        /// </summary>
        /// <param name="repository"></param>
        public void CrearDataTable(RepositorioTO repository)
        {
            string idTabla = repository.ID;

            if (!dataTableDictionary.ContainsKey(idTabla))
            {

                DataTable dataTableRepo = new DataTable(repository.Sql);

                if (repository.Parametros != null && repository.Parametros.Count > 0)
                {
                    foreach (var parametro in repository.Parametros)
                    {
                        if (parametro.Direccion == DireccionValor.Entrada)
                        {
                            // Create Column 
                            DataColumn column = new DataColumn();
                            column.DataType = parametro.TipoDato.GetSystemType();
                            column.ColumnName = parametro.Nombre;

                            // Add the columns to the ProductSalesData DataTable
                            dataTableRepo.Columns.Add(column);

                        }
                    }
                }

                dataTableDictionary.Add(idTabla, dataTableRepo);
            }

        }

        /// <summary>
        /// Publica el avance de los cálculos en la cola.
        /// </summary>
        /// <param name="avance">Avance actual.</param>
        /// <param name="total">Total de registros.</param>
        /// <param name="calculo">Cálculo que se está ejecutando.</param>
        /// <param name="grupo">Grupo al que pertenece el cálculo.</param>
        private void PublicarProgresoGeneral(int avance, int total, CalculoTO calculo, GrupoTO grupo = null)
        {
            var progreso = (avance * 100) / total;
            var message = new { IdProcesoGestor = Identificador, Progreso = progreso, Calculo = calculo.Nombre, Grupo = grupo?.Nombre };

            if (_publicarProgresoAsincrono)
                Task.Factory.StartNew(() => ServiceBus?.Publish(Tenant, message), TaskCreationOptions.LongRunning);
            else
                ServiceBus?.Publish(Tenant, message);
        }

        /// <summary>
        /// Publica el progreso de la semilla.
        /// </summary>
        /// <param name="avance">Avance actual.</param>
        /// <param name="total">Total de registros.</param>
        /// <param name="calculo"></param>
        private void PublicarProgresoDetalle(int avance, int total, CalculoTO calculo)
        {
            double evalue = (double)total / 10; // Always sends max 10 progress notifications

            // Only report progress according to number of progress messages by calculation defined  (10)
            if (( avance % (evalue)) < 1)
            {
                var progreso = (avance * 100) / total;
                var message = new { IdProcesoGestor = Identificador, Progreso = progreso, EsProgresoSemilla = true, Calculo = calculo.Nombre };

                if (_publicarProgresoAsincrono)
                    Task.Factory.StartNew(() => ServiceBus?.Publish(Tenant, message), TaskCreationOptions.LongRunning);
                else
                    ServiceBus?.Publish(Tenant, message);
            }
        }



        /// <summary>
        /// Publica un error en la semilla.
        /// </summary>
        /// <param name="nomnbreCalculo">Nombre del Calculo</param>
        private void PublicarErrorSemilla(string nomnbreCalculo)
        {
            var message = new { IdProcesoGestor = Identificador, Calculo = nomnbreCalculo, HayErrorSemilla = true };

            if (_publicarProgresoAsincrono)
                Task.Factory.StartNew(() => ServiceBus?.Publish(Tenant, message), TaskCreationOptions.LongRunning);
            else
                ServiceBus?.Publish(Tenant, message);
        }
        #endregion


    }
}
