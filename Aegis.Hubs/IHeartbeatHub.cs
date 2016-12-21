using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aegis.Models;
using Aegis.SignalR.Client;

namespace Aegis.Hubs
{
    public interface IHeartbeatHub : IHubBase
    {
        Task<string> GetCurrentUserName();
        Task Heartbeat(HeartbeatInfo heartbeat);
    }
}
