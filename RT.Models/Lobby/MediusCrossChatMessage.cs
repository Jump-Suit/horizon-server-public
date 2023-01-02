using RT.Common;
using Server.Common;

namespace RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobbyExt, MediusLobbyExtMessageIds.CrossChatMessage)]
    public class MediusCrossChatMessage : BaseLobbyExtMessage, IMediusRequest
    {
        public override byte PacketType => (byte)MediusLobbyExtMessageIds.CrossChatMessage;

        public MessageId MessageID { get; set; }

        public string SessionKey;
        public int TargetAccountID;
        public int TargetRoutingDmeWorldID;
        public int SourceDmeWorldID;
        public ushort msgType;



        public override void Deserialize(Server.Common.Stream.MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            //
            MessageID = reader.Read<MessageId>();

            // 
            SessionKey = reader.ReadString(Constants.SESSIONKEY_MAXLEN);
            reader.ReadBytes(2);
            TargetAccountID = reader.ReadInt32();
            TargetRoutingDmeWorldID = reader.ReadInt32();
            SourceDmeWorldID = reader.ReadInt32();

            msgType = reader.ReadByte();
        }

        public override void Serialize(Server.Common.Stream.MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            //
            writer.Write(MessageID ?? MessageId.Empty);

            // 
            writer.Write(SessionKey, Constants.SESSIONKEY_MAXLEN);
            writer.Write(new byte[2]);
            writer.Write(TargetAccountID);
            writer.Write(TargetRoutingDmeWorldID);
            writer.Write(SourceDmeWorldID);

            writer.Write(msgType);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID:{MessageID} " +
                $"SessionKey:{SessionKey} " +
                $"TargetAccountID:{TargetAccountID} " +
                $"TargetRoutingDmeWorldID:{TargetRoutingDmeWorldID} " +
                $"SourceDmeWorldID:{SourceDmeWorldID} " + 
                $"msgType: {msgType} ";
        }
    }
}