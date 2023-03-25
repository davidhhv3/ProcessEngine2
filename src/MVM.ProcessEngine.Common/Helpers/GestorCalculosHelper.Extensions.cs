#region Derechos Reservados
// ===================================================
// Desarrollado Por	    : esteban.giraldo
// Fecha de Creación	: 2015-02-06
// Modificado Por       :
// Fecha Modificación   :
// Empresa		        : MVM S.A.S
// ===================================================
#endregion

using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using MVM.ProcessEngine.TO;
using System.Globalization;
using Spring.Context.Support;
using Spring.Caching;
using System.Threading;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Data;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using System.Diagnostics;
using MVM.ProcessEngine.Common.Exceptions;
using Newtonsoft.Json.Linq;
using Microsoft.SqlServer.Server;

namespace MVM.ProcessEngine.Common.Helpers
{
    /// <summary>
    /// Clase que contiene métodos utilitarios para el servicio del gestor de cálculos
    /// </summary>
    /// <remarks>Esta clase se especializa en las extensiones de los tipos</remarks>
    public static partial class GestorCalculosHelper
    {
        #region String
        /// <summary>
        /// Verifica si un texto es nulo o vacio luego de eliminar los espacios en blanco.
        /// </summary>
        /// <param name="value">Texto a evaluar.</param>
        /// <returns>True si el texto es nulo o si queda vacio luego de eliminar los espacios en blanco.</returns>
        public static bool IsNullOrEmptyTrim(this string value)
        {
            return value != null ? string.IsNullOrEmpty(value.Trim()) : true;
        }

        /// <summary>
        /// Devuelve el texto con la primera letra de cada palabra en mayúscula.
        /// </summary>
        /// <param name="value">Cadena a convertir</param>
        /// <returns>Cadena con cambio</returns>
        public static string ToTitleCase(this string value)
        {
            return CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(value.ToLower());
        }

        /// <summary>
        /// Remueve los acentos y tildes de un texto.
        /// </summary>
        /// <param name="value">Cadena a la cual se le removeran los acentos y tildes.</param>
        /// <returns>Cadena sin acentuaciones y tildes, cadena en blanco si el valor es nulo o vacío.</returns>
        public static string ToStringWithoutAccents(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            String stringFormD = value.Normalize(NormalizationForm.FormD);

            StringBuilder sb = new StringBuilder();

            for (int characterCount = 0; characterCount < stringFormD.Length; characterCount++)
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(stringFormD[characterCount]);

                if (category != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stringFormD[characterCount]);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Permite convertir el valor string a la enumeración especificada
        /// </summary>
        /// <typeparam name="T">Tipo de enumeración a convertir</typeparam>
        /// <param name="valor">Valor de la enumeración a convertir</param>
        /// <returns>El tipo de enumeración específica</returns>
        public static T ConvertirAEnumeracion<T>(this string valor)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), valor, true);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        /// <summary>
        /// Obtiene el valor de la enumeración a partir de una descripción
        /// </summary>
        /// <param name="value">valor string para recuperar en la enumeración</param>
        /// <param name="tipoEnum">Tipo de enumeración</param>
        /// <param name="ignorarMayusculas">Indica si se deben ignorar las mayusculas en la comparación del tipo</param>
        /// <returns>Retorna el valor del enumerado</returns>
        public static T GetEnumDescriptionValue<T>(this string value, bool ignorarMayusculas)
        {
            Type tipoEnum = typeof(T);
            var resultado = default(T);
            string enumDescriptionValue = null;

            if (tipoEnum.IsEnum)
            {
                foreach (FieldInfo fieldInfo in tipoEnum.GetFields())
                {
                    var atributos =
                        fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

                    //Se obtiene el DescriptionAttribute de cada miembro del enum
                    if (atributos != null && atributos.Length > 0)
                    {
                        enumDescriptionValue = atributos[0].Description;

                        if (enumDescriptionValue.ToUpper() == value.ToUpper())
                        {
                            resultado = (T)Enum.Parse(tipoEnum, fieldInfo.Name, true);
                            break;
                        }
                    }
                }

                if (resultado == null)
                    resultado = (T)Enum.GetValues(tipoEnum).GetValue(0);
            }

            return resultado;
        }

        /// <summary>
        /// Devuelve la cadena con la primera letra de cada palabra
        /// en mayúscula
        /// </summary>
        /// <param name="cadena">Cadena a convertir</param>
        /// <returns>Cadena con cambio</returns>
        public static string ToInitCap(string cadena)
        {
            return CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(cadena.ToLower());
        }

