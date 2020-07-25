using Destr.Codegen;
using SerializerGenerator.Destr.Codegen;

namespace SerializerGenerator.Test
{
    [SerializerGaranted]
    public struct ProtocolPacket0 : IPacket<TestProtocol>
    {
        public byte a;
        public TestPacketB b;
    }
}
