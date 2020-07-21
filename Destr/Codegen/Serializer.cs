#define PRINT_TO_FILE

using SerializerGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Destr.Codegen
{
    public partial class Serializer
    {
        public static ISerializer<T> Get<T>() where T : struct
        {
            throw new NotImplementedException();
        }
    }

    public partial class Serializer
    {
        private const string Space = "    ";

        public static void Generate()
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes())
            {
                Generated generated = type.GetCustomAttribute<Generated>();
                if (generated == null)
                    continue;
                Type inter = type.GetInterfaces().Where(i => i.GetGenericTypeDefinition() == typeof(ISerializer<>)).First();
                if (inter == null)
                    continue;
#if PRINT_TO_FILE
                using var writer = new StreamWriter(File.Open(generated.File, FileMode.OpenOrCreate, FileAccess.Write));
                foreach (var line in Generate(type, inter))
                    writer.WriteLine(line);
#else
                Console.WriteLine("File: " + generated.File);
                foreach (var line in Generate(type, inter))
                    Console.WriteLine(line);
#endif
            }
        }

        private static IEnumerable<string> Generate(Type type, Type inter)
        {
            HashSet<Type> usingTypes = new HashSet<Type>();
            Type dataType = inter.GenericTypeArguments[0];

            usingTypes.Add(inter);
            usingTypes.Add(dataType);
            usingTypes.Add(typeof(Generated));
            usingTypes.Add(typeof(BinaryReader));
            usingTypes.Add(typeof(BinaryWriter));

            HashSet<string> namespaces = new HashSet<string>();
            foreach (var usingType in usingTypes)
                if (!string.IsNullOrWhiteSpace(usingType.Namespace))
                    namespaces.Add(usingType.Namespace);

            foreach (var usingNamespaces in namespaces)
                if (usingNamespaces != type.Namespace)
                    yield return $"using {usingNamespaces};";

            bool hasNamespace = !string.IsNullOrWhiteSpace(type.Namespace);
            if (hasNamespace)
            {
                yield return $"namespace {type.Namespace}";
                yield return "{";
                foreach (var line in GenerateClass(Space, type, inter, dataType))
                    yield return line;
                yield return "}";
            }
            else
            {
                foreach (var line in GenerateClass("", type, inter, dataType))
                    yield return line;
            }
            yield break;
        }

        private static IEnumerable<string> GenerateClass(string offset, Type type, Type inter, Type dataType)
        {
            yield return $"{offset}[{nameof(Generated)}]";
            yield return $"{offset}public class {RealTypeName(type)} : {RealTypeName(inter)}";
            yield return $"{offset}{{";
            foreach (var line in GenerateReader($"{offset}{Space}", type, dataType))
                yield return line;
            foreach (var line in GenerateWriter($"{offset}{Space}", type, dataType))
                yield return line;
            yield return $"{offset}}}";
            yield break;
        }

        private static IEnumerable<string> GenerateReader(string offset, Type type, Type dataType)
        {
            yield return $"{offset}public void Read(ref {RealTypeName(dataType)} value, BinaryReader reader)";
            yield return $"{offset}{{";
            foreach (var field in dataType.GetFields())
            {
                if (field.IsStatic || field.IsPrivate)
                    continue;
                yield return $"{offset}{Space}{GenerateFieldReader(field)};";
            }
            yield return $"{offset}}}";
            yield break;
        }

        private static IEnumerable<string> GenerateWriter(string offset, Type type, Type dataType)
        {
            yield return $"{offset}public void Write(BinaryWriter writer, in {RealTypeName(dataType)} value)";
            yield return $"{offset}{{";
            foreach (var field in dataType.GetFields())
            {
                if (field.IsStatic || field.IsPrivate)
                    continue;
                yield return $"{offset}{Space}{GenerateFieldWriter(field)};";
            }
            yield return $"{offset}}}";
            yield break;
        }

        private static string GenerateFieldReader(FieldInfo field)
        {
            if (field.FieldType == typeof(byte)) return $"value.{field.Name} = reader.ReadByte()";
            if (field.FieldType == typeof(short)) return $"value.{field.Name} = reader.ReadInt16()";
            if (field.FieldType == typeof(int)) return $"value.{field.Name} = reader.ReadInt32()";
            if (field.FieldType == typeof(long)) return $"value.{field.Name} = reader.ReadInt64()";
            if (field.FieldType == typeof(float)) return $"value.{field.Name} = reader.ReadSingle()";
            if (field.FieldType == typeof(double)) return $"value.{field.Name} = reader.ReadDouble()";
            if (field.FieldType == typeof(bool)) return $"value.{field.Name} = reader.ReadBoolean()";
            throw new Exception(field.FieldType + " Not supported");
        }

        private static string GenerateFieldWriter(FieldInfo field)
        {
            return $"writer.Write(value.{field.Name})";
        }

        private static string RealTypeName(Type type)
        {
            var name = type.Name;
            if (!type.IsGenericType) return name;
            var sb = new StringBuilder();
            sb.Append(name.Substring(0, name.IndexOf('`')));
            sb.Append("<");
            sb.Append(string.Join(", ", type.GetGenericArguments().Select(t => RealTypeName(t))));
            sb.Append(">");
            return sb.ToString();
        }
    }
}
