using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Destr.Codegen;
using Destr.Codegen.Source;
using Destr.IO;
using Destr.Protocol;


namespace Assets.SerializerGenerator.Codegen.Net
{
    public class CachedPacketReaderGenerator : ClassSourceGenerator, ICodeGenerator
    {
        public void Generate()
        {
            foreach (Type type in CodeGenerator.GetTypes())
            {
                if (type.IsInterface) continue;
                if (type.IsAbstract) continue;
                if (type.FindGenericInterface(typeof(ICachedPacketReader<>)) == null) continue;

                Name = SimpleName(type);
                Namespace = type.Namespace;

                foreach (var generated in type.GetCustomAttributes<Generated>())
                {
                    Type generatedInterface = generated?.Argument ?? type.FindGenericInterface(typeof(ICachedPacketReader<>));
                    if (generatedInterface == null || generatedInterface.GetGenericTypeDefinition() != typeof(ICachedPacketReader<>)) continue;
                    Clear();
                    Attributes.Add(generated);
                    SetPartial(generated.IsPartial);
                    Make(type, generatedInterface.GetGenericArguments()[0]);
                    Write(generated.File);
                }
            }
        }

        public void Make(Type type, Type protocol)
        {
            Type packetInterface = typeof(IPacket<>).MakeGenericType(protocol);
            Extends.Add(typeof(ICachedPacketReader<>).MakeGenericType(protocol));
            List<Type> packetTypeList = new List<Type>();
            foreach (Type packetType in CodeGenerator.GetUsedTypes())
            {
                if (packetType.IsInterface || packetType.IsAbstract) continue;
                Type packetGenericInterface = packetType.FindGenericInterface(typeof(IPacket<>));
                if (packetGenericInterface == null) continue;
                if (packetGenericInterface.GetGenericArguments()[0] != protocol) continue;
                packetTypeList.Add(packetType);
            }

            var readMethod = AddMethod("ReadPackets").Public.Void;
            readMethod.Argument.Add<BinaryReader>().Add(" reader");

            var packetIdField = "packetId";
            readMethod.AddLine($"ushort {packetIdField} = reader.ReadUInt16();");
            var readSwitch = readMethod.AddSwitch(packetIdField);

            readSwitch.Line.Add("default: throw new ").Add<Exception>().Add($"($\"Wrong packet id: {{{packetIdField}}}\");");

            int id = 0;
            foreach (Type packetType in packetTypeList)
            {
                Type serializerType = typeof(ISerializer<>).MakeGenericType(packetType);
                Type queueType = typeof(ConcurrentQueue<>).MakeGenericType(packetType);
                string packetName = SimpleName(packetType);
                string idField = $"_{id}{packetName}Id";
                string serializerField = $"_{id}{packetName}Serializer";
                string queueField = $"_{id}{packetName}Queue";
                Fields.Add($"private const ushort {idField} = {id};");
                Fields.Line.Add("private static ").Add(serializerType).Add($" {serializerField} = null;");
                Fields.Line.Add("private readonly ").Add(queueType).Add($" {queueField} = new ").Add(queueType).Add("();");

                string readTempVar = $"p{id}";
                var readCase = readSwitch.AddCase(idField);
                readCase.Line.Add(serializerField).Add($".Read(reader, out var {readTempVar});");
                readCase.Line.Add(queueField).Add($".Enqueue({readTempVar});");

                readCase.Add("break;");

                var specializedAccept = AddMethod("TryAcceptPacket").Public.Return("bool");
                specializedAccept.Argument.Out.Add(packetType).Add(" packet");
                specializedAccept.Line.Add("return ").Add(queueField).Add(".TryDequeue(out packet);");

                id++;
            }

            var methodTryAccept = AddMethod("TryAcceptPacket<D>").Public.Return("bool");
            methodTryAccept.Argument.Out.Add("D packet");
            methodTryAccept.Where.Line.Add("D : ").Add(packetInterface);
            methodTryAccept.Line.Add("throw new ").Add<Exception>().Add("();");
        }
    }
}