using Destr.Codegen.Source;
using Destr.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Destr.Codegen
{
    public class SerializerGenerator : ClassSourceGenerator, ICodeGenerator
    {
        public void Generate() => Generate(Assembly.GetExecutingAssembly());

        public void Generate(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                Generated generated = type.GetCustomAttribute<Generated>();
                Type serializer = type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISerializer<>)).FirstOrDefault();
                if(generated != null && serializer != null)
                {
                    var dataType = serializer.GetGenericArguments()[0];
                    Name = SimpleName(type);
                    Namespace = type.Namespace;
                    Clear();
                    Make(dataType);
                    Write(generated.File);
                }
                
                SerializerGaranted garanted = type.GetCustomAttribute<SerializerGaranted>();
                if(garanted != null && Serializer.Get(type) == null)
                {
                    string directory = Path.GetDirectoryName(garanted.File);
                    string file = Path.GetFileNameWithoutExtension(garanted.File);
                    string extension = Path.GetExtension(garanted.File);

                    string newClassName = $"{SimpleName(type)}Serializer";
                    string generatedFileName = Path.Combine(directory, $"{newClassName}{extension}");

                    Name = newClassName;
                    Namespace = type.Namespace;
                    Clear();
                    Make(type);
                    Write(generatedFileName);
                }

            }
        }

        public void Make(Type type)
        {
            Attributes.Add<Generated>();
            Extends.Add(typeof(ISerializer<>).MakeGenericType(type));
            var read = AddMethod("Read").Public.Void;
            read.Argument.Ref.Add(type).Add("value");
            read.Argument.Add<BinaryReader>().Add("reader");

            var write = AddMethod("Write").Public.Void;
            write.Argument.Add<BinaryWriter>().Add("writer");
            write.Argument.In.Add(type).Add("value");

            int index = 0;
            Dictionary<Type, string> serializerFieldByType = new Dictionary<Type, string>();
            foreach (var field in FindSerializebleFields(type))
            {
                Type fieldType = field.FieldType;
                string fieldName = field.Name;
                object serializer = Serializer.Get(fieldType);
                if (serializer != null)
                {
                    if (!serializerFieldByType.TryGetValue(fieldType, out string serFieldName))
                    {
                        serFieldName = $"_ser{index++}{SimpleName(fieldType)}";
                        serializerFieldByType.Add(fieldType, serFieldName);
                        Fields.Line.Add("private ").Add(typeof(ISerializer<>), fieldType).Add($" {serFieldName} = null;");
                    }
                    read.Line.Add($"{serFieldName}.Read(ref value.{fieldName}, reader);");
                    write.Line.Add($"{serFieldName}.Write(writer, in value.{fieldName});");
                    continue;
                }

                if (fieldType == typeof(byte)) read.Add($"value.{fieldName} = reader.ReadByte();");
                else if (fieldType == typeof(short)) read.Add($"value.{fieldName} = reader.ReadInt16();");
                else if (fieldType == typeof(int)) read.Add($"value.{fieldName} = reader.ReadInt32();");
                else if (fieldType == typeof(long)) read.Add($"value.{fieldName} = reader.ReadInt64();");
                else if (fieldType == typeof(float)) read.Add($"value.{fieldName} = reader.ReadSingle();");
                else if (fieldType == typeof(double)) read.Add($"value.{fieldName} = reader.ReadDouble();");
                else if (fieldType == typeof(bool)) read.Add($"value.{fieldName} = reader.ReadBoolean();");
                else if (fieldType == typeof(string)) read.Add($"value.{fieldName} = reader.ReadString();");
                else throw new Exception($"{fieldType} not supported for field {fieldName}");

                write.Add($"writer.Write(value.{field.Name});");
            }
        }

        internal static bool IsSerializebleField(FieldInfo field) => !(field.IsStatic || field.IsPrivate);
        internal static IEnumerable<FieldInfo> FindSerializebleFields(Type type) => type.GetFields().Where(IsSerializebleField);
    }
}
