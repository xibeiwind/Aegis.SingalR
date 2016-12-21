using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aegis.SignalR.Client
{
    public abstract class InterfaceHubProxyBase : IHubProxy
    {
        protected InterfaceHubProxyBase(IHubProxy proxy)
        {
            Proxy = proxy;
        }

        public IHubProxy Proxy { get; set; }

        public Task Invoke(string method, params object[] args)
        {
            return Proxy.Invoke(method, args);
        }

        public Task<T> Invoke<T>(string method, params object[] args)
        {
            return Proxy.Invoke<T>(method, args);
        }

        public Task Invoke<T>(string method, Action<T> onProgress, params object[] args)
        {
            return Proxy.Invoke(method, onProgress, args);
        }

        public Task<TResult> Invoke<TResult, TProgress>(string method, Action<TProgress> onProgress,
            params object[] args)
        {
            return Proxy.Invoke<TResult, TProgress>(method, onProgress, args);
        }

        public Subscription Subscribe(string eventName)
        {
            return Proxy.Subscribe(eventName);
        }

        public JToken this[string name]
        {
            get { return Proxy[name]; }
            set { Proxy[name] = value; }
        }

        public JsonSerializer JsonSerializer
        {
            get { return Proxy.JsonSerializer; }
        }
    }
}