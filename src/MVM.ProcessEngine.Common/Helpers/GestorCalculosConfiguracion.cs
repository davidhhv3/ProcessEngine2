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
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using MVM.ProcessEngine.Common.Exceptions;
using MVM.ProcessEngine.TO;
using System.Net;
using System.Reflection;

namespace MVM.ProcessEngine.Common.Helpers
{
    /// <summary>
    /// Clase que provee la funcionalidad para gestionar la configuración de los archivos
    /// </summary>
    public class GestorCalculosConfiguracion
    {
        //public static string _rutaConfiguraciones = GestorCalculosHelper.ObtenerAtributoDeConfiguracion("RutaConfiguraciones", true);
        //private static string _rutaEsquema = GestorCalculosHelper.ObtenerAtributoDeConfiguracion("RutaEsquema", true);
        //private static string _nombrePlantilla = GestorCalculosHelper.ObtenerAtributoDeConfiguracion("EsquemaValidacion", true);

        /// <summary>
        /// Obtiene en un objeto DataSet la información de la configuración
        /// </summary>
        /// <param name="nombreConfiguracion">Nombre de la configuración a obtener</param>
        /// <returns>Objeto del tipo DataSet con la infomración de la configuración</returns>
        //public static DataSet ObtenerDataSetConfiguracion(string nombreConfiguracion)
        //{
        //    var datos = new DataSet();

        //    using (var stream = new StreamReader(string.Format(@"{0}\{1}", _rutaConfiguraciones, nombreConfiguracion)))
        //    {
        //        datos.ReadXml(stream);
        //    }

        //    return datos;
        //}

        ///// <summary>
        ///// Obtiene en un XDocument de Linq la información de la configuración
        ///// </summary>
        ///// <param name="nombreConfiguracion">Nombre de la configuración a obtener</param>
        ///// <returns>Objeto del tipo XDocument con la infomración de la configuración</returns>
        //public static XDocument ObtenerXDocumentConfiguracion(string nombreConfiguracion)
        //{
        //    return XDocument.Load(string.Format(@"{0}\{1}", _rutaConfiguraciones, nombreConfiguracion));
        //}

        /// <summary>
        /// Permite obtener el objeto de configuración del cálculo
        /// </summary>
        /// <param name="nombreConfiguracion">Nombre del objeto de configuración a obtener</param>
        /// <returns>El objeto de configuración con la información del archivo</returns>
        public static ConfiguracionTO ObtenerConfiguracionCalculo(string tenant,string nombreConfiguracion, string idProceso, bool inAzureblob = true)
        {
            if (string.IsNullOrEmpty(nombreConfiguracion))
                throw new ArgumentNullException("nombreConfiguracion");

            //Si el nombre viene sin extensión, se establece
            nombreConfiguracion = nombreConfiguracion.ToUpper().EndsWith(".XML")
                                      ? nombreConfiguracion : string.Format("{0}.XML", nombreConfiguracion);

            ConfiguracionTO configuracionCalculo = null;
            string xml;

            if (inAzureblob) // Azure Bblob 
            {

                xml = AzureStorageHelper.GetBlockBlobAsText(tenant,nombreConfiguracion);

                var sb = new StringBuilder();

                //Se lee y se valida el archivo con el esquema
                if (!SchemaValidate(xml, idProceso, sb))
                {
                    throw new GestorCalculosException("GestorCalculosError_ValidacionArchivo");
                }

                xml = sb.ToString();
                configuracionCalculo = GestorCalculosHelper.DeserializeXml<ConfiguracionTO>(xml);
            }
            else // Ruta fisica
            {

                var rutaConfiguraciones = GestorCalculosHelper.GetMetadataValue(tenant,"RutaConfiguraciones", true);

                //Se valida que el archivo exista
                string rutaCompleta = string.Format(@"{0}\{1}", rutaConfiguraciones, nombreConfiguracion);

                if (!File.Exists(rutaCompleta))
                    throw new GestorCalculosException("GestorCalculosError_ConfiguracionInexistente");

                try
                {
                    var sb = new StringBuilder();

                    using (var reader = new StreamReader(rutaCompleta))
                    {
                        //Se lee y se valida el archivo con el esquema
                        if (!ValidateAndReadFile(tenant,reader, idProceso, sb))
                        {
                            throw new GestorCalculosException("GestorCalculosError_ValidacionArchivo");
                        }
                    }

                    xml = sb.ToString();
                    configuracionCalculo = GestorCalculosHelper.DeserializeXml<ConfiguracionTO>(xml);
                }
                catch (Exception ex)
                {
                    throw new GestorCalculosException("GestorCalculosError_Configuracion", ex);
                }
            }



            return configuracionCalculo;
        }

