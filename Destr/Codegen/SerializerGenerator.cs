using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Destr.Codegen
{
    struct Generator
    {
        private const string Space = "    ";

        private readonly Type _dataType;
        private readonly string _namespace;
        private readonly string _className;

        private readonly HashSet<Type> _usingTypes;
        private readonly Dictionary<Type, string> _serializerFieldByType;

        public Generator(Type type, Type dataType) : this(type.Namespace, RealTypeName(type), dataType)
        {
        }

        public Generator(string ns, string name, Type dataType)
        {
            _namespace = ns;
            _className = name;
            _dataType = dataType;
            _usingTypes = new HashSet<Type>();

            _usingTypes.Add(_dataType);
            _usingTypes.Add(typeof(ISerializer<>));


            _usingTypes.Add(typeof(Generated));
            _usingTypes.Add(typeof(BinaryReader));
            _usingTypes.Add(typeof(BinaryWriter));

            _serializerFieldByType = new Dictionary<Type, string>();
            var fields = _dataType.GetFields();
            int index = 0;
            foreach (var field in _dataType.GetFields())
            {
                if (!IsSerializebleField(field))
                    continue;
                var fieldType = field.FieldType;
                if (_serializerFieldByType.ContainsKey(fieldType))
                    continue;
                object serializer = Serializer.Get(fieldType);
                if (serializer == null)
                    continue;
                _usingTypes.Add(fieldType);
                _serializerFieldByType.Add(field.FieldType, $"_ser{index++}{SimpleName(fieldType)}");
            }
        }

        private bool IsSerializebleField(FieldInfo field) => !(field.IsStatic || field.IsPrivate);

        public IEnumerable<string> GenerateStrings()
        {
            HashSet<string> namespaces = new HashSet<string>();
            foreach (var usingType in _usingTypes)
                if (!string.IsNullOrWhiteSpace(usingType.Namespace))
                    namespaces.Add(usingType.Namespace);

            foreach (var usingNamespaces in namespaces)
                if (usingNamespaces != _namespace)
                    yield return $"using {usingNamespaces};";

            bool hasNamespace = !string.IsNullOrWhiteSpace(_namespace);
            if (hasNamespace)
            {
                yield return $"namespace {_namespace}";
                yield return "{";
                foreach (var line in GenerateClass(Space))
                    yield return line;
                yield return "}";
            }
            else
            {
                foreach (var line in GenerateClass(""))
                    yield return line;
            }
            yield break;
        }

        private IEnumerable<string> GenerateClass(string offset)
        {
            yield return $"{offset}[{nameof(Generated)}]";
            yield return $"{offset}public class {_className} : {SimpleName(typeof(ISerializer<>))}<{RealTypeName(_dataType)}>";
            yield return $"{offset}{{";
            
            string interName = SimpleName(typeof(ISerializer<>));
            foreach (var entry in _serializerFieldByType)
                yield return $"{offset}{Space}private {interName}<{RealTypeName(entry.Key)}> {entry.Value} = null;";

            foreach (var line in GenerateReader($"{offset}{Space}"))
                yield return line;
            foreach (var line in GenerateWriter($"{offset}{Space}"))
                yield return line;
            yield return $"{offset}}}";
            yield break;
        }

        private IEnumerable<string> GenerateReader(string offset)
        {
            yield return $"{offset}public void Read(ref {RealTypeName(_dataType)} value, BinaryReader reader)";
            yield return $"{offset}{{";
            foreach (var field in _dataType.GetFields())
            {
                if (!IsSerializebleField(field))
                    continue;
                yield return $"{offset}{Space}{GenerateFieldReader(field)};";
            }
            yield return $"{offset}}}";
            yield break;
        }

        private IEnumerable<string> GenerateWriter(string offset)
        {
            yield return $"{offset}public void Write(BinaryWriter writer, in {RealTypeName(_dataType)} value)";
            yield return $"{offset}{{";
            foreach (var field in _dataType.GetFields())
            {
                if (!IsSerializebleField(field))
                    continue;
                yield return $"{offset}{Space}{GenerateFieldWriter(field)};";
            }
            yield return $"{offset}}}";
            yield break;
        }

        private string GenerateFieldReader(FieldInfo field)
        {
            Type fieldType = field.FieldType;
            if(_serializerFieldByType.TryGetValue(fieldType, out var serFieldName))
                return $"{serFieldName}.Read(ref value.{field.Name}, reader)";

            if (fieldType == typeof(byte)) return $"value.{field.Name} = reader.ReadByte()";
            if (fieldType == typeof(short)) return $"value.{field.Name} = reader.ReadInt16()";
            if (fieldType == typeof(int)) return $"value.{field.Name} = reader.ReadInt32()";
            if (fieldType == typeof(long)) return $"value.{field.Name} = reader.ReadInt64()";
            if (fieldType == typeof(float)) return $"value.{field.Name} = reader.ReadSingle()";
            if (fieldType == typeof(double)) return $"value.{field.Name} = reader.ReadDouble()";
            if (fieldType == typeof(bool)) return $"value.{field.Name} = reader.ReadBoolean()";
            throw new Exception(fieldType + " Not supported");
        }

        private string GenerateFieldWriter(FieldInfo field)
        {
            Type fieldType = field.FieldType;
            if (_serializerFieldByType.TryGetValue(fieldType, out var serFieldName))
                return $"{serFieldName}.Write(writer, in value.{field.Name})";

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

        private static string FieldName(Type type)
        {
            return type.FullName.Replace('`', '_').Replace('.', '_');
        }

        public static string SimpleName(Type type)
        {
            var name = type.Name;
            if (!type.IsGenericType) return name;
            return name.Substring(0, name.IndexOf('`'));
        }
    }
}
