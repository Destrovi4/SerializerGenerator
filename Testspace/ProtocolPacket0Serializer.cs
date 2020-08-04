using Destr.Codegen;
using Destr.IO;
using System.IO;
namespace Testspace
{
    [Generated]
    public class ProtocolPacket0Serializer : ISerializer<ProtocolPacket0>
    {
        private ISerializer<TestPacketB> _ser0TestPacketB = null;
        public void Read(ref ProtocolPacket0 value, BinaryReader reader)
        {
            value.a = reader.ReadByte();
            _ser0TestPacketB.Read(ref value.b, reader);
        }
        public void Write(BinaryWriter writer, in ProtocolPacket0 value)
        {
            writer.Write(value.a);
            _ser0TestPacketB.Write(writer, in value.b);
        }
    }
}
