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
        private static readonly Dictionary<Type, uint> _packetIyByType = new Dictionary<Type, uint>();
        private const uint _0ProtocolPacket0Id = 0;
        private ISerializer<ProtocolPacket0> _0ProtocolPacket0Serializer = null;
        private PacketListener<TestProtocol,ProtocolPacket0> _0ProtocolPacket0Listener = null;
        private const uint _1ProtocolPacket1Id = 1;
        private ISerializer<ProtocolPacket1> _1ProtocolPacket1Serializer = null;
        private PacketListener<TestProtocol,ProtocolPacket1> _1ProtocolPacket1Listener = null;
        private Action<BinaryReader>[] _descriptor = new Action<BinaryReader>[2];
        static TestProtocol()
        {
            _packetIyByType.Add(typeof(ProtocolPacket0), _0ProtocolPacket0Id);
            _packetIyByType.Add(typeof(ProtocolPacket1), _1ProtocolPacket1Id);
        }
        public TestProtocol()
        {
            _descriptor[_0ProtocolPacket0Id] = P0ProtocolPacket0Reader;
            _descriptor[_1ProtocolPacket1Id] = P1ProtocolPacket1Reader;
        }
        public override void Read(BinaryReader reader)
        {
            _descriptor[reader.ReadUInt16()].Invoke(reader);
        }
        public override void Listen<D>(PacketListener<TestProtocol, D> listener)
        {
            switch(_packetIyByType[typeof(D)])
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
        private void P1ProtocolPacket1Reader(BinaryReader reader)
        {
            ProtocolPacket1 packet = default;
            _1ProtocolPacket1Serializer.Read(ref packet, reader);
            _1ProtocolPacket1Listener(in packet);
        }
    }
}
