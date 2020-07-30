using Destr.Codegen;
using Destr.Protocol;

namespace Test
{
    [SerializerGaranted]
    public struct ProtocolPacket1 : IPacket<TestProtocol>
    {
        public int test;
    }
}
