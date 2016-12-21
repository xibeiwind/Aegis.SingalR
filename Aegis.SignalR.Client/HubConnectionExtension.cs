using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.CSharp;
using Newtonsoft.Json;

namespace Aegis.SignalR.Client
{
    public static class HubConnectionExtension
    {
        private const string ERR_INACCESSABLE = "\"{0}\" is inaccessible from outside due to its protection level.";

        private static readonly Dictionary<Type, Type> _compiledProxyClasses = new Dictionary<Type, Type>();

        public static TProxy CreateHubProxy<THub, TProxy, TSubscriber>(this HubConnection connection, string hubName)
            where THub : IHubBase
            where TProxy : IHubProxy<THub, TSubscriber>
            where TSubscriber : ISubscriber
        {
            var interfaceType = typeof (THub);
            var proxyInterfaceType = typeof (TProxy);

            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException(string.Format("\"{0}\" is not an interface.", interfaceType.Name));
            }


            if (!interfaceType.IsVisible)
            {
                throw new ConstraintException(string.Format(ERR_INACCESSABLE, interfaceType.FullName.Replace("+", ".")));
            }

            if (_compiledProxyClasses.ContainsKey(typeof (THub)))
            {
                return
                    (TProxy)
                        Activator.CreateInstance(_compiledProxyClasses[typeof (TProxy)],
                            connection.CreateHubProxy(hubName));
            }

            var methodInfos = interfaceType.GetMethods();
            var assembliesToReference = new List<string>
            {
                Assembly.GetExecutingAssembly().Location,
                interfaceType.Assembly.Location,
                typeof (IHubProxy).Assembly.Location,
                typeof (IHubProxy<,>).Assembly.Location,
                typeof (JsonConvert).Assembly.Location,
                typeof (IDynamicMetaObjectProvider).Assembly.Location
            };

            foreach (var methodInfo in methodInfos)
            {
                var parameterInfos = methodInfo.GetParameters();

                if (methodInfo.ReturnType != typeof (Task) && methodInfo.ReturnType.BaseType != typeof (Task))
                {
                    if (methodInfo.DeclaringType == null)
                    {
                        throw new ConstraintException("DeclaringType is null.");
                    }

                    var methodParams = string.Join(", ",
                        parameterInfos.Select(
                            pi => string.Format("{0} {1}", pi.ParameterType.Name, pi.ParameterType.Name)));

                    throw new ConstraintException(
                        string.Format(
                            "The returntype of {0}.{1}({2}) must be System.Threading.Tasks.Task{3}.",
                            methodInfo.DeclaringType.FullName.Replace("+", "."),
                            methodInfo.Name,
                            methodParams,
                            methodInfo.ReturnType == typeof (void)
                                ? string.Empty
                                : string.Format("<{0}>", methodInfo.ReturnType.FullName.Replace("+", "."))));
                }

                if (!methodInfo.ReturnType.IsVisible)
                {
                    throw new ConstraintException(string.Format(ERR_INACCESSABLE,
                        methodInfo.ReturnType.FullName.Replace("+", ".")));
                }

                var noPublicParam = parameterInfos.FirstOrDefault(p => !p.ParameterType.IsVisible);
                if (noPublicParam != null)
                {
                    throw new ConstraintException(string.Format(ERR_INACCESSABLE,
                        noPublicParam.ParameterType.FullName.Replace("+", ".")));
                }

                var assemblies =
                    parameterInfos.Select(p => p.ParameterType.Assembly.Location).Distinct().ToList();
                assemblies.Add(methodInfo.ReturnType.Assembly.Location);

                foreach (var assembly in assemblies)
                {
                    if (!assembliesToReference.Contains(assembly))
                    {
                        assembliesToReference.Add(assembly);
                    }
                }
            }

            var template = new InterfaceHubProxyTemplate
            {
                Interface = interfaceType,
                ProxyInterface = proxyInterfaceType
            };

            var code = template.TransformText();

            var codeProvider = new CSharpCodeProvider();
            var compilerParameters = new CompilerParameters {GenerateInMemory = true, GenerateExecutable = false};

            foreach (var assemblyToReference in assembliesToReference)
            {
                compilerParameters.ReferencedAssemblies.Add(assemblyToReference);
            }

            var results = codeProvider.CompileAssemblyFromSource(compilerParameters, code);

            if (results.Errors.HasErrors)
            {
                throw new Exception("Unknown error occured during proxy generation: " + Environment.NewLine +
                                    string.Join(Environment.NewLine,
                                        results.Errors.OfType<CompilerError>().Select(ce => ce.ToString())));
            }

            var compiledAssembly = results.CompiledAssembly;

            var generatedProxyClassType =
                compiledAssembly.GetType(string.Concat(interfaceType.Namespace, ".", interfaceType.Name, "ProxyImpl"));

            _compiledProxyClasses.Add(interfaceType, generatedProxyClassType);

            return (TProxy) Activator.CreateInstance(generatedProxyClassType, connection.CreateHubProxy(hubName));
        }
    }
}