        /// <summary>
        /// Compara cadenas dependiendo a partir de una máscara y devuelve falso en caso de no cumplir con ella.
        /// </summary>
        /// <param name="dato">texto para ser comparado</param>
        /// <param name="mascara">Máscara para comparar contra el dato</param>
        /// <returns>verdadero si coincide con la máscara, de lo contrario falso</returns>
        public static bool CompararMascara(this string dato, string mascara)
        {
            string[] masc = mascara.ToUpper().Split('*');
            dato = dato.ToUpper();
            bool correcto = true;

            string resto = dato;
            for (int i = 0; i < masc.Length && correcto; i++)
            {
                if (i == 0 && !string.IsNullOrEmpty(masc[i]))
                {
                    if (resto.StartsWith(masc[i]))
                        resto = resto.Substring(masc[i].Length);
                    else
                        correcto = false;
                }
                else if (i == (masc.Length - 1) && !string.IsNullOrEmpty(masc[i]))
                {
                    if (!resto.EndsWith(masc[i]))
                        correcto = false;
                }
                else
                {
                    if (!string.IsNullOrEmpty(masc[i]))
                    {
                        if (resto.IndexOf(masc[i]) >= 0)
                            resto = resto.Substring(resto.IndexOf(masc[i]) + masc[i].Length);
                        else
                            correcto = false;
                    }
                }
            }

            return correcto;
        }

        /// <summary>
        /// Adiciona espacios a la cadena de caracteres
        /// </summary>
        /// <param name="valor">Valor al cual se le adiciona espacios</param>
        /// <param name="cantidadEspacios">Cantidad de espacios</param>
        public static void AdicionarEspacios(this string valor, int cantidadEspacios)
        {
            for (int i = 0; i < cantidadEspacios; i++)
            {
                valor += " ";
            }
        }

        /// <summary>
        /// Se ajusta el tamaño de la cadena de caracteres con espacios en blanco acorde a la longitud
        /// </summary>
        /// <param name="valor">valor a redimencionar</param>
        /// <param name="longitud">Longitud de espacios</param>
        /// <param name="caracterLlenado">Caracter de llenado utilizado</param>
        /// <param name="alineacion">Alineación del texto</param>
        /// <returns>Retorna cadena de caracteres con el valor ajustado a los espacios</returns>
        public static string AjustarTamañoEspacios(this string valor, int longitud, string caracterLlenado, AlineacionTexto alineacion)
        {
            if (valor.Length > longitud)
            {
                valor = valor.Substring(0, longitud);
            }
            else if (valor.Length < longitud)
            {
                if (caracterLlenado == null)
                {
                    caracterLlenado = " ";
                }

                if (alineacion == AlineacionTexto.Izquierda)
                {
                    valor = valor.PadRight(longitud, char.Parse(caracterLlenado));
                }
                else
                {
                    valor = valor.PadLeft(longitud, char.Parse(caracterLlenado));
                }
            }

            return valor;
        }

        /// <summary>
        /// Permite obtener el valor aplicado el formato
        /// </summary>
        /// <param name="valor">Valor a obtener</param>
        /// <param name="nombreConfiguracion">Nombre del archivo de configuración</param>
        /// <param name="cultura">cultura actual utilizada para formatear los datos</param>
        /// <param name="parametro">Parametro actual que contiene el esquema</param>
        /// <param name="parametros">Lista de parámetros dinámicos enviados en la ejecución del cálculo</param>
        /// <param name="buffer">Lista de parámetros dinámicos enviados en el procesamiento del cálculo</param>
        /// <param name="idGestor">Identificador del proceso.</param>
        /// <returns>Objeto formateado</returns>
        public static object ObtenerValorFormateado(this string valor, string nombreConfiguracion, string cultura,
            ParametroTO parametro, List<object> parametros, List<object> buffer, string idGestor)
        {
            object result = null;

            if (parametro == null)
                throw new ArgumentNullException("parametro");

            #region Valor por defecto
            if (!string.IsNullOrEmpty(parametro.Defecto) && string.IsNullOrEmpty(valor.ToString()))
                result = parametro.Defecto;
            #endregion

            #region Constantes
            if (!string.IsNullOrEmpty(valor))
                result = GestorCalculosHelper.ObtenerValorConstante(nombreConfiguracion, valor, parametros, buffer, idGestor);
            #endregion

            #region Formateo de la lista
            if (parametro.TipoDato == TipoDato.ListString)
            {
                result = ((JArray)result).ToListSqlDataRecords();
            }
            #endregion

            #region Formateo de la fecha
            if (parametro.TipoDato == TipoDato.DateTimeString && !string.IsNullOrEmpty(parametro.Formato))
            {
                string nuevoValor = (result != null) ? result.ToString() : valor;
                var valorFecha = DateTime.Parse(nuevoValor, new CultureInfo(cultura));
                result = valorFecha.ToString(parametro.Formato);
            }
            #endregion

            return result;
        }

