//Author: Rodolfo Finochietti http://weblogs.shockbyte.com.ar/rodolfof
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Services.Description;
using System.Web.Services.Discovery;
using System.Web.Services.Protocols;
using System.Xml.Schema;
using Microsoft.CSharp;

namespace MVM.ProcessEngine.Common.Helpers
{
    /// <summary>
    /// Clase utilizada para generar proxys dinámicos con base en el WSDL
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WebServiceProxyFactory<T> where T : class
    {
        #region Constants

        private const string ProxyNameSpace = "WebServiceProxyFactory";
        private const string ListFullClassName = "System.Collections.Generic.List<{0}>";

        #endregion

        #region Private Vars

        private Uri _wsdlLocation;

        private DiscoveryProtocol _protocolName = DiscoveryProtocol.Soap;

        private Type _serviceContract;

        #endregion

        #region Constructors

        public WebServiceProxyFactory(string wsdlLocation) : this(new Uri(wsdlLocation), DiscoveryProtocol.Soap) { }

        public WebServiceProxyFactory(string wsdlLocation, DiscoveryProtocol protocolName) : this(new Uri(wsdlLocation), protocolName) { }

        public WebServiceProxyFactory(Uri wsdlLocation) : this(wsdlLocation, DiscoveryProtocol.Soap) { }

        public WebServiceProxyFactory(Uri wsdlLocation, DiscoveryProtocol protocolName)
        {
            _wsdlLocation = wsdlLocation;
            _protocolName = protocolName;
            _serviceContract = typeof(T);
        }

        #endregion

        #region Properties

        public bool UseGenericList
        {
            get;
            set;
        }

        #endregion

        #region Public Methods

        public T Build()
        {
            if (_wsdlLocation == null)
                throw new ArgumentNullException("WsdlLocation", "The argument 'WsdlLocation' can't be null");

            ServiceDescriptionImporter importer = new ServiceDescriptionImporter();
            importer.ProtocolName = _protocolName.ToString();

            //Get WSDL
            DiscoverService(importer);

            //Proxy source code generation
            Assembly assembly;

            using (CSharpCodeProvider codeProvider = new CSharpCodeProvider())
            {
                assembly = GenerateProxyAssembly(codeProvider, GenerateProxySourceCode(importer, codeProvider));
            }

            //Clean up resources
            importer = null;

            return Activator.CreateInstance(assembly.GetTypes()[0]) as T;
        }

        #endregion

        #region Private Methods

        private void DiscoverService(ServiceDescriptionImporter importer)
        {
            using (DiscoveryClientProtocol discoClient = new DiscoveryClientProtocol())
            {
                discoClient.DiscoverAny(_wsdlLocation.ToString());
                discoClient.ResolveAll();

                foreach (object value in discoClient.Documents.Values)
                {
                    if (value is ServiceDescription)
                        importer.AddServiceDescription((ServiceDescription)value, null, null);

                    if (value is XmlSchema)
                        importer.Schemas.Add((XmlSchema)value);
                }
            }
        }

        private string GenerateProxySourceCode(ServiceDescriptionImporter importer, CSharpCodeProvider codeProvider)
        {
            CodeNamespace mainNamespace = new CodeNamespace(ProxyNameSpace);

            importer.Import(mainNamespace, null);

            if (UseGenericList)
                ChangeArrayToGenericList(mainNamespace);

            AddImports(mainNamespace);

            CodeTypeDeclaration[] temp = new CodeTypeDeclaration[mainNamespace.Types.Count];
            mainNamespace.Types.CopyTo(temp, 0);
            foreach (CodeTypeDeclaration declarationType in temp)
            {
                if ((declarationType.BaseTypes.Count > 0) && (declarationType.BaseTypes[0].BaseType == typeof(SoapHttpClientProtocol).FullName))
                    declarationType.BaseTypes.Add(_serviceContract);
                else
                    mainNamespace.Types.Remove(declarationType);
            }

            //Clean up resources
            temp = null;

            //Proxy source code generation
            StringBuilder sourceCode = new StringBuilder();
            string ret;

            using (StringWriter sw = new StringWriter(sourceCode, CultureInfo.CurrentCulture))
            {
                codeProvider.GenerateCodeFromNamespace(mainNamespace, sw, null);
                ret = sourceCode.ToString();
                sw.Close();
            }

            //Clean up resources
            mainNamespace = null;
            sourceCode = null;

            return ret;
        }

        private void ChangeArrayToGenericList(CodeNamespace codeNamespace)
        {
            CodeTypeDeclaration[] temp = new CodeTypeDeclaration[codeNamespace.Types.Count];
            codeNamespace.Types.CopyTo(temp, 0);
            foreach (CodeTypeDeclaration type in temp)
            {
                foreach (CodeTypeMember member in type.Members)
                {
                    CodeMemberMethod methodMember = member as CodeMemberMethod;
                    if (methodMember != null)
                    {
                        foreach (CodeParameterDeclarationExpression paremeter in methodMember.Parameters)
                        {
                            if (paremeter.Type.ArrayElementType != null)
                                paremeter.Type = new CodeTypeReference(String.Format(CultureInfo.InvariantCulture, ListFullClassName, paremeter.Type.ArrayElementType.BaseType));
                        }

                        if (methodMember.ReturnType.ArrayElementType != null)
                        {
                            string listGenericType = String.Format(CultureInfo.InvariantCulture, ListFullClassName, methodMember.ReturnType.ArrayElementType.BaseType);

                            methodMember.ReturnType = new CodeTypeReference(listGenericType);

                            CodeMethodReturnStatement returnStatement = null;
                            foreach (CodeStatement code in methodMember.Statements)
                            {
                                returnStatement = code as CodeMethodReturnStatement;
                                if (returnStatement != null)
                                    break;
                            }

                            if (returnStatement != null)
                            {
                                methodMember.Statements.Remove(returnStatement);
                                methodMember.Statements.Add(new CodeMethodReturnStatement(new CodeArgumentReferenceExpression(String.Format(CultureInfo.InvariantCulture, "({0})results[0]", listGenericType))));
                            }
                        }
                    }
                }
            }
        }

        private void AddImports(CodeNamespace codeNamespace)
        {
            MethodInfo[] methods = _serviceContract.GetMethods();
            foreach (MethodInfo method in methods)
            {
                AddImport(codeNamespace, method.ReturnType);

                foreach (ParameterInfo parameter in method.GetParameters())
                    AddImport(codeNamespace, parameter.ParameterType);
            }
        }

        private void AddImport(CodeNamespace codeNamespace, Type type)
        {
            if (!type.IsPrimitive)
            {
                codeNamespace.Imports.Add(new CodeNamespaceImport(type.Namespace));

                if (type.IsGenericType)
                {
                    Type[] genericArgs = type.GetGenericArguments();

                    foreach (Type genericArg in genericArgs)
                        AddImport(codeNamespace, genericArg);
                }
            }
        }

        private Assembly GenerateProxyAssembly(CSharpCodeProvider csharpCodeProvider, string proxyCode)
        {
            //Assembly compilation
            string location = String.Empty;

            if (HttpContext.Current != null)
            {
                location = HttpContext.Current.Server.MapPath(".");
                location += @"\bin\";
            }

            CompilerParameters parameters = new CompilerParameters();

            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Data.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.dll");
            parameters.ReferencedAssemblies.Add("System.Web.dll");
            parameters.ReferencedAssemblies.Add("System.Web.Services.dll");
            parameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
            //parameters.ReferencedAssemblies.Add(typeof(IConfiguracionService).Assembly.CodeBase);
            //GetReferencedAssemblies(_serviceContract.Assembly, parameters);

            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = false;
            parameters.IncludeDebugInformation = false;
            parameters.TempFiles = new TempFileCollection(Path.GetTempPath());

            CompilerResults cr = csharpCodeProvider.CompileAssemblyFromSource(parameters, proxyCode);

            if (cr.Errors.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < cr.Errors.Count; i++)
                {
                    sb.Append(Environment.NewLine);
                    sb.Append("'");
                    sb.Append(String.Format(CultureInfo.InvariantCulture, "{0} - {1}", cr.Errors[i].ErrorNumber, cr.Errors[i].ErrorText));
                    sb.Append("'");
                }

                throw new ApplicationException(String.Format(CultureInfo.InvariantCulture, "Building web service proxy has failed with {0} errors. Errors:{1}", cr.Errors.Count, sb.ToString()));
            }

            Assembly ret = cr.CompiledAssembly;

            //Clean up resources
            cr = null;
            parameters = null;

            return ret;
        }

        private static void GetReferencedAssemblies(Assembly assembly, CompilerParameters parameters)
        {
            if (!parameters.ReferencedAssemblies.Contains(assembly.Location))
            {
                string location = Path.GetFileName(assembly.Location);
                if (!parameters.ReferencedAssemblies.Contains(location))
                {
                    parameters.ReferencedAssemblies.Add(assembly.Location);
                    foreach (AssemblyName referencedAssembly in assembly.GetReferencedAssemblies())
                        GetReferencedAssemblies(Assembly.Load(referencedAssembly.FullName), parameters);
                }
            }
        }

        #endregion
    }

    public enum DiscoveryProtocol
    {
        Soap,
        Soap12,
        HttpGet,
        HttpPost,
        HttpSoap
    }
}