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

            //Serializer.Generate();
            //var protocol = new TestProtocol();
            //ProtocolPacket0 p0 = default;
            //Console.WriteLine(Serializer.Defenition(typeof(TestPacketA)));

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

            MemoryStream stream = new MemoryStream(new byte[] { 1, 2, 3 });
            BinaryReader reader = new BinaryReader(stream);
            protocol.Read(reader);
            */

            //foreach (var list in new ProtocolGenerator("", typeof(TestProtocol)).GenerateStrings())
            //    Console.WriteLine(list);

            /*
            SourceGenerator src = new SourceGenerator();

            src.AddLine("1");
            src.AddLine(()=> {
                List<string> lines = new List<string>();
                lines.Add("2");
                lines.Add("3");
                return lines;
            });
            src.AddLine(() => {
                return "4";
            });

            foreach (string line in src.GetSourceLines())
                Console.WriteLine(line);
                */

            /*
            foreach (var list in new ProtocolGenerator(typeof(TestProtocol)).GetSourceLines())
                Console.WriteLine(list);
                */
            CodeGenerator.Generate();
        }
    }
}