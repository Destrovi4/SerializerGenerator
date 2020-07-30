using Destr.Codegen;
using Destr.Protocol;

namespace Test
{
    [SerializerGaranted]
    public struct ProtocolPacket0 : IPacket<TestProtocol>
    {
        public byte a;
        public TestPacketB b;
    }
}
