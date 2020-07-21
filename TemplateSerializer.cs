using Destr.Codegen;
using System.IO;

namespace SerializerGenerator
{
    public class TemplateSerializer : ISerializer<TestPacket>
    {
        public void Read(ref TestPacket value, BinaryReader reader)
        {
            value.a = reader.ReadByte();
            value.b = reader.ReadInt16();
            value.c = reader.ReadInt32();
            value.d = reader.ReadInt64();
            value.e = reader.ReadSingle();
            value.g = reader.ReadDouble();
            value.h = reader.ReadBoolean();

        }

        public void Write(BinaryWriter writer, in TestPacket value)
        {
            writer.Write(value.a);
            writer.Write(value.b);
        }
    }
}
