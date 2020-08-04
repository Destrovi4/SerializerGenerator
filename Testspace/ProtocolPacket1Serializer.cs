using Destr.Codegen;
using Destr.IO;
using System.IO;
namespace Testspace
{
    [Generated]
    public class ProtocolPacket1Serializer : ISerializer<ProtocolPacket1>
    {
        public void Read(ref ProtocolPacket1 value, BinaryReader reader)
        {
            value.test = reader.ReadInt32();
        }
        public void Write(BinaryWriter writer, in ProtocolPacket1 value)
        {
            writer.Write(value.test);
        }
    }
}
