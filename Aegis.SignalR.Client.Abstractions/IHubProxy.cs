using Microsoft.AspNet.SignalR.Client;

namespace Aegis.SignalR.Client
{
    public interface IHubProxy<THub, TSubscriber> : IHubProxy
        where THub : IHubBase
        where TSubscriber : ISubscriber
    {
    }
}