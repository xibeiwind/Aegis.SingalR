using System;
using System.Configuration;
using System.Management;
using System.Net;
using System.Windows.Forms;
using Aegis.Hubs;
using Aegis.Models;
using Aegis.SignalR.Client;
using Microsoft.AspNet.SignalR.Client;
using Timer = System.Timers.Timer;

namespace Aegis.ClientApp
{
    public partial class MainForm : Form, IHeartbeatSubscriber
    {
        private Timer heartbeatTimer = new Timer();

        public MainForm()
        {
            InitializeComponent();
        }

        public IHeartbeatHubProxy HeartbeatHubProxy { get; private set; }
        public HubConnection Connection { get; set; }

        private void SignalRInitialize(CookieCollection accountCookie)
        {
            var url = ConfigurationManager.AppSettings["SignalRUrl"] + "SignalR";

            Connection = new HubConnection(url) {CookieContainer = new CookieContainer()};
            Connection.Reconnecting += Connection_Reconnecting;
            Connection.Closed += Connection_Closed;
            Connection.CookieContainer.Add(accountCookie);

            HeartbeatHubProxy =
                Connection.CreateHubProxy<IHeartbeatHub, IHeartbeatHubProxy, IHeartbeatSubscriber>("HeartbeatHub");
            HeartbeatHubProxy.SubscribeOn(this);

            heartbeatTimer = new Timer {Interval = 60000};
            heartbeatTimer.Elapsed += (s, e) => UpdateUIInvoke(() =>
            {
                var info = GetHeartBeatInfo();
                HeartbeatHubProxy.Heartbeat(info);
            });
        }


        private HeartbeatInfo GetHeartBeatInfo()
        {
            return new HeartbeatInfo
            {
                MachineName = GetMachineName(),
                OsInfo = GetOsInfo(),
                CpuInfo = GetCpuInfo(),
                DiskId = GetDiskId(),
                MacAddress = GetMacAddress()
            };
        }

        private static string GetCpuInfo()
        {
            var cpuInfo = " ";
            using (var cimobject = new ManagementClass("Win32_Processor"))
            {
                var moc = cimobject.GetInstances();

                foreach (ManagementObject mo in moc)
                {
                    cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                    mo.Dispose();
                }
            }
            return cpuInfo;
        }

        private static string GetOsInfo()
        {
            return Environment.OSVersion.ToString();
        }

        private static string GetMachineName()
        {
            string hostname;
            hostname = Dns.GetHostName();

            return hostname;
        }

        private static string GetDiskId()
        {
            var HDid = " ";
            using (var cimobject1 = new ManagementClass("Win32_DiskDrive"))
            {
                var moc1 = cimobject1.GetInstances();
                foreach (ManagementObject mo in moc1)
                {
                    HDid = (string) mo.Properties["Model"].Value;
                    mo.Dispose();
                }
            }
            return HDid;
        }

        private static string GetMacAddress()
        {
            var MoAddress = " ";
            using (var mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
            {
                var moc2 = mc.GetInstances();
                foreach (ManagementObject mo in moc2)
                {
                    if ((bool) mo["IPEnabled"])
                        MoAddress = mo["MacAddress"].ToString();
                    mo.Dispose();
                }
            }
            return MoAddress;
        }

        private void HeartbeatStart()
        {
            try
            {
                heartbeatTimer.Start();
            }
            catch
            {
            }
        }

        private void Connection_Reconnecting()
        {
            //throw new NotImplementedException();
        }

        private void Connection_Closed()
        {
            //throw new NotImplementedException();
        }

        public void UpdateUIInvoke(Action action)
        {
            if (action != null)
            {
                try
                {
                    if (InvokeRequired)
                    {
                        Invoke(action);
                    }
                    else
                    {
                        action();
                    }
                }
                catch
                {
                }
            }
        }
    }
}