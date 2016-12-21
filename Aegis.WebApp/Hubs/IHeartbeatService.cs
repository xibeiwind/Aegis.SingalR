using System;
using Aegis.Models;

namespace Aegis.WebApp.Hubs
{
    public interface IHeartbeatService
    {
        void Heartbeat(HeartbeatInfo heartbeat);
    }
}