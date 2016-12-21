using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Aegis.WebApp.Hubs
{
    public class ClientHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }
    }
}