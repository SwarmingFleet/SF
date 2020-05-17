namespace SwarmingFleet.Worker
{
    using System;
    using System.Linq;
    using Google.Protobuf;
    using System.Collections;
    using SwarmingFleet.Common;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Numerics;
    using System.Text;
    using System.Management;
    using Version = Contracts.Version;
    using System.Diagnostics;

    [DebuggerNonUserCode]
    internal static class WorkerSideUtils
    {
        static WorkerSideUtils()
        {

        }

        internal static readonly Version Version = new Version { Major = 1, Minor = 0, Build = 1 };


        internal static ByteString GetSignature()
        {
            int index = 0;
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
                    big ^= (new BigInteger(Encoding.UTF8.GetBytes(mo["SerialNumber"] as string)) << index++);
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

                    big ^= (new BigInteger(Encoding.UTF8.GetBytes(mo["MACAddress"] as string)) << index++);

                }
            }
            var k = big.ToByteArray();
            if (k.Length < 16)
            {
                Array.Resize(ref k, 16);
            }
            else
            {
                k = k.Length > 16 ? k.AsSpan(0..16).ToArray() : k;
            }

            return ByteString.CopyFrom(k);
        }
         
        internal static ByteString GetNonce()
        {
            var cnonce = Guid.NewGuid().ToByteArray();
            return ByteString.CopyFrom(cnonce);
        }
         
        internal static ByteString ComputeHash(ByteString nonce, ByteString cnonce)
        { 
            var entrypoint = Assembly.GetEntryAssembly();
            var k = entrypoint.GetCustomAttribute<IdAttribute>()?.Binary ?? throw new InvalidProgramException();
            Console.WriteLine(k.Length);
            try
            {
               // k.CheckElementsCount(128, nameof(k));
                var cn = cnonce.ToArray();
                Console.WriteLine(cn.Length);
               // cn.CheckElementsCount(128, nameof(cn));
                var n = nonce.ToArray();
                Console.WriteLine(n.Length);
                // n.CheckElementsCount(128, nameof(n));

                var r = new BitArray(cn)
                    .Xor(new BitArray(n))
                    .Xor(new BitArray(k));

                var ns = new byte[r.Count];
                r.CopyTo(ns, 0);

                return ByteString.CopyFrom(ns);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }

    
}
