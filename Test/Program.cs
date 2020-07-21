using Destr.Codegen;
using SerializerGenerator.Test;
using System;
using System.IO;

namespace SerializerGenerator
{
    class Program
    {
        private static void Main(string[] args)
        {
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
        }
    }
}