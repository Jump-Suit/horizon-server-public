using RT.Common;
using Server.Common;

namespace RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobbyExt, MediusLobbyExtMessageIds.MatchPartyRequest)]
    public class MediusMatchPartyRequest : BaseLobbyExtMessage, IMediusRequest
    {

        public override byte PacketType => (byte)MediusLobbyExtMessageIds.MatchPartyRequest;

        public MessageId MessageID { get; set; }
        public string SessionKey; // SESSIONKEY_MAXLEN
        public int MediusJoinType;
        public int Unk2;
        public int Unk3;
        public int Unk4;
        public uint MinPlayers;
        public uint MaxPlayers;
        public int Unk7;
        public int MatchType;
        public int Unk9;
        public int SearchId;

        public override void Deserialize(Server.Common.Stream.MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            //
            MessageID = reader.Read<MessageId>();

            //
            SessionKey = reader.ReadString(Constants.SESSIONKEY_MAXLEN);
            MediusJoinType = reader.ReadInt32();
            Unk2 = reader.ReadInt32();
            Unk3 = reader.ReadInt32();
            Unk4 = reader.ReadInt32();
            MinPlayers = reader.ReadUInt32();
            MaxPlayers = reader.ReadUInt32();
            Unk7 = reader.ReadInt32();
            MatchType = reader.ReadInt32();
            Unk9 = reader.ReadInt32();
            SearchId = reader.ReadInt32();
        }

        public override void Serialize(Server.Common.Stream.MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            //
            writer.Write(MessageID ?? MessageId.Empty);

            //
            writer.Write(SessionKey);
            writer.Write(MediusJoinType);
            writer.Write(Unk2);
            writer.Write(Unk3);
            writer.Write(Unk4);
            writer.Write(MinPlayers);
            writer.Write(MaxPlayers);
            writer.Write(Unk7);
            writer.Write(MatchType);
            writer.Write(Unk9);
            writer.Write(SearchId);
        }


        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID: {MessageID} " +
                $"SessionKey: {SessionKey} " +
                $"MediusJoinType : {MediusJoinType} " +
                $"Unk2: {Unk2} " +
                $"Unk3: {Unk3} " +
                $"Unk4: {Unk4} " +
                $"MinPlayers: {MinPlayers} " +
                $"MaxPlayers: {MaxPlayers} " +
                $"Unk7: {Unk7} " +
                $"MatchType: {MatchType} " +
                $"Unk9: {Unk9} " +
                $"SearchId: {SearchId} ";
        }
    }
}