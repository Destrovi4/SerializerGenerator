#define PRINT_TO_FILE

using SerializerGenerator.Destr.Codegen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Destr.Codegen
{
    public static class Serializer
    {
        private static readonly Dictionary<Type, object> SerializerByType = new Dictionary<Type, object>();

        static Serializer()
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes())
            {
                Type inter = ExtractSerializerInterface(type);
                if (inter == null)
                    continue;
                Type dataType = inter.GenericTypeArguments[0];
                if(SerializerByType.ContainsKey(dataType))
                {
                    Console.WriteLine("Duble diffinition " + dataType);
                    continue;
                    //TODO переzапись с приоритетом у не генерированной реалиzации
                }
                SerializerByType.Add(dataType, Activator.CreateInstance(type));
            }

            foreach (var instance in SerializerByType.Values)
            {
                Type type = instance.GetType();
                foreach (var field in type.GetTypeInfo().DeclaredFields)
                {
                    Type fieldType = field.FieldType;
                    if (fieldType.GetGenericTypeDefinition() != typeof(ISerializer<>))
                        continue;
                    Type dataType = fieldType.GenericTypeArguments[0];
                    field.SetValue(instance, SerializerByType[dataType]);
                }
            }
        }

        private static Type ExtractSerializerInterface(Type type)
        {
            return type.GetInterfaces()
                .Where(i => i != typeof(Serializer))
                .Where(i=> i.IsGenericType)
                .Where(i => i.GetGenericTypeDefinition() == typeof(ISerializer<>))
                .FirstOrDefault();
        }

        public static ISerializer<T> Get<T>() where T : struct
        {
            return (ISerializer<T>)SerializerByType[typeof(T)];
        }

        public static object Get(Type type)
        {
            return SerializerByType.TryGetValue(type, out var serializer) ? serializer : null;
        }

        public static void Generate()
        {
            List<(Type, SerializerGaranted)> serializerGaranted = new List<(Type, SerializerGaranted)>();

            var assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes())
            {
                SerializerGaranted garanted = type.GetCustomAttribute<SerializerGaranted>();
                if (garanted != null)
                {
                    serializerGaranted.Add((type, garanted));
                    continue;
                }

                Generated generated = type.GetCustomAttribute<Generated>();
                if (generated == null)
                    continue;
                Type inter = ExtractSerializerInterface(type);
                if (inter == null)
                    continue;

                var dataType = inter.GenericTypeArguments[0];
                var generator = new Generator(type, dataType);
                Generate(generated.File, in generator);
            }

            foreach ((Type type, SerializerGaranted garanted) in serializerGaranted)
            {
                if (SerializerByType.ContainsKey(type))
                    continue;

                string directory = Path.GetDirectoryName(garanted.File);
                string file = Path.GetFileNameWithoutExtension(garanted.File);
                string extension = Path.GetExtension(garanted.File);

                string newClassName = $"{Generator.SimpleName(type)}Serializer";
                string generatedFileName = Path.Combine(directory, $"{newClassName}{extension}");

                var generator = new Generator(type.Namespace, newClassName, type);
                Generate(generatedFileName, in generator);
            }
        }

        private static void Generate(string file, in Generator generator)
        {
#if PRINT_TO_FILE
            using var writer = new StreamWriter(File.Open(file, FileMode.OpenOrCreate, FileAccess.Write));
            foreach (var line in generator.GenerateStrings())
                writer.WriteLine(line);
#else
                Console.WriteLine("File: " + file);
                foreach (var line in generator.GenerateStrings())
                    Console.WriteLine(line);
#endif
        }

        internal static string Defenition(Type type)
        {
            return string.Join(",", Generator.FindSerializebleFields(type)
                .OrderBy(f => f.Name)
                .Select(f => $"{Generator.RealTypeName(f.FieldType)}:{f.Name}"));
        }

        internal static IEnumerable<Type> Dependency(Type type)
        {
            foreach (FieldInfo field in Generator.FindSerializebleFields(type))
                if (SerializerByType.ContainsKey(type))
                    yield return field.FieldType;
        }
    }
}
