using SharpPcap;
using SharpPcap.LibPcap;
using PacketDotNet;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;
using System.Net.NetworkInformation;


namespace sw_L2switch_psip

{
    
    public partial class sw : Form
    {
        private int eth_1_in = 0;
        private int eth_1_out = 0;

        private int ip_1_in = 0;
        private int ip_1_out = 0;

        private int arp_1_in = 0;
        private int arp_1_out = 0;

        private int tcp_1_in = 0;
        private int tcp_1_out = 0;

        private int udp_1_in = 0;
        private int udp_1_out = 0;

        private int icmp_1_in = 0;
        private int icmp_1_out = 0;

        private int http_1_in = 0;
        private int http_1_out = 0;

        private int https_1_in = 0;
        private int https_1_out = 0;

        private int total_1_in = 0;
        private int total_1_out = 0;



        private int eth_2_in = 0;
        private int eth_2_out = 0;

        private int ip_2_in = 0;
        private int ip_2_out = 0;

        private int arp_2_in = 0;
        private int arp_2_out = 0;

        private int tcp_2_in = 0;
        private int tcp_2_out = 0;

        private int udp_2_in = 0;
        private int udp_2_out = 0;

        private int icmp_2_in = 0;
        private int icmp_2_out = 0;

        private int http_2_in = 0;
        private int http_2_out = 0;

        private int https_2_in = 0;
        private int https_2_out = 0;

        private int total_2_in = 0;
        private int total_2_out = 0;
        
        
        public sw()
        {
            InitializeComponent();
            Start.Enabled = false;
            Stop.Enabled = false;
            eth_if1_in.Text = eth_1_in.ToString();
            ip_if1_in.Text = ip_1_in.ToString();
            arp_if1_in.Text = arp_1_in.ToString();
            tcp_if1_in.Text = tcp_1_in.ToString();
            udp_if1_in.Text = udp_1_in.ToString();
            icmp_if1_in.Text = icmp_1_in.ToString();
            http_if1_in.Text = http_1_in.ToString();
            https_if1_in.Text = https_1_in.ToString();
            total_if1_in.Text = total_1_in.ToString();

            eth_if2_in.Text = eth_1_in.ToString();
            ip_if2_in.Text = ip_1_in.ToString();
            arp_if2_in.Text = arp_1_in.ToString();
            tcp_if2_in.Text = tcp_1_in.ToString();
            udp_if2_in.Text = udp_1_in.ToString();
            icmp_if2_in.Text = icmp_1_in.ToString();
            http_if2_in.Text = http_1_in.ToString();
            https_if2_in.Text = https_1_in.ToString();
            total_if2_in.Text = total_1_in.ToString();

            eth_if1_out.Text = eth_1_out.ToString();
            ip_if1_out.Text = ip_1_out.ToString();
            arp_if1_out.Text = arp_1_out.ToString();
            tcp_if1_out.Text = tcp_1_out.ToString();
            udp_if1_out.Text = udp_1_out.ToString();
            icmp_if1_out.Text = icmp_1_out.ToString();
            http_if1_out.Text = http_1_out.ToString();
            https_if1_out.Text = https_1_out.ToString();
            total_if1_out.Text = total_1_out.ToString();

            eth_if2_out.Text = eth_2_out.ToString();
            ip_if2_out.Text = ip_2_out.ToString();
            arp_if2_out.Text = arp_2_out.ToString();
            tcp_if2_out.Text = tcp_2_out.ToString();
            udp_if2_out.Text = udp_2_out.ToString();
            icmp_if2_out.Text = icmp_2_out.ToString();
            http_if2_out.Text = http_2_out.ToString();
            https_if2_out.Text = https_2_out.ToString();
            total_if2_out.Text = total_2_out.ToString();
            Refresh_start();

    }
        public static bool syslog_enabled = false;
        public static Syslog syslog;
        

        public static bool Syslog_enabled
        {
            get { return syslog_enabled; }
            set { syslog_enabled = value; }

        }
        public static Syslog Syslog
        {
            get { return syslog; }
            set { syslog = value; }

        }




        public static LibPcapLiveDevice device;
        public static LibPcapLiveDevice device2;
        public static LibPcapLiveDevice Device
        {
            get { return device; }

        }

        public static LibPcapLiveDevice Device2
        {
            get { return device2; }

        }

        MacAddressTable macTable = new MacAddressTable();
        List<ACL> aclFilters = new List<ACL>();
        bool implicit_allow = false;

