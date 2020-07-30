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
}