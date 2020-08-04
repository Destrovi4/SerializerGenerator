using System;
using System.IO;

namespace Destr.Protocol
{
    public delegate void PacketListener<T, D>(in D packet) where D : struct, IPacket<T> where T : Protocol<T>;
    public abstract class Protocol<T> where T : Protocol<T>
    {
        public abstract void Read(BinaryReader reader);
        public abstract void Write<D>(BinaryWriter writer, in D data) where D : struct, IPacket<T>;
        public abstract void Listen<D>(PacketListener<T, D> listener) where D : struct, IPacket<T>;
    }

    public static class ProtocolDefaults // Protocol => IProtocol
    {
        public static PacketListener<T, D> GetSender<T, D>(this T protocol) where D : struct, IPacket<T> where T : Protocol<T>
        {
            foreach (var method in protocol.GetType().GetMethods())
            {
                if (!"Write".Equals(method.Name))
                    continue;
                var parametrs = method.GetParameters();
                if (parametrs.Length != 1)
                    continue;
                if (!parametrs[0].IsIn)
                    continue;
                if (parametrs[0].ParameterType == typeof(D))
                    continue;
                return (PacketListener<T, D>)method.CreateDelegate(typeof(D), protocol);
            }
            throw new Exception();
        }
    }
}