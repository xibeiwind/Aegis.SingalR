using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Aegis.Hubs;
using Aegis.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;

namespace Aegis.WebApp.Hubs
{
    public class HeartbeatHub : Hub<IHeartbeatSubscriber>, IHeartbeatHub
    {
        public IHeartbeatService HeartbeatService { get; private set; }
        public HeartbeatHub()
        {
            HeartbeatService = Hubs.HeartbeatService.ServiceInstance;
        }
        public async Task<string> GetCurrentUserName()
        {
            return await Task<string>.Factory.StartNew(() => Context.User.Identity.GetUserName());
        }

        public Task Heartbeat(HeartbeatInfo heartbeat)
        {
            return Task.Factory.StartNew(() => HeartbeatService.Heartbeat(heartbeat));
        }
    }
}