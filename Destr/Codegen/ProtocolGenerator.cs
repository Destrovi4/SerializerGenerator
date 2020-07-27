﻿using Destr.Codegen.Source;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Destr.Codegen
{
    public class ProtocolGenerator : ClassGenerator
    {
        protected override bool FilterType(Type type)
        {
            if (type.IsInterface)
                return false;
            if (type.IsAbstract)
                return false;

            var baseType = type.BaseType;
            if (!baseType.IsGenericType)
                return false;

            if (baseType.GetGenericTypeDefinition() != typeof(Protocol<>))
                return false;

            return true;
        }

        protected override IEnumerable<string> Generate(Type type) => new ProtocolSourceGenerator(type).GetSourceLines();
    }

    public class ProtocolSourceGenerator : ClassSourceGenerator
    {
        private const string ReadingDescriptor = "_descriptorRead";
        private const string WritingDescriptor = "_descriptorWrite";

        public ProtocolSourceGenerator(Type type)
        {
            Name = SimpleName(type);
            Namespace = type.Namespace;

            Type abstractPackage = Type.MakeGenericSignatureType(typeof(IPacket<>), type);
            Type writerAction = Type.MakeGenericSignatureType(typeof(Action<>), typeof(BinaryWriter), abstractPackage);

            Attributes.Add<Generated>();
            var packageTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType)
                    .Where(i => i.GetGenericTypeDefinition() == typeof(IPacket<>))
                    .Any(i => i.GetGenericArguments()[0] == type)
                ).ToArray();

            Dictionary<Type, string> descriptionByType = new Dictionary<Type, string>();
            foreach (var packageType in packageTypes)
                descriptionByType.Add(packageType, Serializer.Defenition(packageType));

            packageTypes.OrderBy(t => descriptionByType[t]);
            Fields.AddLine($"public const string Definition = \"{string.Join(";", packageTypes.Select(t => descriptionByType[t]))}\";");

            var staticConstructor = AddMethod(Name).Static;
            var constructor = AddMethod(Name).Public;

            Require(typeof(Dictionary<Type, uint>));
            Fields.AddLine("private static readonly Dictionary<Type, uint> _packetIdByType = new Dictionary<Type, uint>();");


            AddMethod("Read").Public.Void.Override.AddArgument<BinaryReader>("reader")
                .AddLine($"{ReadingDescriptor}[reader.ReadUInt16()].Invoke(reader);");
            var abstractWriter = AddMethod("Write<D>").Public.Void.Override.AddArgument<BinaryWriter>("writer").AddArgument("in D data").AddWhere("D : struct")
                .AddLine($"(_descriptorWrite[_packetIdByType[typeof(D)]] as writer<D>)(writer, in data);");
            
            var listen = AddMethod("Listen<D>").Public.Override.Void.AddArgument($"PacketListener<{Name}, D> listener");
            var listenSwitch = listen.AddSwitch("_packetIdByType[typeof(D)]");
            listenSwitch.Case.Default.Add("throw new Exception();");

            Require<BinaryReader>();
            Extends.Add(typeof(Protocol<>), type);

            int fieldIndex = 0;
            foreach (var packageType in packageTypes)
            {
                string packageTypeName = SimpleName(packageType);
                string id = $"_{fieldIndex}{packageTypeName}Id";
                string reader = $"P{fieldIndex}{packageTypeName}Reader";
                string writer = $"Write";
                string serializer = $"_{fieldIndex}{packageTypeName}Serializer";
                string listener = $"_{fieldIndex}{packageTypeName}Listener";

                Fields.Line.Add($"private const ushort {id} = {fieldIndex};");

                Fields.Line.Add($"private ").Add(typeof(ISerializer<>), packageType).Add($" {serializer} = null;");
                Fields.Line.Add($"private ").Add(typeof(PacketListener<,>), type, packageType).Add($" {listener} = null;");

                staticConstructor.AddLine($"_packetIdByType.Add(typeof(").Add(packageType).Add($"), {id});");
                constructor.AddLine($"{ReadingDescriptor}[{id}] = {reader};");
                constructor.AddLine($"{WritingDescriptor}[{id}] = (writer<").Add(packageType).Add($">){writer};");

                var readMethod = AddMethod(reader).Private.Void.AddArgument<BinaryReader>("reader");
                readMethod.Line.Add(packageType).Add(" packet = default;");
                readMethod.Line.Add($"{serializer}.Read(ref packet, reader);");
                readMethod.Line.Add($"{listener}(in packet);");

                var writeMethod = AddMethod(writer).Public.Void;
                writeMethod.AddArgument<BinaryWriter>("writer");
                writeMethod.Argument.In.Add(packageType).Add("packet");
                writeMethod.Line.Add($"writer.Write({id});");
                writeMethod.Line.Add($"{serializer}.Write(writer, in packet);");

                listenSwitch.AddCase(id).Break.Line.Add($"{listener} = (PacketListener<{Name}, ").Add(packageType).Add(">)(object)listener;");
                constructor.Line.Add($"{serializer} = Serializer.Get<").Add(packageType).Add(">();");

                fieldIndex++;
            }

            Require(typeof(Action<>));
            Fields.Add($"private Action<BinaryReader>[] {ReadingDescriptor} = new Action<BinaryReader>[{fieldIndex}];");
            Fields.Line.Add($"private readonly object[] {WritingDescriptor} = new object[{fieldIndex}];");
            Fields.Line.Add($"delegate void writer<D>(BinaryWriter writer, in D data) where D : struct, IPacket<{Name}>;");
        }
    }
}

/*
        public readonly string Defenition;
        private Dictionary<Type, object> _serializerByType = new Dictionary<Type, object>();
        public Protocol()
        {
            var assembly = Assembly.GetExecutingAssembly();

            Queue<Type> toCheckDependency = new Queue<Type>();

            foreach (var type in assembly.GetTypes())
            {
                Type packageInterface = type.GetInterfaces()
                    .Where(i => i.IsGenericType)
                    .Where(i => i == typeof(IPacket<T>))
                    .FirstOrDefault();
                if (packageInterface == null)
                    continue;
                var serializer = Serializer.Get(type);
                if (serializer == null)
                    throw new Exception();
                _serializerByType.Add(type, serializer);
                toCheckDependency.Enqueue(type);
            }

            HashSet<Type> checkedDependency = new HashSet<Type>();
            while(toCheckDependency.Count > 0)
            {
                Type type = toCheckDependency.Dequeue();
                checkedDependency.Add(type);
                foreach (Type dependencyType in Serializer.Dependency(type))
                    if (!checkedDependency.Contains(dependencyType))
                        toCheckDependency.Enqueue(dependencyType);
            }

            Defenition = string.Join(";",
                checkedDependency
                    .Where(t => Serializer.Get(t) != null)
                    .Select(t => (name: Generator.RealTypeName(t), type: t))
                    .OrderBy(t => t.name)
                    .Select(t => $"{t.name}:{{{Serializer.Defenition(t.type)}}}")
            );

            Console.WriteLine(Defenition);
        }*/