        Thread t;
        Thread cableMonitor;
        private void Start_Click(object sender, EventArgs e)
        {
            if(int1_box.SelectedIndex == -1 || int2_box.SelectedIndex == -1)
            {
                MessageBox.Show("Both interfaces must have device chosen.");
                return;
            }
            if (int1_box.SelectedIndex == int2_box.SelectedIndex)
            {
                MessageBox.Show("Can't choose the same device on both interfaces.");
                return;
            }
            Stop.Enabled = true;
            Start.Enabled = false;
            Refresh.Enabled = false;
            int1_box.Enabled = false;
            int2_box.Enabled = false;
            t = new Thread(new ThreadStart(refreshMacTable));
            t.Start();
            try
            {
                var selectedInterface = int1_box.SelectedItem.ToString();
                device = LibPcapLiveDeviceList.Instance.FirstOrDefault(x => x.Description == selectedInterface);
                device.Open(DeviceModes.Promiscuous | DeviceModes.NoCaptureLocal | DeviceModes.MaxResponsiveness);
                device.OnPacketArrival += Device_OnPacketArrival;
                var dev1Thread = new Thread(() =>
                {
                    try
                    {
                        device.StartCapture();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                });
                dev1Thread.Start();

                var selectedInterface2 = int2_box.SelectedItem.ToString();
                device2 = LibPcapLiveDeviceList.Instance.FirstOrDefault(x => x.Description == selectedInterface2);
                device2.Open(DeviceModes.Promiscuous | DeviceModes.NoCaptureLocal | DeviceModes.MaxResponsiveness);
                device2.OnPacketArrival += Device2_OnPacketArrival;
                var dev2Thread = new Thread(() =>
                {
                    try
                    {
                        device2.StartCapture();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                });
                dev2Thread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            cableMonitor = new Thread(() => MonitorCableStatusAndSwap(device.Description, device2.Description,device.MacAddress, device2.MacAddress));
            cableMonitor.Start();
        }


        public void MonitorCableStatusAndSwap(string device1, string device2, PhysicalAddress device1MAC, PhysicalAddress device2MAC)
        {
            bool device1Connected = true;
            bool device2Connected = true;
            PhysicalAddress device1Address = device1MAC;
            PhysicalAddress device2Address = device2MAC;

            while (true)
            {
                Thread.Sleep(1000);

                bool device1Up = NetworkInterface.GetIsNetworkAvailable() && NetworkInterface.GetAllNetworkInterfaces().Any(n => n.Description == device1 && n.OperationalStatus == OperationalStatus.Up);
                bool device2Up = NetworkInterface.GetIsNetworkAvailable() && NetworkInterface.GetAllNetworkInterfaces().Any(n => n.Description == device2 && n.OperationalStatus == OperationalStatus.Up);

                if (device1Up != device1Connected || device2Up != device2Connected)
                {
                    if (device1Up && !device1Connected)
                    {
                        MessageBox.Show($"Cable connected to {device1}.");
                        if (syslog_enabled)
                        {
                            syslog.SendSyslogMessage($"Cable connected to {device1}.");
                        }
                        device1Connected = true;
                        device1Address = device1MAC;
                    }
                    else if (device2Up && !device2Connected)
                    {
                        MessageBox.Show($"Cable connected to {device2}.");
                        if (syslog_enabled)
                        {
                            syslog.SendSyslogMessage($"Cable connected to {device2}.");
                        }
                        device2Connected = true;
                        device2Address = device2MAC;
                    }
                    else if (!device1Up && device1Connected)
                    {
                        MessageBox.Show($"Cable disconnected from {device1}.");
                        if (syslog_enabled)
                        {
                            syslog.SendSyslogMessage($"Cable disconnected from {device1}.");
                        }
                        device1Connected = false;
                    }
                    else if (!device2Up && device2Connected)
                    {
                        MessageBox.Show($"Cable disconnected from {device2}.");
                        if (syslog_enabled)
                        {
                            syslog.SendSyslogMessage($"Cable disconnected from {device2}.");
                        }
                        device2Connected = false;
                    }

                    if (device1Connected && device2Connected)
                    {
                        PhysicalAddress currentDevice1Address = device1MAC;
                        PhysicalAddress currentDevice2Address = device2MAC;

                        if (currentDevice1Address.Equals(device2Address) && currentDevice2Address.Equals(device1Address))
                        {
                            MessageBox.Show($"Cables swapped between {device1} and {device2}.");
                            if (syslog_enabled)
                            {
                                syslog.SendSyslogMessage($"Cables swapped between {device1} and {device2}.");
                            }
                        }
                    }
                }
            }
        }


        private void Device_OnPacketArrival(object sender, PacketCapture e)
        {
            var raw = e.GetPacket();
            ReceivedPacket receivedPacket = new ReceivedPacket(raw.Data);

            if (implicit_allow)
            {
                if (aclFilters.Any(acl =>
                    acl.Interface == "1" &&
                    acl.Direction == "IN" &&
                    acl.Allow == true &&
                    (string.IsNullOrEmpty(acl.SourceMAC) || acl.SourceMAC == receivedPacket.SourceMAC) &&
                    (string.IsNullOrEmpty(acl.DestinationMAC) || acl.DestinationMAC == receivedPacket.DestinationMAC) &&
                    (string.IsNullOrEmpty(acl.Protocol) || acl.Protocol == "Any" || acl.Protocol == receivedPacket.Protocol) &&
                    (string.IsNullOrEmpty(acl.SourceIP) || acl.SourceIP == receivedPacket.SourceIP.ToString()) &&
                    (string.IsNullOrEmpty(acl.DestinationIP) || acl.DestinationIP == receivedPacket.DestinationIP.ToString())))
                {
                    
                }
                else { return; }
            }
            else
            {
                if (aclFilters.Any(acl =>
                    acl.Interface == "1" &&
                    acl.Direction == "IN" &&
                    acl.Deny == true &&
                    (string.IsNullOrEmpty(acl.SourceMAC) || acl.SourceMAC == receivedPacket.SourceMAC) &&
                    (string.IsNullOrEmpty(acl.DestinationMAC) || acl.DestinationMAC == receivedPacket.DestinationMAC) &&
                    (string.IsNullOrEmpty(acl.Protocol) || acl.Protocol == "Any" || acl.Protocol == receivedPacket.Protocol) &&
                    (string.IsNullOrEmpty(acl.SourceIP) || acl.SourceIP == receivedPacket.SourceIP.ToString()) &&
                    (string.IsNullOrEmpty(acl.DestinationIP) || acl.DestinationIP == receivedPacket.DestinationIP.ToString())))
                {
                    return;
                }
            }


            total_1_in++;
            eth_1_in++;
            switch (receivedPacket.Protocol)
            {
                case "TCP": 
                    tcp_1_in++;
                    ip_1_in++;
                    break;
                case "HTTP":
                    tcp_1_in++;
                    http_1_in++;
                    ip_1_in++;
                    break;
                case "HTTPS":
                    tcp_1_in++;
                    https_1_in++;
                    ip_1_in++;
                    break;
                case "UDP": 
                    udp_1_in++;
                    ip_1_in++;
                    break;
                case "ICMP":
                    icmp_1_in++;
                    ip_1_in++;
                    break;
                case "ARP":
                    arp_1_in++;
                    break;
                default:
                    break;
            }
            update_1_in();

            macTable.AddEntry(receivedPacket.SourceMAC, device.Description, get_time());
            updateMacTable();

            if (implicit_allow)
            {
                if (aclFilters.Any(acl =>
                    acl.Interface == "1" &&
                    acl.Direction == "OUT" &&
                    acl.Allow == true &&
                    (string.IsNullOrEmpty(acl.SourceMAC) || acl.SourceMAC == receivedPacket.SourceMAC) &&
                    (string.IsNullOrEmpty(acl.DestinationMAC) || acl.DestinationMAC == receivedPacket.DestinationMAC) &&
                    (string.IsNullOrEmpty(acl.Protocol) || acl.Protocol == "Any" || acl.Protocol == receivedPacket.Protocol) &&
                    (string.IsNullOrEmpty(acl.SourceIP) || acl.SourceIP == receivedPacket.SourceIP.ToString()) &&
                    (string.IsNullOrEmpty(acl.DestinationIP) || acl.DestinationIP == receivedPacket.DestinationIP.ToString())))
                {

                }
                else { return; }
            }
            else
            {
                if (aclFilters.Any(acl =>
                    acl.Interface == "1" &&
                    acl.Direction == "OUT" &&
                    acl.Deny == true &&
                    (string.IsNullOrEmpty(acl.SourceMAC) || acl.SourceMAC == receivedPacket.SourceMAC) &&
                    (string.IsNullOrEmpty(acl.DestinationMAC) || acl.DestinationMAC == receivedPacket.DestinationMAC) &&
                    (string.IsNullOrEmpty(acl.Protocol) || acl.Protocol == "Any" || acl.Protocol == receivedPacket.Protocol) &&
                    (string.IsNullOrEmpty(acl.SourceIP) || acl.SourceIP == receivedPacket.SourceIP.ToString()) &&
                    (string.IsNullOrEmpty(acl.DestinationIP) || acl.DestinationIP == receivedPacket.DestinationIP.ToString())))
                {
                    return;
                }
            }

            total_1_out++;
            eth_1_out++;
            switch (receivedPacket.Protocol)
            {
                case "TCP":
                    tcp_1_out++;
                    ip_1_out++;
                    break;
                case "HTTP":
                    tcp_1_out++;
                    http_1_out++;
                    ip_1_out++;
                    break;
                case "HTTPS":
                    tcp_1_out++;
                    https_1_out++;
                    ip_1_out++;
                    break;
                case "UDP":
                    udp_1_out++;
                    ip_1_out++;
                    break;
                case "ICMP":
                    icmp_1_out++;
                    ip_1_out++;
                    break;
                case "ARP":
                    arp_1_out++;
                    break;
                default:
                    break;
            }
            try {device2.SendPacket(raw.Data);}
            catch (Exception ex) {/*MessageBox.Show(ex.ToString());*/}
            update_1_out();
        }


        private void update_1_in()
        {
            if (eth_if1_in.InvokeRequired)
            {
                eth_if1_in.Invoke((MethodInvoker)(() => update_1_in()));
                eth_if1_in.Invoke((MethodInvoker)(() => update_2_out()));
            }
            if (ip_if1_in.InvokeRequired)
            {
                ip_if1_in.Invoke((MethodInvoker)(() => update_1_in()));
                ip_if1_in.Invoke((MethodInvoker)(() => update_2_out()));
            }
            if (arp_if1_in.InvokeRequired)
            {
                arp_if1_in.Invoke((MethodInvoker)(() => update_1_in()));
                arp_if1_in.Invoke((MethodInvoker)(() => update_2_out()));
            }
            if (tcp_if1_in.InvokeRequired)
            {
                tcp_if1_in.Invoke((MethodInvoker)(() => update_1_in()));
                tcp_if1_in.Invoke((MethodInvoker)(() => update_2_out()));
            }
            if (udp_if1_in.InvokeRequired)
            {
                udp_if1_in.Invoke((MethodInvoker)(() => update_1_in()));
                udp_if1_in.Invoke((MethodInvoker)(() => update_2_out()));
            }
            if (icmp_if1_in.InvokeRequired)
            {
                icmp_if1_in.Invoke((MethodInvoker)(() => update_1_in()));
                icmp_if1_in.Invoke((MethodInvoker)(() => update_2_out()));
            }
            if (http_if1_in.InvokeRequired)
            {
                http_if1_in.Invoke((MethodInvoker)(() => update_1_in()));
                http_if1_in.Invoke((MethodInvoker)(() => update_2_out()));
            }
            if (https_if1_in.InvokeRequired)
            {
                https_if1_in.Invoke((MethodInvoker)(() => update_1_in()));
                https_if1_in.Invoke((MethodInvoker)(() => update_2_out()));
            }
            if (total_if1_in.InvokeRequired)
            {
                total_if1_in.Invoke((MethodInvoker)(() => update_1_in()));
                total_if1_in.Invoke((MethodInvoker)(() => update_2_out()));
            }
            eth_if1_in.Text = eth_1_in.ToString();
            ip_if1_in.Text = ip_1_in.ToString();
            arp_if1_in.Text = arp_1_in.ToString();
            tcp_if1_in.Text = tcp_1_in.ToString();
            udp_if1_in.Text = udp_1_in.ToString();
            icmp_if1_in.Text = icmp_1_in.ToString();
            http_if1_in.Text = http_1_in.ToString();
            https_if1_in.Text = https_1_in.ToString();
            total_if1_in.Text = total_1_in.ToString();
        }

        private void update_1_out()
        {
            if (eth_if1_out.InvokeRequired)
            {
                eth_if1_out.Invoke((MethodInvoker)(() => update_1_out()));
            }
            if (ip_if1_out.InvokeRequired)
            {
                ip_if1_out.Invoke((MethodInvoker)(() => update_1_out()));
            }
            if (arp_if1_out.InvokeRequired)
            {
                arp_if1_out.Invoke((MethodInvoker)(() => update_1_out()));
            }
            if (tcp_if1_out.InvokeRequired)
            {
                tcp_if1_out.Invoke((MethodInvoker)(() => update_1_out()));
            }
            if (udp_if1_out.InvokeRequired)
            {
                udp_if1_out.Invoke((MethodInvoker)(() => update_1_out()));
            }
            if (icmp_if1_out.InvokeRequired)
            {
                icmp_if1_out.Invoke((MethodInvoker)(() => update_1_out()));
            }
            if (http_if1_out.InvokeRequired)
            {
                http_if1_out.Invoke((MethodInvoker)(() => update_1_out()));
            }
            if (https_if1_out.InvokeRequired)
            {
                https_if1_out.Invoke((MethodInvoker)(() => update_1_out()));
            }
            if (total_if1_out.InvokeRequired)
            {
                total_if1_out.Invoke((MethodInvoker)(() => update_1_out()));
            }
            eth_if1_out.Text = eth_1_out.ToString();
            ip_if1_out.Text = ip_1_out.ToString();
            arp_if1_out.Text = arp_1_out.ToString();
            tcp_if1_out.Text = tcp_1_out.ToString();
            udp_if1_out.Text = udp_1_out.ToString();
            icmp_if1_out.Text = icmp_1_out.ToString();
            http_if1_out.Text = http_1_out.ToString();
            https_if1_out.Text = https_1_out.ToString();
            total_if1_out.Text = total_1_out.ToString();
        }


        private void Device2_OnPacketArrival(object sender, PacketCapture e)
        {
            var raw = e.GetPacket();
            ReceivedPacket receivedPacket = new ReceivedPacket(raw.Data);

            if (implicit_allow)
            {
                if (aclFilters.Any(acl =>
                    acl.Interface == "2" &&
                    acl.Direction == "IN" &&
                    acl.Allow == true &&
                    (string.IsNullOrEmpty(acl.SourceMAC) || acl.SourceMAC == receivedPacket.SourceMAC) &&
                    (string.IsNullOrEmpty(acl.DestinationMAC) || acl.DestinationMAC == receivedPacket.DestinationMAC) &&
                    (string.IsNullOrEmpty(acl.Protocol) || acl.Protocol == "Any" || acl.Protocol == receivedPacket.Protocol) &&
                    (string.IsNullOrEmpty(acl.SourceIP) || acl.SourceIP == receivedPacket.SourceIP.ToString()) &&
                    (string.IsNullOrEmpty(acl.DestinationIP) || acl.DestinationIP == receivedPacket.DestinationIP.ToString())))
                {

                }
                else { return; }
            }
            else
            {
                if (aclFilters.Any(acl =>
                    acl.Interface == "2" &&
                    acl.Direction == "IN" &&
                    acl.Deny == true &&
                    (string.IsNullOrEmpty(acl.SourceMAC) || acl.SourceMAC == receivedPacket.SourceMAC) &&
                    (string.IsNullOrEmpty(acl.DestinationMAC) || acl.DestinationMAC == receivedPacket.DestinationMAC) &&
                    (string.IsNullOrEmpty(acl.Protocol) || acl.Protocol == "Any" || acl.Protocol == receivedPacket.Protocol) &&
                    (string.IsNullOrEmpty(acl.SourceIP) || acl.SourceIP == receivedPacket.SourceIP.ToString()) &&
                    (string.IsNullOrEmpty(acl.DestinationIP) || acl.DestinationIP == receivedPacket.DestinationIP.ToString())))
                {
                    return;
                }
            }


            total_2_in++;
            eth_2_in++;
            switch (receivedPacket.Protocol)
            {
                case "TCP":
                    tcp_2_in++;
                    ip_2_in++;
                    break;
                case "HTTP":
                    tcp_2_in++;
                    http_2_in++;
                    ip_2_in++;
                    break;
                case "HTTPS":
                    tcp_2_in++;
                    https_2_in++;
                    ip_2_in++;
                    break;
                case "UDP":
                    udp_2_in++;
                    ip_2_in++;
                    break;
                case "ICMP":
                    icmp_2_in++;
                    ip_2_in++;
                    break;
                case "ARP":
                    arp_2_in++;
                    break;
                default:
                    break;
            }
            update_2_in();

            macTable.AddEntry(receivedPacket.SourceMAC, device2.Description, get_time());
            updateMacTable();

            if (implicit_allow)
            {
                if (aclFilters.Any(acl =>
                    acl.Interface == "2" &&
                    acl.Direction == "OUT" &&
                    acl.Allow == true &&
                    (string.IsNullOrEmpty(acl.SourceMAC) || acl.SourceMAC == receivedPacket.SourceMAC) &&
                    (string.IsNullOrEmpty(acl.DestinationMAC) || acl.DestinationMAC == receivedPacket.DestinationMAC) &&
                    (string.IsNullOrEmpty(acl.Protocol) || acl.Protocol == "Any" || acl.Protocol == receivedPacket.Protocol) &&
                    (string.IsNullOrEmpty(acl.SourceIP) || acl.SourceIP == receivedPacket.SourceIP.ToString()) &&
                    (string.IsNullOrEmpty(acl.DestinationIP) || acl.DestinationIP == receivedPacket.DestinationIP.ToString())))
                {

                }
                else { return; }
            }
            else
            {
                if (aclFilters.Any(acl =>
                    acl.Interface == "2" &&
                    acl.Direction == "OUT" &&
                    acl.Deny == true &&
                    (string.IsNullOrEmpty(acl.SourceMAC) || acl.SourceMAC == receivedPacket.SourceMAC) &&
                    (string.IsNullOrEmpty(acl.DestinationMAC) || acl.DestinationMAC == receivedPacket.DestinationMAC) &&
                    (string.IsNullOrEmpty(acl.Protocol) || acl.Protocol == "Any" || acl.Protocol == receivedPacket.Protocol) &&
                    (string.IsNullOrEmpty(acl.SourceIP) || acl.SourceIP == receivedPacket.SourceIP.ToString()) &&
                    (string.IsNullOrEmpty(acl.DestinationIP) || acl.DestinationIP == receivedPacket.DestinationIP.ToString())))
                {
                    return;
                }
            }

            total_2_out++;
            eth_2_out++;
            switch (receivedPacket.Protocol)
            {
                case "TCP":
                    tcp_2_out++;
                    ip_2_out++;
                    break;
                case "HTTP":
                    tcp_2_out++;
                    http_2_out++;
                    ip_2_out++;
                    break;
                case "HTTPS":
                    tcp_2_out++;
                    https_2_out++;
                    ip_2_out++;
                    break;
                case "UDP":
                    udp_2_out++;
                    ip_2_out++;
                    break;
                case "ICMP":
                    icmp_2_out++;
                    ip_2_out++;
                    break;
                case "ARP":
                    arp_2_out++;
                    break;
                default:
                    break;
            }
            try {device.SendPacket(raw.Data);}
            catch (Exception ex) {/*MessageBox.Show(ex.ToString());*/}
            update_2_out();

        }

        private void update_2_in()
        {
            if (eth_if2_in.InvokeRequired)
            {
                eth_if2_in.Invoke((MethodInvoker)(() => update_2_in()));
            }
            if (ip_if2_in.InvokeRequired)
            {
                ip_if2_in.Invoke((MethodInvoker)(() => update_2_in()));
            }
            if (arp_if2_in.InvokeRequired)
            {
                arp_if2_in.Invoke((MethodInvoker)(() => update_2_in()));
            }
            if (tcp_if2_in.InvokeRequired)
            {
                tcp_if2_in.Invoke((MethodInvoker)(() => update_2_in()));
            }
            if (udp_if2_in.InvokeRequired)
            {
                udp_if2_in.Invoke((MethodInvoker)(() => update_2_in()));
            }
            if (icmp_if2_in.InvokeRequired)
            {
                icmp_if2_in.Invoke((MethodInvoker)(() => update_2_in()));
            }
            if (http_if2_in.InvokeRequired)
            {
                http_if2_in.Invoke((MethodInvoker)(() => update_2_in()));
            }
            if (https_if2_in.InvokeRequired)
            {
                https_if2_in.Invoke((MethodInvoker)(() => update_2_in()));
            }
            if (total_if2_in.InvokeRequired)
            {
                total_if2_in.Invoke((MethodInvoker)(() => update_2_in()));
            }
            eth_if2_in.Text = eth_2_in.ToString();
            ip_if2_in.Text = ip_2_in.ToString();
            arp_if2_in.Text = arp_2_in.ToString();
            tcp_if2_in.Text = tcp_2_in.ToString();
            udp_if2_in.Text = udp_2_in.ToString();
            icmp_if2_in.Text = icmp_2_in.ToString();
            http_if2_in.Text = http_2_in.ToString();
            https_if2_in.Text = https_2_in.ToString();
            total_if2_in.Text = total_2_in.ToString();
        }

        private void update_2_out()
        {
            if (eth_if2_out.InvokeRequired)
            {
                eth_if2_out.Invoke((MethodInvoker)(() => update_2_out()));
            }
            if (ip_if2_out.InvokeRequired)
            {
                ip_if2_out.Invoke((MethodInvoker)(() => update_2_out()));
            }
            if (arp_if2_out.InvokeRequired)
            {
                arp_if2_out.Invoke((MethodInvoker)(() => update_2_out()));
            }
            if (tcp_if2_out.InvokeRequired)
            {
                tcp_if2_out.Invoke((MethodInvoker)(() => update_2_out()));
            }
            if (udp_if2_out.InvokeRequired)
            {
                udp_if2_out.Invoke((MethodInvoker)(() => update_2_out()));
            }
            if (icmp_if2_out.InvokeRequired)
            {
                icmp_if2_out.Invoke((MethodInvoker)(() => update_2_out()));
            }
            if (http_if2_out.InvokeRequired)
            {
                http_if2_out.Invoke((MethodInvoker)(() => update_2_out()));
            }
            if (https_if2_out.InvokeRequired)
            {
                https_if2_out.Invoke((MethodInvoker)(() => update_2_out()));
            }
            if (total_if2_out.InvokeRequired)
            {
                total_if2_out.Invoke((MethodInvoker)(() => update_2_out()));
            }
            eth_if2_out.Text = eth_2_out.ToString();
            ip_if2_out.Text = ip_2_out.ToString();
            arp_if2_out.Text = arp_2_out.ToString();
            tcp_if2_out.Text = tcp_2_out.ToString();
            udp_if2_out.Text = udp_2_out.ToString();
            icmp_if2_out.Text = icmp_2_out.ToString();
            http_if2_out.Text = http_2_out.ToString();
            https_if2_out.Text = https_2_out.ToString();
            total_if2_out.Text = total_2_out.ToString();
        }

        public int get_time()
        {
            int time_value;
            time_value = int.Parse(time_set.Text);
            Console.WriteLine(time_value);
            return time_value;
        }

        private void updateMacTable()
        {

                    mac_table.BeginInvoke(new Action(() =>
                    {
                        
                        mac_table.Rows.Clear();  
                        foreach (var entry in macTable.GetEntries().ToList())
                        {
                            mac_table.Rows.Add(entry.MacAddress,entry.InterfaceName,entry.RemainingTime);
                        }
                        mac_table.Refresh();

                    }));
        }

        public void refreshMacTable()
        {
            while(true)
            {
                
                Thread.Sleep(3000);
                try
                {
                    mac_table.Invoke(new Action(() =>
                    {
                        var entries = macTable.GetEntries();
                        if (entries.Count > 0)
                        {
                            mac_table.Rows.Clear();
                            foreach (var entry in macTable.GetEntries().ToList())
                            {
                                mac_table.Rows.Add(entry.MacAddress, entry.InterfaceName, entry.RemainingTime);
                            }
                            mac_table.Refresh();
                        }
                        else
                        {

                            mac_table.Rows.Clear();
                            mac_table.Refresh();
                        }

                    }));
                }
                catch (Exception ex) { }
            }       
        }


        private void Stop_Click(object sender, EventArgs e)
        {
            Stop.Enabled = false;
            Start.Enabled = true;
            Refresh.Enabled = true;
            int1_box.Enabled = true;
            int2_box.Enabled = true;
            device?.StopCapture();
            device2?.StopCapture();
            device?.Close();
            device2?.Close();
            mac_table.Invoke(new Action(() =>
            {
                mac_table.Refresh();

            }));
        }

        public void Refresh_Click(object sender, EventArgs e)
        {
            Start.Enabled = true;
            var devices = CaptureDeviceList.Instance;
            int1_box.SelectedIndex = -1;
            int2_box.SelectedIndex = -1;
            int1_box.Items.Clear();
            int2_box.Items.Clear();
            foreach (var device in devices)
            {
                int1_box.Items.Add(device.Description);
                int2_box.Items.Add(device.Description);
            }
        }

        public void Refresh_start()
        {
            Start.Enabled = true;
            var devices = CaptureDeviceList.Instance;
            int1_box.SelectedIndex = -1;
            int2_box.SelectedIndex = -1;
            int1_box.Items.Clear();
            int2_box.Items.Clear();
            foreach (var device in devices)
            {
                int1_box.Items.Add(device.Description);
                int2_box.Items.Add(device.Description);
            }
        }

        private void int1_box_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void int2_box_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void eth_if1_in_TextChanged(object sender, EventArgs e)
        {

        }

        private void sw_FormClosing(object sender, FormClosingEventArgs e)
        {
            device?.StopCapture();
            device?.Close();
            device2?.StopCapture();
            device2?.Close();
            Application.Exit();
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            eth_1_in = 0;
            eth_1_out = 0;

            ip_1_in = 0;
            ip_1_out = 0;

            arp_1_in = 0;
            arp_1_out = 0;

            tcp_1_in = 0;
            tcp_1_out = 0;

            udp_1_in = 0;
            udp_1_out = 0;

            icmp_1_in = 0;
            icmp_1_out = 0;

            http_1_in = 0;
            http_1_out = 0;

            https_1_in = 0;
            https_1_out = 0;

            total_1_in = 0;
            total_1_out = 0;

            eth_if1_in.Text = eth_1_in.ToString();
            ip_if1_in.Text = ip_1_in.ToString();
            arp_if1_in.Text = arp_1_in.ToString();
            tcp_if1_in.Text = tcp_1_in.ToString();
            udp_if1_in.Text = udp_1_in.ToString();
            icmp_if1_in.Text = icmp_1_in.ToString();
            http_if1_in.Text = http_1_in.ToString();
            https_if1_in.Text = https_1_in.ToString();
            total_if1_in.Text = total_1_in.ToString();

            eth_if1_out.Text = eth_1_out.ToString();
            ip_if1_out.Text = ip_1_out.ToString();
            arp_if1_out.Text = arp_1_out.ToString();
            tcp_if1_out.Text = tcp_1_out.ToString();
            udp_if1_out.Text = udp_1_out.ToString();
            icmp_if1_out.Text = icmp_1_out.ToString();
            http_if1_out.Text = http_1_out.ToString();
            https_if1_out.Text = https_1_out.ToString();
            total_if1_out.Text = total_1_out.ToString();

        }

        private void Reset2_Click(object sender, EventArgs e)
        {
            eth_2_in = 0;
            eth_2_out = 0;

            ip_2_in = 0;
            ip_2_out = 0;

            arp_2_in = 0;
            arp_2_out = 0;

            tcp_2_in = 0;
            tcp_2_out = 0;

            udp_2_in = 0;
            udp_2_out = 0;

            icmp_2_in = 0;
            icmp_2_out = 0;

            http_2_in = 0;
            http_2_out = 0;

            https_2_in = 0;
            https_2_out = 0;

            total_2_in = 0;
            total_2_out = 0;

            eth_if2_in.Text = eth_2_in.ToString();
            ip_if2_in.Text = ip_2_in.ToString();
            arp_if2_in.Text = arp_2_in.ToString();
            tcp_if2_in.Text = tcp_2_in.ToString();
            udp_if2_in.Text = udp_2_in.ToString();
            icmp_if2_in.Text = icmp_2_in.ToString();
            http_if2_in.Text = http_2_in.ToString();
            https_if2_in.Text = https_2_in.ToString();
            total_if2_in.Text = total_2_in.ToString();

            eth_if2_out.Text = eth_2_out.ToString();
            ip_if2_out.Text = ip_2_out.ToString();
            arp_if2_out.Text = arp_2_out.ToString();
            tcp_if2_out.Text = tcp_2_out.ToString();
            udp_if2_out.Text = udp_2_out.ToString();
            icmp_if2_out.Text = icmp_2_out.ToString();
            http_if2_out.Text = http_2_out.ToString();
            https_if2_out.Text = https_2_out.ToString();
            total_if2_out.Text = total_2_out.ToString();

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            mac_table.Rows.Clear();
            foreach (var entry in macTable.GetEntries().ToList())
            {
               
                entry.RemainingTime = get_time();
            }
            foreach (var entry in macTable.GetEntries().ToList())
            {
                 mac_table.Rows.Add(entry.MacAddress, entry.InterfaceName, entry.RemainingTime);
            }
            
            mac_table.Refresh();
        }

        private void Clear_Click(object sender, EventArgs e)
        {
           macTable.ClearTable();
           mac_table.Invoke(new Action(() =>
           {
                mac_table.Rows.Clear();
                mac_table.Refresh();

           }));
        }



        private void Add_Click(object sender, EventArgs e)
        {
            aclFilters.Clear();
            foreach (DataGridViewRow row in filters_table.Rows)
            {
                try
                {
                string iface = row.Cells["iface"].Value != null ? row.Cells["iface"].Value.ToString() : "";
                string protocol = row.Cells["protocol"].Value != null ? row.Cells["protocol"].Value.ToString() : "";
                string direction = row.Cells["in_out"].Value != null ? row.Cells["in_out"].Value.ToString() : "";
                bool allow = row.Cells["allow"].Value != null && Convert.ToBoolean(row.Cells["allow"].Value);
                bool deny = row.Cells["deny"].Value != null && Convert.ToBoolean(row.Cells["deny"].Value);
                string sourceIP = row.Cells["source_ip"].Value != null ? row.Cells["source_ip"].Value.ToString() : "";
                string destIP = row.Cells["dest_ip"].Value != null ? row.Cells["dest_ip"].Value.ToString() : "";
                string sourceMAC = row.Cells["source_mac"].Value != null ? row.Cells["source_mac"].Value.ToString() : "";
                string destMAC = row.Cells["dest_mac"].Value != null ? row.Cells["dest_mac"].Value.ToString() : "";

                ACL aclFilter = new ACL(iface, protocol, direction, allow, deny, sourceIP, destIP, sourceMAC, destMAC);
                aclFilters.Add(aclFilter);
                
                }catch (Exception ex)
                 {
                     MessageBox.Show(ex.Message);
                 }
            }
            filters_table.Invoke(new Action(() =>
            {
                filters_table.Rows.Add();
                filters_table.Refresh();
            }));
            if (aclFilters.Any(acl => acl.Allow == true )) { implicit_allow = true; }
            else { implicit_allow = false; }
            if (syslog_enabled)
            {
                syslog.SendSyslogMessage("New rule added to ACL table.");
            }
        }

        private void Clear_rules_Click(object sender, EventArgs e)
        {
            aclFilters.Clear();   
            filters_table.Invoke(new Action(() =>
            {
                filters_table.Rows.Clear();
                filters_table.Rows.Add();
                filters_table.Refresh();
            }));
            implicit_allow = false;
            if (syslog_enabled)
            {
                syslog.SendSyslogMessage("ACL table was cleared.");
            }
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            try
            {
            int rowIndex = filters_table.CurrentCell.RowIndex;
            filters_table.Invoke(new Action(() =>
            {     
                filters_table.Rows.RemoveAt(rowIndex);
            if (filters_table.RowCount == 0)
                {
                    filters_table.Rows.Add();
                }
                filters_table.Refresh();
            }));
             aclFilters.RemoveAt(rowIndex);  } catch(Exception ex) { }
            if (aclFilters.Any(acl => acl.Allow == true)) { implicit_allow = true; }
            else { implicit_allow = false; }
            if (syslog_enabled)
            {
                syslog.SendSyslogMessage("Rule was removed from ACL table.");
            }
        }

        private void start_syslog_Click(object sender, EventArgs e)
        {
            try
            {
                syslog = new Syslog(syslog_address.Text, 514);
                syslog_enabled = true;
                syslog.SendSyslogMessage("Syslog started. Server ip: " + syslog_address.Text);
                start_syslog.Enabled = false;
                stop_syslog.Enabled = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void stop_syslog_Click(object sender, EventArgs e)
        {
            syslog.SendSyslogMessage("Syslog stopped.");
            syslog_enabled = false;
            start_syslog.Enabled = true;
            stop_syslog.Enabled = false;
        }
    }
}
