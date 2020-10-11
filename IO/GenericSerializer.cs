using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Destr.IO
{
    public class GenericSerializer<T> : ISerializer, ISerializer<T> where T : struct
    {
        private delegate void Writer(BinaryWriter writer, object data);
        private delegate void Reader(BinaryReader reader, object data);

        private Writer[] _writers;
        private Reader[] _readers;

        public GenericSerializer() {
            Type type = typeof(T);
            List<Writer> writerList = new List<Writer>();
            List<Reader> readerList = new List<Reader>();
            foreach(FieldInfo fieldInfo in type.GetFields()) {
                if(fieldInfo.IsStatic)
                    continue;
                Type fieldTyle = fieldInfo.GetType();
                if(fieldTyle == typeof(bool)) {
                    readerList.Add((r, d) => fieldInfo.SetValue(d, r.ReadBoolean()));
                    writerList.Add((w, d) => w.Write((bool)fieldInfo.GetValue(d)));
                } else if(fieldTyle == typeof(byte)) {
                    readerList.Add((r, d) => fieldInfo.SetValue(d, r.ReadByte()));
                    writerList.Add((w, d) => w.Write((byte)fieldInfo.GetValue(d)));
                } else if(fieldTyle == typeof(char)) {
                    readerList.Add((r, d) => fieldInfo.SetValue(d, r.ReadChar()));
                    writerList.Add((w, d) => w.Write((char)fieldInfo.GetValue(d)));
                } else if(fieldTyle == typeof(decimal)) {
                    readerList.Add((r, d) => fieldInfo.SetValue(d, r.ReadDecimal()));
                    writerList.Add((w, d) => w.Write((decimal)fieldInfo.GetValue(d)));
                } else if(fieldTyle == typeof(double)) {
                    readerList.Add((r, d) => fieldInfo.SetValue(d, r.ReadDouble()));
                    writerList.Add((w, d) => w.Write((double)fieldInfo.GetValue(d)));
                } else if(fieldTyle == typeof(short)) {
                    readerList.Add((r, d) => fieldInfo.SetValue(d, r.ReadInt16()));
                    writerList.Add((w, d) => w.Write((short)fieldInfo.GetValue(d)));
                } else if(fieldTyle == typeof(int)) {
                    readerList.Add((r, d) => fieldInfo.SetValue(d, r.ReadInt32()));
                    writerList.Add((w, d) => w.Write((int)fieldInfo.GetValue(d)));
                } else if(fieldTyle == typeof(long)) {
                    readerList.Add((r, d) => fieldInfo.SetValue(d, r.ReadInt64()));
                    writerList.Add((w, d) => w.Write((long)fieldInfo.GetValue(d)));
                } else if(fieldTyle == typeof(sbyte)) {
                    readerList.Add((r, d) => fieldInfo.SetValue(d, r.ReadSByte()));
                    writerList.Add((w, d) => w.Write((sbyte)fieldInfo.GetValue(d)));
                } else if(fieldTyle == typeof(string)) {
                    readerList.Add((r, d) => fieldInfo.SetValue(d, r.ReadString()));
                    writerList.Add((w, d) => w.Write((string)fieldInfo.GetValue(d)));
                } else if(fieldTyle == typeof(ushort)) {
                    readerList.Add((r, d) => fieldInfo.SetValue(d, r.ReadUInt16()));
                    writerList.Add((w, d) => w.Write((ushort)fieldInfo.GetValue(d)));
                } else if(fieldTyle == typeof(uint)) {
                    readerList.Add((r, d) => fieldInfo.SetValue(d, r.ReadUInt16()));
                    writerList.Add((w, d) => w.Write((uint)fieldInfo.GetValue(d)));
                } else if(fieldTyle == typeof(ulong)) {
                    readerList.Add((r, d) => fieldInfo.SetValue(d, r.ReadUInt64()));
                    writerList.Add((w, d) => w.Write((ulong)fieldInfo.GetValue(d)));
                } else {


                    //Serializer.Get()
                }
            }

            _writers = writerList.ToArray();
            _readers = readerList.ToArray();
        }

        public void Write(BinaryWriter writer, in T value)
        {
            Write(writer, (object)value);
        }

        public void Read(BinaryReader reader, out T value)
        {
            object container = new T();
            Read(reader, container);
            value = (T)container;
        }

        public void Write(BinaryWriter writer, object value)
        {
            foreach(Writer fieldWriter in _writers)
            {
                fieldWriter(writer, value);
            }
        }

        public void Read(BinaryReader reader, object value)
        {
            foreach(Reader fieldReader in _readers)
            {
                fieldReader(reader, value);
            }
        }
    }
}