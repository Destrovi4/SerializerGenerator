using System.IO;

namespace Destr.Protocol
{
    public interface ICachedPacketWriter<T> where T : IProtocol<T>
    {
        void WritePackets(BinaryWriter writer);
        void SendPacket<D>(in D packet) where D : IPacket<T>;
    }
}
