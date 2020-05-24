
using System;
using System.Reflection;

[assembly: Id(0x12, 0xfa, 0xc0, 0x1c, 0x92, 0xf3, 0xd6, 0x4a, 0x86, 0x65, 0x1d, 0x45, 0x5d, 0x28, 0x19, 0x33)] 
[assembly: AssemblyKeyFile(@"C:\Users\Yuyu\Source\Repos\SF\src\SwarmingFleet.Worker\worker-1.0.0.snk")]

[AttributeUsage(AttributeTargets.Assembly)]
internal sealed class IdAttribute : Attribute
{
    internal readonly byte[] Binary;

    internal IdAttribute(params byte[] binary)
    {
        this.Binary = binary;
    }
}