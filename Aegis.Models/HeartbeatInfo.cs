using System;

namespace Aegis.Models
{
    public class HeartbeatInfo : AegisEntity
    {
        public string MachineName { get; set; }
        public string OsInfo { get; set; }
        public string CpuInfo { get; set; }
        public string DiskId { get; set; }
        public string MacAddress { get; set; }
        public DateTime Time { get; set; }
        public string ClientName { get; set; }
    }
}