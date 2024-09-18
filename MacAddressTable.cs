using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Threading;

namespace sw_L2switch_psip
{
    public class MacAddressTable
    {
        private List<MacAddressTableEntry> entries;

        public MacAddressTable()
        {
            entries = new List<MacAddressTableEntry>();
            Thread t = new Thread(new ThreadStart(Timer_Tick));
            t.Start();
        }

        public void AddEntry(string macAddress, string interfaceName, int timeoutInSeconds)
        {
            var existingEntry = entries.FirstOrDefault(e => e.MacAddress == macAddress);
            if (existingEntry != null)
            {
                existingEntry.RemainingTime = timeoutInSeconds;
            }
            else
            {
                var newEntry = new MacAddressTableEntry(macAddress, interfaceName, timeoutInSeconds);
                entries.Add(newEntry);
                if(sw.syslog_enabled == true){
                    sw.syslog.SendSyslogMessage("New physical address added to MAC table: " + newEntry.MacAddress + " from interface: " + newEntry.InterfaceName);
                }
            }
        }

        public List<MacAddressTableEntry> GetEntries()
        {
            return entries;
        }

        private void Timer_Tick()
        {

            while (true)
            {
                Thread.Sleep(1000);
                foreach (var entry in entries)
                {
                    entry.DecrementTime();
                    if(entry.RemainingTime <= 0 && sw.syslog_enabled == true)
                    {
                        sw.syslog.SendSyslogMessage(entry.MacAddress +" from interface: " + entry.InterfaceName + " removed from MAC table.");
                    }
                }
                entries.RemoveAll(entry => entry.RemainingTime <= 0);
            }
            
        }
        public void ClearTable()
        {
            entries.Clear();
            if (sw.syslog_enabled == true)
            {
                sw.syslog.SendSyslogMessage("MAC table was cleared");
            }
        }
    }

    public class MacAddressTableEntry
    {
        public string MacAddress { get; }
        public string InterfaceName { get; }
        public int TimeoutInSeconds { get; }
        public int RemainingTime { get; set; }


        public MacAddressTableEntry(string macAddress, string interfaceName, int timeoutInSeconds)
        {
            MacAddress = macAddress;
            InterfaceName = interfaceName;
            TimeoutInSeconds = timeoutInSeconds;
            RemainingTime = timeoutInSeconds;

        }

        public void ResetTimer()
        {
            RemainingTime = TimeoutInSeconds;
        }

        public void DecrementTime()
        {
            
            RemainingTime--;
            
        }

        private void Timer_Tick(object sender, ElapsedEventArgs e)
        {
            DecrementTime();
        }

        
    }
}
