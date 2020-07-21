using Destr.Codegen;
using System.IO;
using SerializerGenerator;
namespace SerializerGenerator.Test
{
    [Generated]
    public class TestPacketBSerializer : ISerializer<TestPacketB>
    {
        private ISerializer<TestPacketA> _ser0TestPacketA = null;
        public void Read(ref TestPacketB value, BinaryReader reader)
        {
            _ser0TestPacketA.Read(ref value.a, reader);
            value.b = reader.ReadInt32();
        }
        public void Write(BinaryWriter writer, in TestPacketB value)
        {
            _ser0TestPacketA.Write(writer, in value.a);
            writer.Write(value.b);
        }
    }
}
