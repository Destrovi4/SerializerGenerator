using Destr.Codegen;
using SerializerGenerator.Test;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Destr.Codegen
{
    public interface IPacket<T> where T : Protocol<T> { }

    public delegate void PacketListener<T,D>(in D packet) where D : struct, IPacket<T> where T : Protocol<T>;

    public abstract class Protocol<T> where T : Protocol<T>
    {
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
        }

        public void Write<D>(BinaryWriter writer, in D data) where D : struct, IPacket<T>
        {
            var serializer = _serializerByType[data.GetType()] as ISerializer<D>;
            serializer.Write(writer, data);
        }

        public abstract void Read(BinaryReader reader);
        public abstract void Listen<D>(PacketListener<T,D> listener) where D : struct, IPacket<T>;
    }
}
