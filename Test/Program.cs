using Destr.Codegen;
using SerializerGenerator.Destr.Codegen;
using SerializerGenerator.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SerializerGenerator
{
    class Program
    {
        private static void Main(string[] args)
        {
            CodeGenerator.Generate();

            /*
            Serializer.Generate();

            var serializer = Serializer.Get<TestPacketB>();

            TestPacketB etalon = new TestPacketB
            {
                a = {
                    a = 1,
                    b = 2,
                    c = 3,
                    d = 4,
                    e = 5,
                    f = 6,
                    g = true
                },
                b = 123
            };


            byte[] buffer = new byte[128];

            serializer.Write(new BinaryWriter(new MemoryStream(buffer)), in etalon);

            TestPacketB result = default;
            serializer.Read(ref result, new BinaryReader(new MemoryStream(buffer)));

            Console.WriteLine(etalon.a.a == result.a.a);
            Console.WriteLine(etalon.a.b == result.a.b);
            Console.WriteLine(etalon.a.c == result.a.c);
            Console.WriteLine(etalon.a.d == result.a.d);
            Console.WriteLine(etalon.a.e == result.a.e);
            Console.WriteLine(etalon.a.f == result.a.f);
            Console.WriteLine(etalon.a.g == result.a.g);
            Console.WriteLine(etalon.b == result.b);
            */

            /*
            MemoryStream stream = new MemoryStream(new byte[] { 1, 2, 3 });
            BinaryReader reader = new BinaryReader(stream);
            Console.WriteLine(reader.ReadByte());
            Console.WriteLine(reader.ReadByte());
            Console.WriteLine(reader.ReadByte());
            stream.Position = 0;
            Console.WriteLine(reader.ReadByte());
            Console.WriteLine(reader.ReadByte());
            Console.WriteLine(reader.ReadByte());
            */

            /*
            var protocol = new TestProtocol();
            protocol.Listen((in ProtocolPacket0 packet) => { 
            
            });
            */

            /*
            Serializer.Generate();


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

            CodeGenerator.Generate();
            */
        }

        public static void OnProtocolPacket0(in ProtocolPacket0 package)
        {
            Console.WriteLine(package.a);
        }
    }
}