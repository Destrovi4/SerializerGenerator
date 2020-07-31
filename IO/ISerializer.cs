using System.IO;

namespace Destr.IO
{
    public interface ISerializer<T> where T : struct
    {
        void Write(BinaryWriter writer, in T value);
        void Read(ref T value, BinaryReader reader);
    }
}
