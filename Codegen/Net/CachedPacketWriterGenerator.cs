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
    public class CachedPacketWriterGenerator : ClassSourceGenerator, ICodeGenerator
    {
        public void Generate()
        {
            foreach (Type type in CodeGenerator.GetTypes())
            {
                if (type.IsInterface) continue;
                if (type.IsAbstract) continue;
                if (type.FindGenericInterface(typeof(ICachedPacketWriter<>)) == null) continue;

                Name = SimpleName(type);
                Namespace = type.Namespace;

                foreach (var generated in type.GetCustomAttributes<Generated>())
                {
                    Type generatedInterface = generated?.Argument ?? type.FindGenericInterface(typeof(ICachedPacketWriter<>));
                    if (generatedInterface == null || generatedInterface.GetGenericTypeDefinition() != typeof(ICachedPacketWriter<>)) continue;
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
            Extends.Add(typeof(ICachedPacketWriter<>).MakeGenericType(protocol));
            List<Type> packetTypeList = new List<Type>();
            foreach (Type packetType in CodeGenerator.GetUsedTypes())
            {
                if (packetType.IsInterface || packetType.IsAbstract) continue;
                Type packetGenericInterface = packetType.FindGenericInterface(typeof(IPacket<>));
                if (packetGenericInterface == null) continue;
                if (packetGenericInterface.GetGenericArguments()[0] != protocol) continue;
                packetTypeList.Add(packetType);
            }

            var writeMethod = AddMethod("WritePackets").Public.Void;
            writeMethod.Argument.Add<BinaryWriter>().Add(" writer");

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
                writeMethod.Line.Add($"while({queueField}.TryDequeue(out var {readTempVar}))");
                var writeMethodBody = writeMethod.Block.Bordered;
                writeMethodBody.Add($"writer.Write({idField});");
                writeMethodBody.Add($"{serializerField}.Write(writer, in {readTempVar});");

                var specializedSend = AddMethod("SendPacket").Public.Void;
                specializedSend.Argument.In.Add(packetType).Add("packet");
                specializedSend.AddLine($"{queueField}.Enqueue(packet);");
                specializedSend.AddLine("OnPacketSent();");

                id++;
            }

            var sendMethod = AddMethod("SendPacket<D>").Public.Void;
            sendMethod.Argument.In.Add("D packet");
            sendMethod.Where.Line.Add("D : ").Add(packetInterface);
            sendMethod.Line.Add("throw new ").Add<Exception>().Add("();");

            Methods.Add("partial void OnPacketSent();");
        }
    }
}


/*
 public void Send(in UserCommand packet)
        {
            _0UserCommandQueue.Enqueue(packet);
        }
 * */

/*
 public void Write(BinaryWriter writer)
        {
            while(_0UserCommandQueue.TryDequeue(out var p0))
            {
                writer.Write(_0UserCommandId);
                _0UserCommandSerializer.Write(writer, in p0);
            }
        }
        */

/*
public void Send<D>(in D packet) where D : IPacket<ClientServerProtocol>
        {
            throw new Exception();
        }
        */


/*
private const ushort _0UserCommandId = 0;
private static ISerializer<UserCommand> _0UserCommandSerializer = null;
private readonly ConcurrentQueue<UserCommand> _0UserCommandQueue = new ConcurrentQueue<UserCommand>();
*/