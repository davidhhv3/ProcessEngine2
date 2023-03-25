#region Derechos Reservados
// ===================================================
// Desarrollado Por	    : esteban.giraldo
// Fecha de Creación	: 2015-02-06
// Modificado Por       : esteban.giraldo
// Fecha Modificación   : 2015-02-26
// Empresa		        : MVM S.A.S
// ===================================================
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Caching;
using Microsoft.Practices.EnterpriseLibrary.Caching.Expirations;
using MVM.ProcessEngine.Common.Exceptions;
using MVM.ProcessEngine.Common.Helpers;
using MVM.ProcessEngine.Interfaces;
using MVM.ProcessEngine.TO;
using System.Data;
using MVM.ProcessEngine.Core.DataAccess;
using MVM.ProcessEngine.Plugin;

namespace MVM.ProcessEngine.Core
{
    /// <summary>
    /// Clase encargada de implementar los métodos del proceso de cálculo
    /// </summary>
    internal class CalculoService : ICalculoService
    {

        #region Campos
        private BackgroundWorker worker = null;

        private string Tenant;
        #endregion

        #region Constructor
        /// <summary>
        /// Crea una nueva instancia de la clase <see cref="MVM.ProcessEngine.Core.CalculoService"/>
        /// </summary>
        public CalculoService() { }
        #endregion

        #region ICalculoService Members
        ///<summary>
        /// <see cref="MVM.ProcessEngine.Interfaces.ICalculoService.EjecutarCalculo"/>
        ///</summary>
        public string EjecutarCalculo(string tenant, string nombreConfiguracionArchivo, string idGrupo, string idCalculo, bool modoAsincrono, bool procesarCalculosEnParalelo, bool interrumpirCalculoPorExcepcion, params object[] parametros)
        {

            Tenant = tenant;

            //Se crea la instancia del proceso de cálculo
            var gestorCalculo = new GestorCalculo(tenant,
                idCalculo != null ? nombreConfiguracionArchivo + ":" + idCalculo : nombreConfiguracionArchivo)
            {
                Tenant = tenant,
                IdGrupo = idGrupo,
                IdCalculo = idCalculo,
                NombreConfiguracion = nombreConfiguracionArchivo,
                ModoAsincrono = modoAsincrono,
                InterrumpirCalculoPorExcepcion = interrumpirCalculoPorExcepcion,
                HabilitarRegistroDetalleTecnico = GestorCalculosHelper.GetMetadataValue(tenant, "HabilitarRegistroDetalleTecnico", true).ToBoolean(),
                HabilitadoImpresionVariables = GestorCalculosHelper.GetMetadataValue(tenant, "HabilitarImpresionVariables", true).ToBoolean(),
                Parametros = (parametros != null) ? parametros.ToList() : null
            };

            if (!modoAsincrono)
            {
                //Ejecuta el proceso de cálculo de forma síncrona
                gestorCalculo.EjecutarProceso();
            }
            else
            {
                var serviceBus = VerificarUsoEncolamiento(tenant);
                gestorCalculo.ServiceBus = serviceBus.Result;

                //Se inicializa la configuración por defecto del worker de trabajo asíncrono
                InicializarWorker();
                //Se comienza la ejecución asíncrona del proceso
                worker.RunWorkerAsync(gestorCalculo);
                gestorCalculo.Worker = worker;
            }

            //Retorna el identificador del proceso para consultar los resultados
            return gestorCalculo.Identificador;
        }

        ///<summary>
        /// <see cref="MVM.ProcessEngine.Interfaces.ICalculoService.CancelarCalculo"/>
        ///</summary>
        public void CancelarCalculo(string tenant, string identificador)
        {
            GestorCalculo gestorCalculo = new GestorCalculo();
            var serviceBus = VerificarUsoEncolamiento(tenant);
            gestorCalculo.ServiceBus = serviceBus.Result;

            var mensaje = new { IdProcesoGestor = identificador, Cancelado = true, Mensaje = "Proceso Cancelado..." };
            gestorCalculo.ServiceBus?.Publish(tenant, mensaje);
        }

        ///<summary>
        /// <see cref="MVM.ProcessEngine.Interfaces.ICalculoService.ObtenerDataRepositorio"/>
        ///</summary>
        public DataTable ObtenerDataRepositorio(string tenant, RepositorioTO repository, ConfiguracionTO configuracion, List<object> buffer)
        {
            return GestorCalculosServiceLocator.GetService<GestorDatosBase>(
                              configuracion.TipoProveedor.ToString()).GetData(tenant, repository, configuracion, buffer, null);
        }

        ///<summary>
        /// <see cref="MVM.ProcessEngine.Interfaces.ICalculoService.EjecutarRepositorio"/>
        ///</summary>
        public void EjecutarRepositorio(string tenant, RepositorioTO repository, ConfiguracionTO configuracion, List<object> buffer)
        {
            GestorCalculosServiceLocator.GetService<GestorDatosBase>(
                              configuracion.TipoProveedor.ToString()).ExecuteNonQuery(tenant, repository, configuracion, buffer, null);
        }