        /// <summary>
        /// Permite obtener el valor aplicado el formato
        /// </summary>
        /// <param name="valor">Valor a obtener</param>
        /// <param name="nombreConfiguracion">Nombre del archivo de configuración</param>
        /// <param name="cultura">cultura actual utilizada para formatear los datos</param>
        /// <param name="parametro">Parametro actual que contiene el esquema</param>
        /// <param name="parametros">Lista de parámetros dinámicos enviados en la ejecución del cálculo</param>
        /// <param name="buffer">Lista de parámetros dinámicos enviados en el procesamiento del cálculo</param>
        /// <param name="idGestor">Identificador del proceso.</param>
        /// <returns>Objeto formateado</returns>
        public static object ObtenerValorFormateado(this string valor, string nombreConfiguracion, string cultura,
            VariableTO variable, List<EquivalenciaTO> equivalencias, List<object> parametros, List<object> buffer, string idGestor)
        {
            object result = null;

            if (variable == null)
                throw new ArgumentNullException("parametro");

            #region Valor por defecto
            if (!string.IsNullOrEmpty(variable.ValorDefecto) && string.IsNullOrEmpty(valor.ToString()))
                result = variable.ValorDefecto;
            #endregion

            #region Constantes
            result = GestorCalculosHelper.ObtenerValorConstante(nombreConfiguracion, valor, parametros, buffer, idGestor);
            #endregion

            #region Equivalencias
            if (equivalencias != null && equivalencias.Count > 0)
            {
                EquivalenciaTO equivalencia = (from e in equivalencias
                                               where e.ValorOriginal == valor.ToString()
                                               select e).FirstOrDefault();

                if (equivalencia != null)
                    result = GestorCalculosHelper.ObtenerValorEquivalencia(nombreConfiguracion, equivalencia.ValorOriginal, equivalencia.ValorNuevo, parametros, buffer, idGestor);
            }
            #endregion

            #region Formateo de la fecha
            if (variable.TipoDato == TipoDato.DateTimeString && !string.IsNullOrEmpty(variable.Formato))
            {
                var valorFecha = DateTime.Parse(valor.ToString(), new CultureInfo(cultura));
                valor = valorFecha.ToString(variable.Formato);
                result = valor;
            }
            #endregion

            return result;
        }

        /// <summary>
        /// Permite hacer el formateo al tipo decimal
        /// </summary>
        /// <param name="valor"></param>
        /// <param name="cultura">La cultura a utilizar para el formateo</param>
        /// <param name="digitosFlotantes">Los digitos flotantes para truncar el decimal</param>
        /// <param name="tipoRedondeo">El tipo de redondeo a utilizar</param>
        /// <returns>Retorna el tipo decimal formateado</returns>
        public static decimal ToDecimalFormateado(this string valor, string cultura, int? digitosFlotantes, TipoRedondeo tipoRedondeo)
        {
            decimal resultado = decimal.Parse(valor, new CultureInfo(cultura));

            if (digitosFlotantes.HasValue)
                resultado = resultado.TruncarValor(digitosFlotantes.Value, tipoRedondeo);

            return resultado;
        }

