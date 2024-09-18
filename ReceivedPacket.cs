using System;
using System.Net;
using System.Collections.Generic;
namespace sw_L2switch_psip
{
    public class ReceivedPacket
    {
        public byte[] RawData { get; set; }
        public string SourceMAC { get; set; }
        public string DestinationMAC { get; set; }
        public IPAddress SourceIP { get; set; }
        public IPAddress DestinationIP { get; set; }
        public int SourcePort { get; set; }
        public int DestinationPort { get; set; }
        public string Protocol { get; set; }
        //public List<string> Protocols { get; set; }

        public ReceivedPacket(byte[] rawData)
        {
            RawData = rawData;
            ParsePacket();
        }

        private void ParsePacket()
        {
            var ethernetPacket = PacketDotNet.Packet.ParsePacket(PacketDotNet.LinkLayers.Ethernet, RawData) as PacketDotNet.EthernetPacket;
            if (ethernetPacket != null)
            {
                SourceMAC = ethernetPacket.SourceHardwareAddress.ToString();
                DestinationMAC = ethernetPacket.DestinationHardwareAddress.ToString();

                var ipPacket = ethernetPacket.PayloadPacket as PacketDotNet.IPPacket;
                if (ipPacket != null)
                {
                    SourceIP = ipPacket.SourceAddress;
                    DestinationIP = ipPacket.DestinationAddress;
                    //Protocols.Add("IP");
                    Protocol = "IP";
                    var tcpPacket = ipPacket.PayloadPacket as PacketDotNet.TcpPacket;
                    if (tcpPacket != null)
                    {
                        //Protocols.Add("TCP");
                        Protocol = "TCP";
                        SourcePort = tcpPacket.SourcePort;
                        DestinationPort = tcpPacket.DestinationPort;
                        if (tcpPacket.SourcePort == 80 || tcpPacket.DestinationPort == 80)
                        {
                            //Protocols.Add("HTTP");
                            Protocol = "HTTP";
                        }
                        else if (tcpPacket.SourcePort == 443 || tcpPacket.DestinationPort == 443)
                        {
                            //Protocols.Add("HTTPS");
                            Protocol = "HTTPS";
                        }
                    }
                    else
                    {
                        var udpPacket = ipPacket.PayloadPacket as PacketDotNet.UdpPacket;
                        if (udpPacket != null)
                        {
                            //Protocols.Add("UDP");
                            Protocol = "UDP";
                            SourcePort = udpPacket.SourcePort;
                            DestinationPort = udpPacket.DestinationPort;
                        }
                        else
                        {
                            var icmpv4Packet = ipPacket.PayloadPacket as PacketDotNet.IcmpV4Packet;
                            if (icmpv4Packet != null)
                            {
                                //Protocols.Add("ICMP");
                                Protocol = "ICMP";
                            }
                            var icmpv6Packet = ipPacket.PayloadPacket as PacketDotNet.IcmpV6Packet;
                            if (icmpv6Packet != null)
                            {
                                //Protocols.Add("ICMP");
                                Protocol = "ICMP";
                            }
                        }
                    }
                }
                var arpPacket = ethernetPacket.PayloadPacket as PacketDotNet.ArpPacket;
                if (arpPacket != null)
                {
                    //Protocols.Add("ARP");
                    Protocol = "ARP";
                }
            }
        }
    }
}
