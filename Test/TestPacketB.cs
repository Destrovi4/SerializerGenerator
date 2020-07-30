using Destr.Codegen;

namespace Test
{
    [SerializerGaranted]
    public struct TestPacketB
    {
        public TestPacketA a;
        public int b;
    }
}
