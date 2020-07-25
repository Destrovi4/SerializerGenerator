using Destr.Codegen;
using System.IO;
using System.Collections.Generic;
using System;
namespace SerializerGenerator.Test
{
    [Generated]
    public class TestProtocol : Protocol<TestProtocol>
    {
        public const string Definition = "Byte:a,TestPacketB:b;Int32:test";
        private static readonly Dictionary<Type, uint> _packetIdByType = new Dictionary<Type, uint>();
        private const uint _0ProtocolPacket0Id = 0;
        private ISerializer<ProtocolPacket0> _0ProtocolPacket0Serializer = null;
        private PacketListener<TestProtocol,ProtocolPacket0> _0ProtocolPacket0Listener = null;
        private const uint _1ProtocolPacket1Id = 1;
        private ISerializer<ProtocolPacket1> _1ProtocolPacket1Serializer = null;
        private PacketListener<TestProtocol,ProtocolPacket1> _1ProtocolPacket1Listener = null;
        private Action<BinaryReader>[] _descriptorRead = new Action<BinaryReader>[2];
        private readonly object[] _descriptorWrite = new object[2];
        delegate void writer<D>(BinaryWriter writer, in D data) where D : struct, IPacket<TestProtocol>;
        static TestProtocol()
        {
            _packetIdByType.Add(typeof(ProtocolPacket0), _0ProtocolPacket0Id);
            _packetIdByType.Add(typeof(ProtocolPacket1), _1ProtocolPacket1Id);
        }
        public TestProtocol()
        {
            _descriptorRead[_0ProtocolPacket0Id] = P0ProtocolPacket0Reader;
            _descriptorWrite[_0ProtocolPacket0Id] = (writer<ProtocolPacket0>)P0ProtocolPacket0Write;
            _descriptorRead[_1ProtocolPacket1Id] = P1ProtocolPacket1Reader;
            _descriptorWrite[_1ProtocolPacket1Id] = (writer<ProtocolPacket1>)P1ProtocolPacket1Write;
        }
        public override void Read(BinaryReader reader)
        {
            _descriptorRead[reader.ReadUInt16()].Invoke(reader);
        }
        public override void Write<D>(BinaryWriter writer, in D data) where D : struct
        {
            (_descriptorWrite[_packetIdByType[typeof(D)]] as writer<D>)(writer, in data);
        }
        public override void Listen<D>(PacketListener<TestProtocol, D> listener)
        {
            switch(_packetIdByType[typeof(D)])
            {
                default:
                    throw new Exception();
                case _0ProtocolPacket0Id:
                    _0ProtocolPacket0Listener = (PacketListener<TestProtocol, ProtocolPacket0>)(object)listener;
                break;
                case _1ProtocolPacket1Id:
                    _1ProtocolPacket1Listener = (PacketListener<TestProtocol, ProtocolPacket1>)(object)listener;
                break;
            }
        }
        private void P0ProtocolPacket0Reader(BinaryReader reader)
        {
            ProtocolPacket0 packet = default;
            _0ProtocolPacket0Serializer.Read(ref packet, reader);
            _0ProtocolPacket0Listener(in packet);
        }
        public void P0ProtocolPacket0Write(BinaryWriter writer, in ProtocolPacket0 packet)
        {
            _0ProtocolPacket0Serializer.Write(writer, in packet);
        }
        private void P1ProtocolPacket1Reader(BinaryReader reader)
        {
            ProtocolPacket1 packet = default;
            _1ProtocolPacket1Serializer.Read(ref packet, reader);
            _1ProtocolPacket1Listener(in packet);
        }
        public void P1ProtocolPacket1Write(BinaryWriter writer, in ProtocolPacket1 packet)
        {
            _1ProtocolPacket1Serializer.Write(writer, in packet);
        }
    }
}
