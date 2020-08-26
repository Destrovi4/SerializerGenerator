using System.IO;

namespace Destr.Protocol
{
    public interface ICachedPacketReader<T> where T : IProtocol<T>
    {
        void ReadPackets(BinaryReader reader);
        bool TryAcceptPacket<D>(out D packet) where D : IPacket<T>;
    }
}
