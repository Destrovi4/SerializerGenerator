#define PRINT_TO_FILE

using Destr.Codegen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
                if (inter == null)
                    continue;
                Type dataType = inter.GenericTypeArguments[0];
                if (SerializerByType.ContainsKey(dataType))
                {
                    Console.WriteLine("Duble diffinition " + dataType);
                    continue;
                    //TODO переzапись с приоритетом у не генерированной реалиzации
                }
                SerializerByType.Add(dataType, Activator.CreateInstance(type));
            }
            string[] sss = AssemblyList.SelectMany(a => a.GetTypes()).Select(t=>t.Name).ToArray();
            foreach (var type in AssemblyList.SelectMany(a => a.GetTypes()))
            {
                foreach (var field in type.GetTypeInfo().DeclaredFields)
                {
                    if (!field.IsStatic)
                        continue;
                    Type fieldType = field.FieldType;
                    if (!fieldType.IsGenericType)
                        continue;
                    if (fieldType.GetGenericTypeDefinition() != typeof(ISerializer<>))
                        continue;
                    Type dataType = fieldType.GenericTypeArguments[0];
                    field.SetValue(type, SerializerByType[dataType]);
                }
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
            return (ISerializer<T>)SerializerByType[typeof(T)];
        }

        public static object Get(Type type)
        {
            return SerializerByType.TryGetValue(type, out var serializer) ? serializer : null;
        }

        internal static string Defenition(Type type)
        {
            return string.Join(",", SerializerGenerator.FindSerializebleFields(type)
                .OrderBy(f => f.Name)
                .Select(f => $"{SerializerGenerator.RealTypeName(f.FieldType)}:{f.Name}"));
        }

        internal static IEnumerable<Type> Dependency(Type type)
        {
            foreach (FieldInfo field in SerializerGenerator.FindSerializebleFields(type))
                if (SerializerByType.ContainsKey(type))
                    yield return field.FieldType;
        }

        public static void AddAssembly(Assembly assembly) => AssemblyList.Add(assembly);

        public static IEnumerable<Assembly> GetAssemblys() => AssemblyList;
    }
}
