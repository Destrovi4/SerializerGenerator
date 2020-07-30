using System.IO;

namespace Destr.IO
{
    public interface ISerializer<T> where T : struct
    {
        public void Write(BinaryWriter writer, in T value);
        public void Read(ref T value, BinaryReader reader);
    }
}
