﻿using Destr.Codegen;
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
        public abstract void Read(BinaryReader reader);
        public abstract void Write<D>(BinaryWriter writer, in D data) where D : struct, IPacket<T>;
        public abstract void Listen<D>(PacketListener<T,D> listener) where D : struct, IPacket<T>;
    }
}
