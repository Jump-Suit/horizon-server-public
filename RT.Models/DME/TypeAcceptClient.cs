using RT.Common;
using Server.Common;

namespace RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassDME, MediusDmeMessageIds.AcceptClient)]
    public class TypeAcceptClient : BaseDMEMessage
    {

        public override byte PacketType => (byte)MediusDmeMessageIds.AcceptClient;

        public short NetObjectBufferStart;
        public short NetDataStreamStart;

        public override void Deserialize(Server.Common.Stream.MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            // 
            NetObjectBufferStart = reader.ReadInt16();
            NetDataStreamStart = reader.ReadInt16();
        }

        public override void Serialize(Server.Common.Stream.MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            // 
            writer.Write(NetObjectBufferStart);
            writer.Write(NetDataStreamStart);
        }


        public override string ToString()
        {
            return base.ToString() + " " +
                $"NetObjectBufferStart: {NetObjectBufferStart} " +
                $"NetDataStreamStart: {NetDataStreamStart}";
        }
    }
}