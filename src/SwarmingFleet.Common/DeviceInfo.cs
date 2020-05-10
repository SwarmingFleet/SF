
namespace SwarmingFleet.Common
{
    using System.Management;
    using System.Net.NetworkInformation;
    using System.Collections.Immutable;
    using System.Numerics;
    using System.Text;
    using System.Linq;
    using System; 

    public static class DeviceInfo
    {
        static DeviceInfo()
        {
            var macAddresses = ImmutableDictionary.CreateBuilder<string, string>();
            using (var mo = new ManagementObjectSearcher(@"
                SELECT 
                    MACAddress, Name 
                FROM 
                    Win32_NetworkAdapter 
                WHERE 
                    (MACAddress IS NOT NULL) AND
                    (PNPDeviceID LIKE 'PCI\\%')")
               .Get())
                foreach (var obj in mo)
                {
                    macAddresses.Add(obj["Name"] as string, (obj["MACAddress"] as string).Replace(":", null));
                }
            MacAddresses = macAddresses.ToImmutable();

            var cpus = ImmutableArray.CreateBuilder<string>();
            using (var mo = new ManagementObjectSearcher(@"
                SELECT 
                    Name 
                FROM 
                    Win32_Processor")
               .Get())
                foreach (var obj in mo)
                {
                    cpus.Add(obj["Name"] as string);
                }
            CPUs = cpus.ToImmutable();

            var gpus = ImmutableDictionary.CreateBuilder<string, uint>();
            using (var mo = new ManagementObjectSearcher(@"
                SELECT 
                    Name, AdapterRAM
                FROM 
                    Win32_VideoController")
               .Get())
                foreach (var obj in mo)
                {
                    gpus.Add(obj["Name"] as string, (uint)obj["AdapterRAM"] / (1 << 30));
                }
            GPUs = gpus.ToImmutable();

            var disks = ImmutableDictionary.CreateBuilder<string, ulong>();
            using (var mo = new ManagementObjectSearcher(@"
                SELECT 
                    Caption, Size
                FROM 
                    Win32_DiskDrive")
               .Get())
                foreach (var obj in mo)
                {
                    disks.Add(obj["Caption"] as string, (ulong)obj["Size"] / (1 << 30));
                }
            Disks = disks.ToImmutable();

            var rams = ImmutableList.CreateBuilder<ulong>();
            using (var mo = new ManagementObjectSearcher(@"
                SELECT 
                    Capacity
                FROM 
                    Win32_PhysicalMemory")
               .Get())
                foreach (var obj in mo)
                {
                    rams.Add((ulong)obj["Capacity"] / (1 << 30));
                }
            RAMs = rams.ToImmutable();


            int i = 0;
            BigInteger big = 1;
            using (var mos = new ManagementObjectSearcher(@"
                SELECT 
                    SerialNumber
                FROM 
                    Win32_DiskDrive")
               .Get())
            {
                foreach (var mo in mos.Cast<ManagementObject>()
                    .OrderBy(x => x.Properties["SerialNumber"].Value as string))
                {
                    big ^= (new BigInteger(Encoding.UTF8.GetBytes(mo["SerialNumber"] as string)) << i++);
                }
            }

            using (var mos = new ManagementObjectSearcher(@"
                SELECT 
                    MACAddress 
                FROM 
                    Win32_NetworkAdapter 
                WHERE 
                    (MACAddress IS NOT NULL) AND
                    (PNPDeviceID LIKE 'PCI\\%')")
               .Get())
            {
                foreach (var mo in mos.Cast<ManagementObject>()
                    .OrderBy(x => x.Properties["MACAddress"].Value as string))
                {

                    big ^= (new BigInteger(Encoding.UTF8.GetBytes(mo["MACAddress"] as string)) << i++);

                }
            }
            Identifier = Convert.ToBase64String(big.ToByteArray());
        }

        public readonly static IImmutableDictionary<string, string> MacAddresses;
        public readonly static IImmutableList<string> CPUs;
        public readonly static IImmutableDictionary<string, uint> GPUs;
        public readonly static IImmutableDictionary<string, ulong> Disks;
        public readonly static IImmutableList<ulong> RAMs;
        public readonly static string Identifier;
    }
}
