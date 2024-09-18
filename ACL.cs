namespace sw_L2switch_psip
{
    public class ACL
    {
        public string Interface { get; set; }
        public string Protocol { get; set; }
        public string Direction { get; set; }
        public bool Allow { get; set; }
        public bool Deny { get; set; }
        public string SourceIP { get; set; }
        public string DestinationIP { get; set; }
        public string SourceMAC { get; set; }
        public string DestinationMAC { get; set; }

        public ACL(string iface, string protocol, string direction, bool allow, bool deny, string sourceIP, string destIP, string sourceMAC, string destMAC)
        {
            Interface = iface;
            Protocol = protocol;
            Direction = direction;
            Allow = allow;
            Deny = deny;
            SourceIP = sourceIP;
            DestinationIP = destIP;
            SourceMAC = sourceMAC;
            DestinationMAC = destMAC;
        }
    }
}