        public async Task<string> GuardarDataRepositorio(string tenant, string activityName, RepositorioTO repository, ConfiguracionTO configuracion, List<object> buffer)
        {
            var data = GestorCalculosServiceLocator.GetService<GestorDatosBase>(
                              configuracion.TipoProveedor.ToString()).GetData(tenant, repository, configuracion, buffer, null);
            var fileBytes = FileHelper.GetXlsFromDataTable(data);
            var path = await AzureStorageHelper.CreateBlob(tenant, activityName, "reports", fileBytes);

            return path;
        }

        #endregion

        #region Métodos privados
        /// <summary>
        /// Permite inicializar el worker que gestiona el trabajo asíncrono
        /// </summary>
        private void InicializarWorker()
        {
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
        }

        /// <summary>
        /// Permite validar el estado actual del proceso de cálculo
        /// </summary>
        /// <param name="gestorCalculo">Proceso de cálculo a validar</param>
        /// <returns>Objeto del tipo ResultadoProcesoTO con la información del estado actual del proceso ed cálculo</returns>
        private ResultadoProcesoTO ValidarEstadoActualCalculo(GestorCalculo gestorCalculo)
        {
            var resultadoOperacion = new ResultadoProcesoTO();

            if (gestorCalculo == null)
            {
                resultadoOperacion.Estado = EstadoProceso.Cancelado;
                resultadoOperacion.Exitoso = false;
                resultadoOperacion.Mensaje = BitacoraMensajesHelper.ObtenerMensajeRecursos("GestorCalculosInfo_ProcesoNoDisponible");
            }
            else if (gestorCalculo.ModoAsincrono && gestorCalculo.Worker.IsBusy
               && !gestorCalculo.Worker.CancellationPending)
            {
                resultadoOperacion.Exitoso = false;
                resultadoOperacion.Mensaje = BitacoraMensajesHelper.ObtenerMensajeRecursos("GestorCalculosInfo_ProcesoActivo");
                resultadoOperacion.Estado = EstadoProceso.Ejecutando;
            }
            else if (gestorCalculo.ModoAsincrono && gestorCalculo.Worker.CancellationPending)
            {
                resultadoOperacion.Exitoso = false;
                resultadoOperacion.Mensaje = BitacoraMensajesHelper.ObtenerMensajeRecursos("GestorCalculosInfo_ProcesoCancelado");
                resultadoOperacion.Estado = EstadoProceso.Cancelado;
            }
            else
            {
                resultadoOperacion.Exitoso = true;
                resultadoOperacion.Mensaje = BitacoraMensajesHelper.ObtenerMensajeRecursos("GestorCalculosInfo_ProcesoCompletado");
                resultadoOperacion.Estado = EstadoProceso.Completado;
            }

            return resultadoOperacion;
        }

       
      

        #endregion

        #region Manejadores de eventos
        /// <summary>
        /// Manejador del evento que ejecuta el proceso asíncrono
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var gestorCalculo = e.Argument as GestorCalculo;

            if (gestorCalculo == null)
                throw new GestorCalculosException("GestorCalculosError_ProcesamientoAsincrono");

            //Se inicializa el proceso de carga de información
            gestorCalculo.EjecutarProceso();

            if (worker.CancellationPending)
            {
                gestorCalculo.Cancelado = true;
            }

            e.Result = gestorCalculo;
        }

        /// <summary>
        /// Manejador del evento en el momento de completarse el procesamiento
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                var proceso = e.Result as GestorCalculo;
                if (proceso != null && !proceso.Cancelado)
                {
                    // doesn't publish asynchronously because it has already ended the work.
                    var mensaje = new { IdProcesoGestor = proceso.Identificador, Completado = true, HayError = false, Mensaje = "Completado exitosamente." };
                    proceso?.ServiceBus?.Publish(Tenant,mensaje);

                }
                else if (proceso != null && proceso.Cancelado)
                {
                    var mensaje = new { IdProcesoGestor = proceso.Identificador, Cancelado = true, Mensaje = "Proceso Cancelado..." };                    
                    proceso?.ServiceBus?.Publish(Tenant,mensaje);
                }
            }
        }

 
        /// <summary>
        /// Verifica si el gestor está configurado paea encolar los mensajes.
        /// </summary>
        /// <returns>Implementación del </returns>
        //private async Task<StorageQueueMessagePublisher> VerificarUsoEncolamiento(string tenant)
        //{
        //    StorageQueueMessagePublisher serviceBus = null;
        //    var useServiceBus = bool.Parse(GestorCalculosHelper.GetMetadataValue(tenant,"UseServiceBus", true));

        //    if (useServiceBus)
        //    {
        //        serviceBus = new StorageQueueMessagePublisher();
        //    }

        //    return serviceBus;
        //}


        private async Task<ServicesBusQueueMessagePublisher> VerificarUsoEncolamiento(string tenant)
        {
            ServicesBusQueueMessagePublisher serviceBus = null;
            var useServiceBus = bool.Parse(GestorCalculosHelper.GetMetadataValue(tenant, "UseServiceBus", true));

            if (useServiceBus)
            {
                serviceBus = new ServicesBusQueueMessagePublisher();
            }

            return serviceBus;
        }
        #endregion
    }
}