        /// <summary>
        /// Permite validar el archivo de configuración y leer el contenido del XML
        /// </summary>
        /// <param name="stream">Flujo de butes con el contenido del archivo</param>
        /// <param name="idProceso">El identificador del proceso</param>
        /// <param name="sb">Parametro de salida con el contenido del archivo validado</param>
        /// <returns>Retorna el resultado de la validación</returns>
        private static bool ValidateAndReadFile(string tenant,StreamReader stream, string idProceso, StringBuilder sb)
        {
            bool isValid = true;

            //Set the validation settings
            var settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

            string rutaEsquema = GestorCalculosHelper.GetMetadataValue(tenant,"RutaEsquema", true);
            string nombrePlantilla = GestorCalculosHelper.GetMetadataValue(tenant,"EsquemaValidacion", true);

            settings.Schemas.Add(null, string.Format(@"{0}\{1}", rutaEsquema, nombrePlantilla));

            settings.ValidationEventHandler += delegate (object sender, ValidationEventArgs args)
            {
                isValid = false;
                if (args.Severity == XmlSeverityType.Warning)
                {
                    throw new GestorCalculosException(BitacoraMensajesHelper.ObtenerMensajeRecursos("@GestorCalculosWarning_ValidacionEsquema", args.Message, args.Exception.LineNumber.ToString(), args.Exception.LinePosition.ToString()));
                }
                else
                {
                    throw new GestorCalculosException(BitacoraMensajesHelper.ObtenerMensajeRecursos("@GestorCalculosError_ValidacionEsquema", args.Message, args.Exception.LineNumber.ToString(), args.Exception.LinePosition.ToString()));
                }
            };

            using (var reader = XmlReader.Create(stream, settings))
            {
                // Parse the file and get xml content into the string builder. 
                while (reader.Read())
                {
                    sb.Append(reader.ReadOuterXml());
                }
            }

            return isValid;
        }

        /// <summary>
        /// Permite validar el archivo de configuración contra le schema xsd, este esquema debe estar embebido en la dll
        /// </summary>
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="idProceso"></param>
        /// <returns></returns>
        public static bool SchemaValidate(string xml, string idProceso, StringBuilder sb)
        {
            bool isValid = true;

            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                using (var streamReader = new StreamReader(memoryStream))
                {

                    //Set the validation settings
                    var settings = new XmlReaderSettings();
                    settings.ValidationType = ValidationType.Schema;
                    settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                    settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
                    settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

                    Assembly myAssembly = Assembly.GetExecutingAssembly();
                    using (Stream schemaStream = myAssembly.GetManifestResourceStream("MVM.ProcessEngine.Common.Template.xsd"))
                    {
                        XmlSchema schema = XmlSchema.Read(schemaStream, null);
                        settings.Schemas.Add(schema);
                    }

                    settings.ValidationEventHandler += delegate (object sender, ValidationEventArgs args)
                    {
                        isValid = false;
                        //BitacoraMensajesHelper bitacoraMensajes = new BitacoraMensajesHelper();

                        if (args.Severity == XmlSeverityType.Warning)
                        {
                            //bitacoraMensajes.InsertarMensaje("@GestorCalculosWarning_ValidacionEsquema", args.Message, args.Exception.LineNumber.ToString(), args.Exception.LinePosition.ToString());
                            throw new GestorCalculosException(BitacoraMensajesHelper.ObtenerMensajeRecursos("GestorCalculosWarning_ValidacionEsquema", args.Message, args.Exception.LineNumber.ToString(), args.Exception.LinePosition.ToString()));
                        }
                        else
                        {
                            //bitacoraMensajes.InsertarMensaje("@GestorCalculosError_ExcepcionProceso", args.Message, args.Exception.LineNumber.ToString(), args.Exception.LinePosition.ToString());
                            throw new GestorCalculosException(BitacoraMensajesHelper.ObtenerMensajeRecursos("GestorCalculosError_ValidacionEsquema", args.Message, args.Exception.LineNumber.ToString(), args.Exception.LinePosition.ToString()));
                        }

                        throw new GestorCalculosException("Error validando esquema del XML.");
                    };

                    using (var reader = XmlReader.Create(streamReader, settings))
                    {
                        // Parse the file and get xml content into the string builder. 
                        while (reader.Read())
                        {
                            sb.Append(reader.ReadOuterXml());
                        }
                    }
                }
            }

            return isValid;
        }
    }
}
