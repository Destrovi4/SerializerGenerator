﻿#define PRINT_TO_FILE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Destr.Codegen
{
    public class Serializer
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
            var assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes())
            {
                Generated generated = type.GetCustomAttribute<Generated>();
                if (generated == null)
                    continue;
                Type inter = ExtractSerializerInterface(type);
                if (inter == null)
                    continue;
#if PRINT_TO_FILE
                using var writer = new StreamWriter(File.Open(generated.File, FileMode.OpenOrCreate, FileAccess.Write));
                foreach (var line in new Generator(type, inter).GenerateStrings())
                    writer.WriteLine(line);
#else
                Console.WriteLine("File: " + generated.File);
                foreach (var line in new Generator(type, inter).GenerateStrings())
                    Console.WriteLine(line);
#endif
            }
        }
    }
}
