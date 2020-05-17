

namespace SwarmingFleet.Broker
{
    using Google.Protobuf;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    public static class BrokerSideUtils
    {
        public static ByteString GetNonce()
        {
            var cnonce = Guid.NewGuid().ToByteArray();

            return ByteString.CopyFrom(cnonce);
        }

        public static ByteString ComputeHash(ByteString nonce, ByteString cnonce,string spk)
        {
            var n = new BitArray(nonce.ToArray());
            var nc = new BitArray(cnonce.ToArray());
            var k = new BitArray(Convert.FromBase64String(spk));
            var r = n.Xor(nc).Xor(k);
            var ns = new byte[r.Count];
            r.CopyTo(ns, 0);
            return ByteString.CopyFrom(ns);
        }

    }
}
