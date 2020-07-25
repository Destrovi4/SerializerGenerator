using Destr.Codegen;
using SerializerGenerator.Destr.Codegen;
using System;
using System.Collections.Generic;
using System.Text;

namespace SerializerGenerator.Test
{
    [SerializerGaranted]
    public struct ProtocolPacket1 : IPacket<TestProtocol>
    {
        public int test;
    }
}
