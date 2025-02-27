﻿using RT.Common;
using Server.Common;

namespace RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassDME, MediusDmeMessageIds.DataStreamEnd)]
    public class TypeDataStreamEnd : BaseDMEMessage
    {

        public override byte PacketType => (byte)MediusDmeMessageIds.DataStreamEnd;

        public byte Channel;

        public override void Deserialize(Server.Common.Stream.MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            // 
            Channel = reader.ReadByte();
        }

        public override void Serialize(Server.Common.Stream.MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            // 
            writer.Write(Channel);
        }


        public override string ToString()
        {
            return base.ToString() + " " +
                $"Channel: {Channel}";
        }
    }
}