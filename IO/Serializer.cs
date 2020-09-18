#define PRINT_TO_FILE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Destr.Codegen;
using Destr.Codegen.Source;


namespace Destr.IO
{
    public static class Serializer
    {
        private static readonly Dictionary<Type, object> SerializerByType = new Dictionary<Type, object>();

        private static readonly HashSet<Assembly> AssemblyList = new HashSet<Assembly>();

        static Serializer()
        {
            AssemblyList.Add(Assembly.GetExecutingAssembly());
        }

        public static void Init()
        {
            var types = AssemblyList.SelectMany(a => a.GetTypes());
            foreach (Type type in types)
            {
                Type inter = ExtractSerializerInterface(type);
                if (inter == null) continue;
                Type dataType = inter.GenericTypeArguments[0];
                if (SerializerByType.ContainsKey(dataType))
                {
                    Console.WriteLine($"Double definition {dataType}");
                    continue;

                    //TODO переzапись с приоритетом у не генерированной реалиzации
                }
                SerializerByType.Add(dataType, Activator.CreateInstance(type));
            }
            string[] sss = AssemblyList.SelectMany(a => a.GetTypes()).Select(t => t.Name).ToArray();
            List<string> notFound = new List<string>();
            foreach (var type in AssemblyList.SelectMany(a => a.GetTypes()))
            {
                foreach (var field in type.GetTypeInfo().DeclaredFields)
                {
                    if (!field.IsStatic) continue;
                    Type fieldType = field.FieldType;
                    if (!fieldType.IsGenericType) continue;
                    if (fieldType.GetGenericTypeDefinition() != typeof(ISerializer<>)) continue;
                    Type dataType = fieldType.GenericTypeArguments[0];
                    if (SerializerByType.TryGetValue(dataType, out object serializer))
                    {
                        field.SetValue(type, serializer);
                    }
                    else
                    {
                        notFound.Add($"{type.Name}::{field.Name} {fieldType.Name} {dataType.Name}");
                    }
                }
            }

            if (notFound.Count > 0)
            {
                throw new KeyNotFoundException(string.Join("\n", notFound));
            }
        }

        private static Type ExtractSerializerInterface(Type type)
        {
            return type.GetInterfaces()
                .Where(i => i != typeof(Serializer))
                .Where(i => i.IsGenericType)
                .Where(i => i.GetGenericTypeDefinition() == typeof(ISerializer<>))
                .FirstOrDefault();
        }

        public static ISerializer<T> Get<T>() where T : struct
        {
            Type type = typeof(T);
            if (SerializerByType.TryGetValue(type, out object serializer))
                return (ISerializer<T>)serializer;
            throw new Exception($"Not found serizliser for {type}");
        }

        public static object Get(Type type)
        {
            return SerializerByType.TryGetValue(type, out var serializer) ? serializer : null;
        }

        internal static string Definition(Type type)
        {
            return string.Join(",", SerializerGenerator.FindSerializableFields(type)
                .OrderBy(f => f.Name)
                .Select(f => $"{SourceGenerator.RealTypeName(f.FieldType)}:{f.Name}"));
        }

        internal static IEnumerable<Type> Dependency(Type type)
        {
            foreach (FieldInfo field in SerializerGenerator.FindSerializableFields(type))
                if (SerializerByType.ContainsKey(type))
                    yield return field.FieldType;
        }

        public static void AddAssembly(Assembly assembly)
        {
            AssemblyList.Add(assembly);
        }

        public static IEnumerable<Assembly> GetAssemblies()
        {
            return AssemblyList;
        }
    }
}