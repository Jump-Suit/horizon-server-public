using RT.Common;
using Server.Common;
using System;

namespace RT.Models
{
    public class RawMediusMessage : BaseMediusMessage
    {

        protected NetMessageClass _class;
        public override NetMessageClass PacketClass => _class;

        protected byte _messageType;
        public override byte PacketType => _messageType;

        public byte[] Contents { get; set; }

        public RawMediusMessage()
        {

        }

        public RawMediusMessage(NetMessageClass msgClass, byte messageType)
        {
            _class = msgClass;
            _messageType = messageType;
        }

        public override void Deserialize(Server.Common.Stream.MessageReader reader)
        {
            Contents = reader.ReadRest();
        }

        public override void Serialize(Server.Common.Stream.MessageWriter writer)
        {
            if (Contents != null)
                writer.Write(Contents);
        }

        public override string ToString()
        {
            return base.ToString() + $" MsgClass:{PacketClass} MsgType:{PacketType} Contents:{BitConverter.ToString(Contents)}";
        }
    }

    public class RawMediusMessage0 : BaseMediusPluginMessage
    {
        protected NetMessageClass _class;
        public override NetMessageClass PacketClass => _class;

        protected NetMessageTypeIds _messageType;
        public override NetMessageTypeIds PacketType => _messageType;

        public byte[] Contents { get; set; }

        public RawMediusMessage0()
        {

        }

        public RawMediusMessage0(NetMessageClass msgClass, NetMessageTypeIds messageType)
        {
            _class = msgClass;
            _messageType = messageType;
        }

        public override void DeserializePlugin(Server.Common.Stream.MessageReader reader)
        {
            Contents = reader.ReadRest();
        }

        public override void SerializePlugin(Server.Common.Stream.MessageWriter writer)
        {
            if (Contents != null)
                writer.Write(Contents);
        }

        public override string ToString()
        {
            return base.ToString() + $" MsgType: {PacketType} Contents:{BitConverter.ToString(Contents)}";
        }
    }
    
    public class RawGHSMediusMessage : BaseMediusGHSMessage
    {
        protected NetMessageClass _class;
        public override NetMessageClass PacketClass => _class;

        protected GhsOpcode _messageType;
        public override GhsOpcode GhsOpcode => _messageType;

        public byte[] Contents { get; set; }

        public RawGHSMediusMessage()
        {

        }

        public RawGHSMediusMessage(NetMessageClass msgClass, GhsOpcode messageType)
        {
            _class = msgClass;
            _messageType = messageType;
        }

        public override void Deserialize(Server.Common.Stream.MessageReader reader)
        {
            Contents = reader.ReadRest();
        }

        public override void Serialize(Server.Common.Stream.MessageWriter writer)
        {
            if (Contents != null)
                writer.Write(Contents);
        }

        public override string ToString()
        {
            return base.ToString() + $" MsgType:{GhsOpcode} Contents:{BitConverter.ToString(Contents)}";
        }
    }
    
}
