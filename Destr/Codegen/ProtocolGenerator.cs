using Destr.Codegen.Source;
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
        public ProtocolSourceGenerator(Type type)
        {
            Name = SimpleName(type);
            Namespace = type.Namespace;

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
            Fields.AddLine("private static readonly Dictionary<Type, uint> _packetIyByType = new Dictionary<Type, uint>();");


            AddMethod("Read").Public.Void.Override.Argument<BinaryReader>("reader").AddLine("_descriptor[reader.ReadUInt16()].Invoke(reader);");

            var listen = AddMethod("Listen<D>").Public.Override.Void.Argument($"PacketListener<{Name}, D> listener");
            var listenSwitch = listen.AddSwitch("_packetIyByType[typeof(D)]");
            listenSwitch.Case.Default.Add("throw new Exception();");

            Require<BinaryReader>();
            Extends.Add(typeof(Protocol<>), type);

            int fieldIndex = 0;
            foreach (var packageType in packageTypes)
            {
                string packageTypeName = SimpleName(packageType);
                string id = $"_{fieldIndex}{packageTypeName}Id";
                string reader = $"P{fieldIndex}{packageTypeName}Reader";
                string serializer = $"_{fieldIndex}{packageTypeName}Serializer";
                string listener = $"_{fieldIndex}{packageTypeName}Listener";

                Fields.Line.Add($"private const uint {id} = {fieldIndex};");

                Fields.Line.Add($"private ").Add(typeof(ISerializer<>), packageType).Add($" {serializer} = null;");
                Fields.Line.Add($"private ").Add(typeof(PacketListener<,>), type, packageType).Add($" {listener} = null;");

                staticConstructor.AddLine($"_packetIyByType.Add(typeof(").Add(packageType).Add($"), {id});");
                constructor.AddLine($"_descriptor[{id}] = {reader};");

                var method = AddMethod(reader).Private.Void.Argument<BinaryReader>("reader");
                method.Line.Add(packageType).Add(" packet = default;");
                method.Line.Add($"{serializer}.Read(ref packet, reader);");
                method.Line.Add($"{listener}(in packet);");

                listenSwitch.AddCase(id).Break.Line.Add($"{listener} = (PacketListener<{Name}, ").Add(packageType).Add(">)(object)listener;");

                fieldIndex++;
            }

            Require(typeof(Action<>));
            Fields.Add($"private Action<BinaryReader>[] _descriptor = new Action<BinaryReader>[{fieldIndex}];");
        }
    }
}
