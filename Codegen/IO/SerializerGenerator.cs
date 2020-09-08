using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Destr.Codegen.Source;
using Destr.IO;


namespace Destr.Codegen
{
    public class SerializerGenerator : ClassSourceGenerator
    {
        private struct GenerationTask
        {
            public string file;
            public string name;
            public string classNamespace;
            public Type dataType;
        }

        private static HashSet<Type> _hasSerializer = new HashSet<Type>();
        
        private HashSet<Type> _requiredSerializerSet = new HashSet<Type>();

        [CodegenMethod]
        public static void GenerateSerializers()
        {
            Queue<GenerationTask> generationTaskQueue = new Queue<GenerationTask>();
            var garantedList = new List<(SerializerGuaranteedAttribute, Type)>();
            foreach (Type type in CodeGenerator.GetTypes())
            {
                Type baseSerializer = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISerializer<>));
                Generated[] generatedTypes = type.GetCustomAttributes<Generated>().ToArray();
                if (generatedTypes != null && generatedTypes.Length > 0)
                {
                    foreach (Generated generated in generatedTypes)
                    {
                        Type serializer = generated?.Argument ?? baseSerializer;
                        if (serializer == null)
                            continue;
                        if (serializer.GetGenericTypeDefinition() != typeof(ISerializer<>))
                            continue;
                        Type dataType = serializer.GetGenericArguments()[0];
                        generationTaskQueue.Enqueue(new GenerationTask() {
                            file = generated.File,
                            name = SimpleName(type),
                            classNamespace = type.Namespace,
                            dataType = dataType
                        });
                    }
                }
                SerializerGuaranteedAttribute guaranteed = type.GetCustomAttribute<SerializerGuaranteedAttribute>();
                if (guaranteed != null && Serializer.Get(type) == null)
                {
                    string directory = Path.GetDirectoryName(guaranteed.File);
                    string file = Path.GetFileNameWithoutExtension(guaranteed.File);
                    string extension = Path.GetExtension(guaranteed.File);

                    var attr = CodeGenerator.GetUsedTypes().Where(t => t.IsGenericType).ToArray();
                    if (type.IsGenericType && type.GetGenericArguments().All(a=>a.IsGenericParameter))
                    {
                        foreach(Type usedType in CodeGenerator.GetUsedTypes().Where(t=>t.IsGenericType && t.GetGenericTypeDefinition() == type.GetGenericTypeDefinition()))
                        {
                            string newUsedClassName = $"{SinpleGenericName(usedType)}Serializer";
                            generationTaskQueue.Enqueue(new GenerationTask()
                            {
                                file = Path.Combine(directory, $"{newUsedClassName}{extension}"),
                                name = newUsedClassName,
                                classNamespace = type.Namespace,
                                dataType = usedType
                            });
                        }
                        continue;
                    }

                    string newClassName = $"{SimpleName(type)}Serializer";

                    generationTaskQueue.Enqueue(new GenerationTask() {
                        file = Path.Combine(directory, $"{newClassName}{extension}"),
                        name = newClassName,
                        classNamespace = type.Namespace,
                        dataType = type
                    });
                }
            }
            var generator = new SerializerGenerator();
            while (generationTaskQueue.Count > 0)
            {
                var task = generationTaskQueue.Dequeue();
                if (_hasSerializer.Contains(task.dataType))
                    continue;
                _hasSerializer.Add(task.dataType);
                generator.Clear();
                generator.Make(task);

                var path = Path.GetDirectoryName(task.file);
                string extension = Path.GetExtension(task.file);
                foreach (Type requiredType in generator._requiredSerializerSet)
                {
                    var className = $"{SimpleName(requiredType)}Serializer";
                    generationTaskQueue.Enqueue(new GenerationTask() {
                        file = Path.Combine(path, $"{className}{extension}"),
                        name = className,
                        classNamespace = requiredType.Namespace,
                        dataType = requiredType
                    });
                }
            }
        }

        public override void Clear()
        {
            _requiredSerializerSet.Clear();
            base.Clear();
        }

        private void Make(GenerationTask task)
        {
            Clear();
            Name = task.name;
            Namespace = task.classNamespace;
            Make(task.dataType);
            Write(task.file);
        }

        public void Make(Type type)
        {
            Attributes.Add<Generated>();
            Extends.Add(typeof(ISerializer<>).MakeGenericType(type));
            var read = AddMethod("Read").Public.Void;
            read.Argument.Add<BinaryReader>().Add("reader");
            read.Argument.Out.Add(type).Add("value");

            var write = AddMethod("Write").Public.Void;
            write.Argument.Add<BinaryWriter>().Add("writer");
            write.Argument.In.Add(type).Add("value");

            int index = 0;
            Dictionary<Type, string> serializerFieldByType = new Dictionary<Type, string>();
            foreach (var field in FindSerializableFields(type))
            {
                Type fieldType = field.FieldType;
                string fieldName = field.Name;

                if (fieldType.IsEnum)
                {
                    read.Line.Add($"value.{fieldName} = (")
                        .Add(fieldType)
                        .Add(")reader.ReadInt32();");
                    write.Add($"writer.Write((int)value.{field.Name});");
                    continue;
                }
                else if (fieldType == typeof(byte))
                    read.Add($"value.{fieldName} = reader.ReadByte();");
                else if (fieldType == typeof(short))
                    read.Add($"value.{fieldName} = reader.ReadInt16();");
                else if (fieldType == typeof(int))
                    read.Add($"value.{fieldName} = reader.ReadInt32();");
                else if (fieldType == typeof(long))
                    read.Add($"value.{fieldName} = reader.ReadInt64();");
                else if (fieldType == typeof(float))
                    read.Add($"value.{fieldName} = reader.ReadSingle();");
                else if (fieldType == typeof(double))
                    read.Add($"value.{fieldName} = reader.ReadDouble();");
                else if (fieldType == typeof(bool))
                    read.Add($"value.{fieldName} = reader.ReadBoolean();");
                else if (fieldType == typeof(string))
                    read.Add($"value.{fieldName} = reader.ReadString();");
                else if (!fieldType.IsValueType)
                {
                    // TODO WARNING AHTUNG

                    read.Line.Add($"value.{fieldName} = (")
                        .Add(fieldType)
                        .Add(")new ")
                        .Add<BinaryFormatter>()
                        .Add("().Deserialize(reader.BaseStream);");
                    write.Line.Add("new ")
                        .Add<BinaryFormatter>()
                        .Add($"().Serialize(writer.BaseStream, value.{fieldName});");
                    continue;
                }
                else
                {
                    if (!serializerFieldByType.TryGetValue(fieldType, out string serFieldName))
                    {
                        serFieldName = $"_ser{index++}{SimpleName(fieldType)}";
                        serializerFieldByType.Add(fieldType, serFieldName);
                        Fields.Line.Add("private static ").Add(typeof(ISerializer<>), fieldType).Add($" {serFieldName} = null;");
                        _requiredSerializerSet.Add(fieldType);
                    }
                    read.Line.Add($"{serFieldName}.Read(reader, out value.{fieldName});");
                    write.Line.Add($"{serFieldName}.Write(writer, in value.{fieldName});");
                    continue;
                }

                write.Add($"writer.Write(value.{field.Name});");
            }
        }

        internal static bool IsSerializableField(FieldInfo field)
        {
            return !(field.IsStatic || field.IsPrivate);
        }

        internal static IEnumerable<FieldInfo> FindSerializableFields(Type type)
        {
            return type.GetFields().Where(IsSerializableField);
        }
    }
}