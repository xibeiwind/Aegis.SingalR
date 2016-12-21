using Aegis.SignalR.Client;

namespace Aegis.Hubs
{
    public interface IHeartbeatHubProxy : IHubProxy<IHeartbeatHub, IHeartbeatSubscriber>, IHeartbeatHub
    {

    }
}