        /// <summary>
        /// Permite validar si una expresión contiene algunos de los valores enviados
        /// </summary>
        /// <param name="haystack"></param>
        /// <param name="needles"></param>
        /// <returns></returns>
        public static bool ContainsAny(this string haystack, params string[] needles)
        {
            foreach (string needle in needles)
            {
                if (haystack.Contains(needle))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Permite convertir un tipo string a booleano
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ToBoolean(this string value)
        {
            if (value.ToUpper().Equals("TRUE"))
                return true;
            else
                return false;
        }
        #endregion

        #region DateTime y DateTime?
        /// <summary>
        /// Permite obtener los días del mes en un formato específico
        /// </summary>
        /// <returns>Lista de los días del mes</returns>
        public static List<SemillaTO> ObtenerDiasMes(this DateTime currentDate, bool incluirPeriodo)
        {
            var semillasDias = new List<SemillaTO>();
            int currentMonth = currentDate.Month;
            int currentYear = currentDate.Year;
            //Se obtienen la cantidad de días de mes actual
            int daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);

            for (int i = 1; i <= daysInMonth; i++)
            {
                string day = (i < 10) ? "0" + i.ToString() : i.ToString();
                string month = (currentMonth < 10) ? "0" + currentMonth.ToString() : currentMonth.ToString();
                string fechaFormateada = string.Format("{0}-{1}-{2}", currentYear.ToString(), month, day);

                if (incluirPeriodo)
                {
                    for (int j=1; j <=24; j++)
                    {
                        semillasDias.Add(new SemillaTO() { ValorPrincipal = fechaFormateada, ValorSecundario = j.ToString() });
                    }
                }
                else
                {
                    semillasDias.Add(new SemillaTO() { ValorPrincipal = fechaFormateada, ValorSecundario = "" });
                }
            }

            return semillasDias;
        }

        /// <summary>
        /// Dada una fecha verifica si se encuentra en el rango dado
        /// </summary>
        /// <param name="fecha">Fecha a validar</param>
        /// <param name="fechaini">Fecha inicial rango</param>
        /// <param name="fechafin">Fecha final rango</param>
        /// <returns></returns>
        public static bool Between(this DateTime fecha, DateTime? fechaini, DateTime? fechafin)
        {
            // acepta cuando la fecha es mayor a la fecha inicial
            // y cuando la fehca final tiene valor o esta abierta (nula)
            if (!fechaini.HasValue)
                throw new NullReferenceException("fechaini");

            return fecha >= fechaini && (fechafin.HasValue ? fecha <= fechafin : true);
        }

        /// <summary>
        /// Dada una fecha verifica si se encuentra en el rando dado
        /// </summary>
        /// <param name="fecha">Fecha a validar</param>
        /// <param name="fechaini">Fecha inicial rango</param>
        /// <param name="fechafin">Fecha final rango</param>
        /// <returns></returns>
        public static bool Between(this DateTime? fecha, DateTime? fechaini, DateTime? fechafin)
        {
            return fecha.HasValue ? fecha.Value.Between(fechaini, fechafin) : false;
        }

        /// <summary>
        /// Indica si la fecha es mayor a otra
        /// </summary>
        /// <param name="fecha"></param>
        /// <param name="fechacomparar"></param>
        /// <returns></returns>
        public static bool Mayor(this DateTime? fecha, DateTime? fechacomparar)
        {
            return fecha.HasValue ? fecha.Value.Mayor(fechacomparar) : false;
        }

        /// <summary>
        /// Indica si la fecha es mayor a otra
        /// </summary>
        /// <param name="fecha"></param>
        /// <param name="fechacomparar"></param>
        /// <returns></returns>
        public static bool Mayor(this DateTime fecha, DateTime? fechacomparar)
        {
            if (!fechacomparar.HasValue)
                throw new NullReferenceException("fecha");

            return fecha.Ticks.CompareTo(fechacomparar.Value.Ticks) > 0;
        }

        /// <summary>
        /// Indica si la fecha es menor a otra
        /// </summary>
        /// <param name="fecha"></param>
        /// <param name="fechacomparar"></param>
        /// <returns></returns>
        public static bool Menor(this DateTime? fecha, DateTime? fechacomparar)
        {
            return fecha.HasValue ? fecha.Value.Menor(fechacomparar) : false;
        }

        /// <summary>
        /// Indica si la fecha es menor a otra
        /// </summary>
        /// <param name="fecha"></param>
        /// <param name="fechacomparar"></param>
        /// <returns></returns>
        public static bool Menor(this DateTime fecha, DateTime? fechacomparar)
        {
            return fecha.Ticks.CompareTo(fechacomparar.Value.Ticks) < 0;
        }

        /// <summary>
        /// Indica los meses que existen entre 2 fechas
        /// </summary>
        /// <param name="fecha"></param>
        /// <param name="fechafutura"></param>
        /// <returns></returns>
        public static int MesesEntre(this DateTime fecha, DateTime fechafutura)
        {
            int anno = fechafutura.Year - fecha.Year;
            int mes = fechafutura.Month - fecha.Month;

            return anno * 12 + mes;
        }

        /// <summary>
        /// Indica los meses que existen entre 2 fechas
        /// </summary>
        /// <param name="fecha"></param>
        /// <param name="fechafutura"></param>
        /// <returns></returns>
        public static int MesesEntre(this DateTime? fecha, DateTime? fechafutura)
        {
            return fecha.Value.MesesEntre(fechafutura.Value);
        }

        /// <summary>
        /// Formatea una cadena para enviarle parámetros
        /// </summary>
        /// <param name="cadena"></param>
        /// <param name="para"></param>
        /// <returns></returns>
        public static string Formatear(this string cadena, params object[] para)
        {
            return string.Format(cadena, para);
        }

        /// <summary>
        /// Indica el primer día del mes
        /// </summary>
        /// <param name="fecha"></param>
        /// <returns></returns>
        public static DateTime PrimerDiaMes(this DateTime? fecha)
        {
            return fecha.Value.PrimerDiaMes();
        }

        /// <summary>
        /// Indica el primer día del mes
        /// </summary>
        /// <param name="fecha"></param>
        /// <returns></returns>
        public static DateTime PrimerDiaMes(this DateTime fecha)
        {
            return new DateTime(fecha.Year, fecha.Month, 1);
        }

        /// <summary>
        /// Indica el último día del mes
        /// </summary>
        /// <param name="fecha"></param>
        /// <returns></returns>
        public static DateTime UltimoDiaMes(this DateTime? fecha)
        {
            return fecha.Value.UltimoDiaMes();
        }

        /// <summary>
        /// Indica el último día del mes
        /// </summary>
        /// <param name="fecha"></param>
        /// <returns></returns>
        public static DateTime UltimoDiaMes(this DateTime fecha)
        {
            return new DateTime(fecha.Year, fecha.Month, DateTime.DaysInMonth(fecha.Year, fecha.Month));
        }

        /// <summary>
        /// Indice el último instante del mes con segundos, por ejemplo: 31 de enero 12:59:59
        /// </summary>
        /// <param name="fecha"></param>
        /// <returns></returns>
        public static DateTime UltimoInstanteMes(this DateTime fecha)
        {
            return fecha.AddMonths(1).AddSeconds(-1);
        }

        /// <summary>
        /// Devuelve una fecha con el formato yyyy-mm-dd
        /// </summary>
        /// <param name="fecha"></param>
        /// <returns></returns>
        public static string ToYYYYMMDD(this DateTime? fecha)
        {
            return fecha.Value.ToYYYYMMDD();
        }

        /// <summary>
        /// Devuelve una fecha con el formato yyyy-mm-dd
        /// </summary>
        /// <param name="fecha"></param>
        /// <returns></returns>
        public static string ToYYYYMMDD(this DateTime fecha)
        {
            return fecha.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// convierte una cadena con el formato yyyy-mm-dd a una fecha, ej: "1981-10-22".ToFechaYYYYMMDD()
        /// </summary>
        /// <param name="cadena">cadena con formato yyyy-mm-dd, ej: 1981-10-22</param>
        /// <returns></returns>
        public static DateTime ToFechaYYYYMMDD(this string cadena)
        {
            var arr = cadena.Split('-');
            return new DateTime(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]));
        }

