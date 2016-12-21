using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.SignalR.Client;

namespace Aegis.SignalR.Client
{
    public static class HubProxyExtension
    {
        public static void SubscribeOn<THubBase, TSubscriber>(this IHubProxy<THubBase, TSubscriber> hubProxy,
            TSubscriber instance)
            where TSubscriber : class, ISubscriber
            where THubBase : IHubBase
        {
            if (hubProxy.GetType().BaseType != typeof (InterfaceHubProxyBase))
            {
                throw new NotSupportedException("This method can only be called for HubProxies.");
            }

            var theRealHubProxy = hubProxy;

            var interfaceType = typeof (TSubscriber);

            if (!interfaceType.IsInterface)
            {
                throw new NotSupportedException("T is not an interface.");
            }

            var methodInfos = interfaceType.GetMethods();

            foreach (var methodInfo in methodInfos)
            {
                var parameterInfos = methodInfo.GetParameters();

                if (parameterInfos.Count() > 7)
                {
                    throw new NotSupportedException(
                        string.Format(
                            "Only interface methods with less or equal 7 parameters are supported: {0}.{1}({2})!",
                            // ReSharper disable once PossibleNullReferenceException
                            methodInfo.DeclaringType.FullName.Replace("+", "."),
                            methodInfo.Name,
                            string.Join(", ",
                                methodInfo.GetParameters()
                                    .Select(p => string.Format("{0} {1}", p.ParameterType.Name, p.Name)))));
                }

                MethodInfo onMethod;
                Type actionType;

                if (parameterInfos.Any())
                {
                    onMethod =
                        typeof (HubProxyExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
                            .First(
                                m => m.Name.Equals("On") && m.GetGenericArguments().Length == parameterInfos.Length);

                    onMethod = onMethod.MakeGenericMethod(parameterInfos.Select(pi => pi.ParameterType).ToArray());
                    actionType = parameterInfos.Length > 1
                        ? typeof (Action<,>).MakeGenericType(parameterInfos.Select(p => p.ParameterType).ToArray())
                        : typeof (Action<>).MakeGenericType(parameterInfos.Select(p => p.ParameterType).ToArray());
                }
                else
                {
                    onMethod =
                        typeof (HubProxyExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
                            .First(
                                m => m.Name.Equals("On") && m.GetGenericArguments().Length == 0);

                    actionType = typeof (Action);
                }

                var actionDelegate = Delegate.CreateDelegate(actionType, instance, methodInfo);


                onMethod.Invoke(null, new object[] {theRealHubProxy, methodInfo.Name, actionDelegate});
            }
        }
    }
}