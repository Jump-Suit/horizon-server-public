using RT.Common;
using Server.Common;

namespace RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobbyExt, MediusLobbyExtMessageIds.CroxxChatFwdMessage)]
    public class MediusCrossChatFwdMessage : BaseLobbyExtMessage
    {
        public override byte PacketType => (byte)MediusLobbyExtMessageIds.CroxxChatFwdMessage;

        public MessageId MessageID { get; set; }

        public int OriginatorAccountID;
        public int TargetRoutingDmeWorldID;
        public int SourceDmeWorldID;

        public override void Deserialize(Server.Common.Stream.MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            //
            MessageID = reader.Read<MessageId>();
            reader.ReadBytes(3);

            //
            OriginatorAccountID = reader.ReadInt32();
            TargetRoutingDmeWorldID = reader.ReadInt32();
            SourceDmeWorldID = reader.ReadInt32();
        }

        public override void Serialize(Server.Common.Stream.MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            //
            writer.Write(MessageID ?? MessageId.Empty);
            writer.Write(new byte[3]);

            // 
            writer.Write(OriginatorAccountID);
            writer.Write(TargetRoutingDmeWorldID);
            writer.Write(SourceDmeWorldID);
        }


        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID: {MessageID} " +
                $"OriginatorAccountID: {OriginatorAccountID} " +
                $"TargetRoutingDmeWorldID: {TargetRoutingDmeWorldID} " +
                $"SourceDmeWorldID: {SourceDmeWorldID}";
        }
    }
}
