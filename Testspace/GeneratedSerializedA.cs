using Destr.Codegen;
using Destr.IO;
using System.IO;
namespace Testspace
{
    [Generated]
    public class GeneratedSerializedA : ISerializer<TestPacketA>
    {
        public void Read(ref TestPacketA value, BinaryReader reader)
        {
            value.a = reader.ReadByte();
            value.b = reader.ReadInt16();
            value.c = reader.ReadInt32();
            value.d = reader.ReadInt64();
            value.e = reader.ReadSingle();
            value.f = reader.ReadDouble();
            value.g = reader.ReadBoolean();
        }
        public void Write(BinaryWriter writer, in TestPacketA value)
        {
            writer.Write(value.a);
            writer.Write(value.b);
            writer.Write(value.c);
            writer.Write(value.d);
            writer.Write(value.e);
            writer.Write(value.f);
            writer.Write(value.g);
        }
    }
}
