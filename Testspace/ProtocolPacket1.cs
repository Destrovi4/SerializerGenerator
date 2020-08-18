using Destr.Codegen;
using Destr.Protocol;
using Test;


namespace Testspace
{
    [SerializerGaranted]
    public struct ProtocolPacket1 : IPacket<TestProtocol>
    {
        public int test;
    }
}
