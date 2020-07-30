using Destr.IO;
using Destr.Protocol;
using System;
using System.IO;

namespace Test
{
    class Program
    {
        private static void Main(string[] args)
        {
            byte[] buffer = new byte[128];

            var writer = new BinaryWriter(new MemoryStream(buffer));

            TestPacketB result = default;
            var reader = new BinaryReader(new MemoryStream(buffer));

            var protocol = new TestProtocol();
            protocol.Listen((PacketListener<TestProtocol, ProtocolPacket0>)OnProtocolPacket0);

            ProtocolPacket0 p0 = default;
            p0.a = 123;
            protocol.Write(writer, in p0);
            protocol.Read(reader);

            Console.WriteLine(Serializer.Defenition(typeof(TestPacketA)));

        }

        public static void OnProtocolPacket0(in ProtocolPacket0 package)
        {
            Console.WriteLine(package.a);
        }
    }
}