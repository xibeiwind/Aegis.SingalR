using System.Diagnostics;
using Aegis.Models;
using Newtonsoft.Json;

namespace Aegis.WebApp.Hubs
{
    public class HeartbeatService : IHeartbeatService
    {
        public static IHeartbeatService ServiceInstance { get; private set; }

        static HeartbeatService()
        {
            ServiceInstance = new HeartbeatService();
        }
        private HeartbeatService()
        {
            
        }

        public void Heartbeat(HeartbeatInfo heartbeat)
        {
            try
            {
                Trace.TraceInformation(JsonConvert.SerializeObject(heartbeat));
            }
            catch
            {

            }
        }
    }
}