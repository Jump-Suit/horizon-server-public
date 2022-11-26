using RT.Common;
using Server.Common;

namespace RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassDME, MediusDmeMessageIds.ClientLeaves)]
    public class TypeClientLeaves : BaseDMEMessage
    {

        public override byte PacketType => (byte)MediusDmeMessageIds.ClientLeaves;

        public byte ClientIndex;

        public override void Deserialize(Server.Common.Stream.MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            // 
            ClientIndex = reader.ReadByte();
        }

        public override void Serialize(Server.Common.Stream.MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            // 
            writer.Write(ClientIndex);
        }


        public override string ToString()
        {
            return base.ToString() + " " +
                $"ClientIndex: {ClientIndex}";
        }
    }
}