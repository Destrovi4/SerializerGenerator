using Destr.Codegen;

namespace Testspace
{
    [SerializerGaranted]
    public struct TestPacketB
    {
        public TestPacketA a;
        public int b;
    }
}
