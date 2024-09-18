using SharpPcap;
using PacketDotNet;
using System;
using System.Net;
using System.Text;
using System.Net.NetworkInformation;
namespace sw_L2switch_psip
{
    

    public class Syslog
    {
        private string serverIp;
        private int serverPort;

        public Syslog(string ip, int port)
        {
            serverIp = ip;
            serverPort = port;
        }

        public void SendSyslogMessage(string message)
        {

            try
            {
                EthernetPacket syslog_message_eth = new EthernetPacket(
                    PhysicalAddress.Parse("AA-BB-AA-BB-AA-BB"),
                    PhysicalAddress.Parse("FF-FF-FF-FF-FF-FF"),
                    EthernetType.IPv4);
                IPv4Packet syslog_message_ip = new IPv4Packet(
                       IPAddress.Parse("192.168.1.99"),
                       IPAddress.Parse(serverIp)
                    );
                UdpPacket syslog_message_udp = new UdpPacket(
                        64350, 514
                       );
                syslog_message_udp.PayloadData = Encoding.ASCII.GetBytes(message);
                syslog_message_ip.PayloadPacket = syslog_message_udp;
                syslog_message_eth.PayloadPacket = syslog_message_ip;

                sw.device.SendPacket(syslog_message_eth);
                sw.device2.SendPacket(syslog_message_eth);
            }
            catch(Exception ex) { }

        }   

        
    }
}