        #endregion

        #region Decimal
        /// <summary>
        /// Permite truncar un valor acorde a la especificación de redondeo y digitos flutantes
        /// </summary>
        /// <param name="valor">Valor a truncar</param>
        /// <param name="numeroDigitosFlotantes">Digitos flotantes</param>
        /// <param name="tipoRedondeo">Tipo de redondeo a utilizar</param>
        /// <returns>Valor truncado</returns>
        public static Decimal TruncarValor(this decimal valor, int? numeroDigitosFlotantes, TipoRedondeo tipoRedondeo)
        {
            if (numeroDigitosFlotantes.HasValue)
            {
                switch (tipoRedondeo)
                {
                    case TipoRedondeo.Defecto:
                        valor = Math.Round(valor, numeroDigitosFlotantes.Value);
                        break;
                    case TipoRedondeo.Mayor:
                        valor = Math.Round(valor, numeroDigitosFlotantes.Value, MidpointRounding.AwayFromZero);
                        break;
                    case TipoRedondeo.Menor:
                        valor = Math.Round(valor, numeroDigitosFlotantes.Value, MidpointRounding.ToEven);
                        break;
                }
            }

            return valor;
        }
        #endregion

        #region List<T>
        /// <summary>
        /// Convierte una lista de Elementos en un DataTable.
        /// </summary>
        /// <typeparam name="T">Tipo de dato contenido en la lista.</typeparam>
        /// <param name="data">Información contenida en la lista.</param>
        /// <returns>DataTable con la información de la lista.</returns>
        public static DataTable ToDataTable<T>(this List<T> data) where T : new()
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();

            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }

            object[] values = new object[props.Count];

            for (int itemCount = 0; itemCount < data.Count; itemCount++)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(data[itemCount]);
                }
                table.Rows.Add(values);
            }
            return table;
        }

        #endregion

        #region Dictionary<string, string>
        /// <summary>
        /// Permite consultar un valor en la lista
        /// </summary>
        /// <param name="lista">Lista desde donde se efectuara la consulta</param>
        /// <param name="clave">Clave a consultar</param>
        /// <returns>Valor si es encontrado</returns>
        public static string ConsultarValorLista(this Dictionary<string, string> lista, string clave, string nombreColumnaArchivo, string nombreColumnaBD)
        {
            string valor = null;

            if (lista != null && lista.Count > 0)
            {
                if (!lista.TryGetValue(clave.ToString(), out valor))
                {
                    throw new GestorCalculosException("GestorCalculosError_BusquedaClaveValor", clave, nombreColumnaArchivo, nombreColumnaBD);
                }
            }

            return valor;
        }
        #endregion

        #region Objetos
        /// <summary>
        /// Indica si el objeto base contiene la propiedad.
        /// </summary>
        /// <param name="baseObject">Objeto al cual se le buscará la propiedad.</param>
        /// <param name="propertyName">Cadena con el nombre de la propiedad a buscar.</param>
        /// <returns>Verdadero si existe la propiedad, en caso contrario falso.</returns>
        public static bool HasProperty(this object baseObject, string propertyName)
        {
            Type dataType = baseObject.GetType();
            PropertyInfo property = dataType.GetProperty(propertyName);
            return (property != null);
        }

        /// <summary>
        /// Crea una nueva instancia de un Tipo en especifico con los valores 
        /// de las propiedades que se toman desde otro
        /// </summary>
        /// <typeparam name="T">Tipo de objeto a crear</typeparam>
        /// <param name="objetoBase">Objeto base del cual se tomaran los valores de las propiedades</param>
        /// <returns>Un nuevo objeto con los valores de las propiedades edl objetoBase</returns>
        public static T LoadObjectFrom<T>(this object objetoBase) where T : class, new()
        {
            var objetoResultado = new T();

            Type typeBase = objetoBase.GetType();
            PropertyInfo[] propiedadesResultado = objetoResultado.GetType().GetProperties();
            PropertyInfo propiedadBase;

            foreach (PropertyInfo propiedad in propiedadesResultado)
            {
                propiedadBase = typeBase.GetProperty(propiedad.Name);
                if (propiedadBase != null)
                {
                    propiedad.SetValue(objetoResultado, propiedadBase.GetValue(objetoBase, null), null);
                }
            }
            return objetoResultado;
        }

        /// <summary>
        /// Crea una nueva instancia de un Tipo en especifico con los valores 
        /// de las propiedades que se toman desde otro, si el objeto es null retorna null
        /// </summary>
        /// <typeparam name="T">Tipo de objeto a crear</typeparam>
        /// <param name="objetoBase">Objeto base del cual se tomaran los valores de las propiedades</param>
        /// <returns>Un nuevo objeto con los valores de las propiedades edl objetoBase</returns>
        public static T LoadObjectFromOrDefault<T>(this object objetoBase) where T : class, new()
        {
            if (objetoBase != null)
                return LoadObjectFromOrDefault<T>(objetoBase);
            return null;
        }

        /// <summary>
        /// Haces cast  de un objeto a otro
        /// </summary>
        /// <typeparam name="T">Tipo de objeto al que se le va a convertir</typeparam>
        /// <param name="objetoBase">Objeto base que se va a transformar</param>
        /// <returns>Null si no lo puede convertir, en caso contrario el objeto</returns>
        public static T Value<T>(this object objetoBase) where T : class
        {
            return objetoBase as T;
        }

        /// <summary>
        /// Establece un valor sobre una propiedad en especifico
        /// </summary>
        /// <param name="objetoBase">Objeto sobre el cual se va a hacer la acción</param>
        /// <param name="propiedad">Nombre de la propiedad a la que se le va a asignar el valor</param>
        /// <param name="value">Valor a establecer</param>
        /// <returns>Retorna el objeto modificado</returns>
        public static object SetValueToProperty(this object objetoBase, string propiedad, object value)
        {
            objetoBase.GetType().GetProperty(propiedad).SetValue(objetoBase, value, null);
            return objetoBase;
        }

        /// <summary>
        /// Obtiene el valor de una propiedad de un objeto
        /// </summary>
        /// <typeparam name="T">Tipo de dato que se va a obtener desde la propiedad</typeparam>
        /// <param name="objetoBase">Objeto sobre el cual se va a hacer la acción</param>
        /// <param name="propiedad">nombre de la propiedad a obtener</param>
        /// <returns>Valor de la propiedad o null en caso de no poder obtenerla</returns>
        public static T GetValueOfProperty<T>(this object objetoBase, string propiedad) where T : class
        {
            return objetoBase.GetType().GetProperty(propiedad).GetValue(objetoBase, null) as T;
        }

        /// <summary>
        /// Obtiene el valor de una propiedad de un objeto, como un object
        /// </summary>
        /// <param name="objetoBase">Objeto sobre el cual se va a hacer la acción</param>
        /// <param name="propiedad">nombre de la propiedad a obtener</param>
        /// <returns>Valor de la propiedad o null en caso de no poder obtenerla</returns>
        public static object GetValueFromProperty(this object objetoBase, string propiedad)
        {
            return objetoBase.GetType().GetProperty(propiedad).GetValue(objetoBase, null);
        }

        /// <summary>
        /// Metodo para copiar un lista de propiedades de un objeto a otro
        /// </summary>
        /// <param name="objetoBase">Objeto Base</param>
        /// <param name="objeto">Objeto Origen</param>
        /// <param name="propiedades">Lista de propiedades</param>
        public static void CopiarPropiedades(this object objetoBase, object objeto, string[] propiedades)
        {
            Type tipoDato = objetoBase.GetType();
            PropertyInfo propiedad;
            foreach (string nombrePropiedad in propiedades)
            {
                propiedad = tipoDato.GetProperty(nombrePropiedad);
                object valor = propiedad.GetValue(objeto, null);
                propiedad.SetValue(objetoBase, valor, null);
            }
        }
        #endregion

        #region Exception
        private const string LOGGER = "Logger";
        private const string METHOD = "Method";
        /// <summary>
        /// campo de formato para retornar la cadena de caracteres de la excepción
        /// </summary>
        private const string FORMAT_EXCEPTION_STRING = "Exception: {0}, StackTrace: {1} InnerException: {2}, InnerExceptionStackTrace: {3}";

        /// <summary>
        /// Permite crear la estructura de una entrada de log para el bloque de aplicación de logging de 
        /// Enterprise Library con base a una excepción
        /// </summary>
        /// <param name="exception">Excepción a la que se le genera entrada de log</param>
        /// <returns>Objeto del tipo LogEntry con la información de la entrada de log</returns>
        public static LogEntry ToLogEntryEntLib(this Exception exception)
        {
            //Se establecen los valores por defecto
            string tipoError = TipoError.Tecnico.ToString();
            int? codigoError = 0;
            string logger = string.Empty;
            string method = string.Empty;
            AppDomain domain = AppDomain.CurrentDomain;
            Process process = Process.GetCurrentProcess();

            if (exception.Data != null)
            {
                if (exception.Data.Contains("TipoError"))
                    tipoError = exception.Data["TipoError"].ToString();

                if (exception.Data.Contains("CodigoError"))
                    codigoError = int.Parse(exception.Data["CodigoError"].ToString());

                if (exception.Data.Contains(LOGGER))
                    logger = exception.Data[LOGGER].ToString();

                if (exception.Data.Contains(METHOD))
                    method = exception.Data[METHOD].ToString();
            }

            LogEntry entry = CreateDefaultLogEntry();

            entry.EventId = codigoError.Value;
            entry.TimeStamp = DateTime.Now;
            entry.MachineName = ObtenerUsuarioId();
            //entry.ProcessName = method;
            entry.ProcessName = (process != null) ? process.ProcessName : method;
            //entry.AppDomainName = logger;
            entry.AppDomainName = (domain != null) ? domain.FriendlyName : logger;
            //Se asigna la prioridad
            entry.Priority = 1;
            //Se establece el título
            entry.Title = tipoError;
            //Se asocia la entrada de log a las categorías correspondientes 
            entry.Categories.Add(LogCategory.Error.GetDescription());
            //Se establece la severidad del mensaje 
            entry.Severity = TraceEventType.Error;
            //Se establece el mensaje de error 
            entry.Message = exception.Message;
            //Se establece mas información sobre el error
            if (!string.IsNullOrEmpty(exception.StackTrace))
                entry.ExtendedProperties.Add(new KeyValuePair<string, object>("StackTrace", exception.StackTrace));

            if (exception.InnerException != null)
            {
                entry.ExtendedProperties.Add(new KeyValuePair<string, object>("InnerException", exception.InnerException.Message));

                if (!string.IsNullOrEmpty(exception.InnerException.StackTrace))
                    entry.ExtendedProperties.Add(new KeyValuePair<string, object>("StackTraceInnerException", exception.InnerException.StackTrace));
            }

            //Retorna entrada de log
            return entry;
        }

        /// <summary>
        /// Permite escribir la excepción en un log
        /// </summary>
        /// <param name="exception">Excepción a registrar</param>
        /// <param name="logger">Objeto que la produce</param>
        /// <param name="action">Método desde donde se genera</param>
        public static void WriteLog(this Exception exception, string logger, string action)
        {
            exception.Data.Add(LOGGER, logger);
            exception.Data.Add(METHOD, action);

            WriteLog(exception.ToLogEntryEntLib());

            if (exception.InnerException != null)
            {
                exception.InnerException.Data.Add(LOGGER, logger);
                exception.InnerException.Data.Add(METHOD, action);
                WriteLog(exception.InnerException.ToLogEntryEntLib());
            }
        }

        /// <summary>
        /// Permite obtener una cadena de caracteres con el mensaje de la excepción completa 
        /// </summary>
        /// <param name="exception">Excepción a convertir</param>
        public static string ToLogString(this Exception exception)
        {
            string innerExceptionMessage = string.Empty;
            string innerExceptionStackTrace = string.Empty;
            string message = exception.Message;
            string stackTrace = (!string.IsNullOrEmpty(exception.StackTrace)) ? exception.StackTrace : string.Empty;

            if (exception.InnerException != null)
            {
                innerExceptionMessage = exception.InnerException.Message;

                if (!string.IsNullOrEmpty(exception.InnerException.StackTrace))
                {
                    innerExceptionStackTrace = exception.InnerException.StackTrace;
                }
            }

            return string.Format(FORMAT_EXCEPTION_STRING, message, stackTrace, innerExceptionMessage, innerExceptionStackTrace);
        }
        #endregion

        #region Enums
        /// <summary>
        /// Retorna el valor del atributo <see cref="T:System.ComponentModel.DescriptionAttribute"/>
        /// aplicado al enumerado.
        /// </summary>
        /// <param name="value">Enumerado del cual se obtiene la descripción.</param>
        /// <returns>En texto con la descripcion del enumerado.</returns>
        public static string GetDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute da = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));
            return da != null ? da.Description : string.Empty;
        }


        /// <summary>
        /// Permite obtener la descripción de acuerdo al valor enviado
        /// </summary>
        /// <param name="value">Valor de la propiedad para obtener la descripción</param>
        /// <returns>En texto con la descripcion del enumerado.</returns>
        public static string GetDescription(this PropertyInfo value)
        {
            object[] obj = value.GetCustomAttributes(typeof(DescriptionAttribute), true);
            DescriptionAttribute da = null;
            if (obj != null && obj.Length > 0)
            {
                da = (DescriptionAttribute)obj[0];
            }
            return da != null ? da.Description : string.Empty;
        }

        /// <summary>
        /// Permite obtener el DbType a partir del tipo de dato
        /// </summary>
        /// <param name="tipoDato">Tipo de dato a obtener</param>
        /// <returns></returns>
        public static DbType GetDbType(this TipoDato tipoDato)
        {
            DbType tipo = DbType.Object;

            switch (tipoDato)
            {
                case TipoDato.String:
                    tipo = DbType.String;
                    break;
                case TipoDato.Decimal:
                    tipo = DbType.Decimal;
                    break;
                case TipoDato.Integer:
                    tipo = DbType.Int32;
                    break;
                case TipoDato.DateTime:
                    tipo = DbType.DateTime;
                    break;
                case TipoDato.DateTimeString:
                    tipo = DbType.String;
                    break;
                case TipoDato.Boolean:
                    tipo = DbType.Boolean;
                    break;
                default:
                    tipo = DbType.Object;
                    break;
            }

            return tipo;
        }
        #endregion

        #region DataTable
        /// <summary>
        /// Convierte los valores de la tabla a una lista
        /// </summary>
        /// <param name="table">Tabla a convertir</param>
        /// <returns>Retorna una lista de cadena de caracteres</returns>
        public static List<string> ToListString(this DataTable table)
        {
            var lista = new List<string>();

            if (table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    lista.Add(row[0].ToString());
                }
            }

            return lista;
        }

        /// <summary>
        /// Retorna un diccionario con los valores de la tabla
        /// </summary>
        /// <param name="table">Tabla a convertir</param>
        /// <returns>Retorna el diccionario con los valores</returns>
        public static Dictionary<string, string> ToDictionaryString(this DataTable table)
        {
            var diccionario = new Dictionary<string, string>();

            if (table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    string key = row[0].ToString();
                    string value = null;

                    if (row[1] != null && row[1] != DBNull.Value && !string.IsNullOrEmpty(row[1].ToString()))
                        value = row[1].ToString();

                    if (!diccionario.ContainsKey(key))
                        diccionario.Add(key, value);
                    else
                        throw new GestorCalculosException("GestorCalculosError_KeyRepetido", key);
                }
            }

            return diccionario;
        }

        /// <summary>
        /// Retorna una lista de datos del tipo semilla a partir de un DataTable
        /// </summary>
        /// <param name="table">Tabla a convertir</param>
        /// <returns>Lista con la información de la semilla</returns>
        public static List<SemillaTO> ToListSemilla(this DataTable table)
        {
            var listaSemillas = new List<SemillaTO>();

            if (table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    string key = row[0].ToString();
                    string value = null;

                    if (row[1] != null && row[1] != DBNull.Value && !string.IsNullOrEmpty(row[1].ToString()))
                        value = row[1].ToString();

                    var semilla = new SemillaTO();
                    semilla.ValorPrincipal = key;
                    semilla.ValorSecundario = value;
                    listaSemillas.Add(semilla);
                }
            }

            return listaSemillas;
        }

        /// <summary>
        /// Retorna una lista de datos del tipo SqlDataRecords a partir de un JArray
        /// </summary>
        /// <param name="values">Lista de valores</param>
        /// <returns>Lista con la información para Table Value Parameters</returns>
        public static IEnumerable<SqlDataRecord> ToListSqlDataRecords(this JArray values)
        {
            var metaData = new[] { new SqlMetaData("item", SqlDbType.VarChar, SqlMetaData.Max) };

            foreach (var val in values)
            {
                var record = new SqlDataRecord(metaData);
                record.SetString(0, (string)val);
                yield return record;
            }
        }

        /// <summary>
        /// Permite obtener el DbType a partir del tipo de dato
        /// </summary>
        /// <param name="tipoDato">Tipo de dato a obtener</param>
        /// <returns></returns>
        public static Type GetSystemType(this TipoDato tipoDato)
        {
            Type tipo = typeof(Object);

            switch (tipoDato)
            {
                case TipoDato.String:
                    tipo = typeof(string);
                    break;
                case TipoDato.Decimal:
                    tipo = typeof(decimal);
                    break;
                case TipoDato.Integer:
                    tipo = typeof(Int32);
                    break;
                case TipoDato.DateTime:
                    tipo = typeof(DateTime);
                    break;
                case TipoDato.DateTimeString:
                    tipo = typeof(string);
                    break;
                case TipoDato.Boolean:
                    tipo = typeof(bool);
                    break;
                default:
                    tipo = typeof(object);
                    break;
            }

            return tipo;
        }

        public static string GetFormatExpression(this TipoDato tipoDato)
        {

            string formato = "'{0}'";

            switch (tipoDato)
            {
                case TipoDato.String:
                    formato = "'{0}'";
                    break;
                case TipoDato.Decimal:
                    formato = "{0}";
                    break;
                case TipoDato.Integer:
                    formato = "{0}";
                    break;
                case TipoDato.DateTime:
                    formato = "#{0}#";
                    break;
                case TipoDato.DateTimeString:
                    formato = "#{0}#";
                    break;
                case TipoDato.Boolean:
                    formato = "{0}";
                    break;
                default:
                    formato = "'{0}'";
                    break;
            }

            return formato;
        }
    #endregion
}
}