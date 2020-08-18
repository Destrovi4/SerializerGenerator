using Destr.Codegen;
using Destr.Protocol;
using Test;


namespace Testspace
{
    [SerializerGaranted]
    public struct ProtocolPacket0 : IPacket<TestProtocol>
    {
        public byte a;
        public TestPacketB b;
    }
}
