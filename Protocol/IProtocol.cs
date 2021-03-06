﻿using System;
using System.Collections.Generic;


namespace Destr.Protocol
{
    public delegate void PacketListener<T, D>(in D packet) where D : struct, IPacket<T> where T : IProtocol<T>;


    public interface IProtocol<T> where T : IProtocol<T>
    {
        //ushort GetPacketId(Type packerType);
        string Definition { get; }
        IEnumerable<Type> GetPacketTypes();
        //void Read(BinaryReader reader);
        //void Write<D>(BinaryWriter writer, in D data) where D : struct, IPacket<T>;
        //void Listen<D>(PacketListener<T, D> listener) where D : struct, IPacket<T>;
    }

    /*
    public static class ProtocolDefaults // Protocol => IProtocol
    {
        public static PacketListener<T, D> GetSender<T, D>(this T protocol) where D : struct, IPacket<T> where T : IProtocol<T>
        {
            foreach (var method in protocol.GetType().GetMethods())
            {
                if (!"Write".Equals(method.Name)) continue;
                var parameters = method.GetParameters();
                if (parameters.Length != 1) continue;
                if (!parameters[0].IsIn) continue;
                if (parameters[0].ParameterType == typeof(D)) continue;
                return (PacketListener<T, D>)method.CreateDelegate(typeof(D), protocol);
            }
            throw new Exception();
        }
    }
    */
}