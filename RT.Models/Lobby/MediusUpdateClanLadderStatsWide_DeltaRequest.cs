using RT.Common;
using Server.Common;
using System;

namespace RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobbyExt, MediusLobbyExtMessageIds.UpdateClanLadderStatsWide_Delta)]
    public class MediusUpdateClanLadderStatsWide_DeltaRequest : BaseLobbyMessage, IMediusRequest
    {

        public override byte PacketType => (byte)MediusLobbyExtMessageIds.UpdateClanLadderStatsWide_Delta;

        public MessageId MessageID { get; set; }

        /// <summary>
        /// Clan Id to Update
        /// </summary>
        public int ClanId; 
        /// <summary>
        /// Total set of wide stats to update the clan with
        /// </summary>
        public int[] Stats = new int[Constants.LADDERSTATSWIDE_MAXLEN];

        public override void Deserialize(Server.Common.Stream.MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            //
            MessageID = reader.Read<MessageId>();

            //
            reader.ReadBytes(4);
            for (int i = 0; i < Constants.LADDERSTATSWIDE_MAXLEN; ++i) { Stats[i] = reader.ReadInt32(); }
        }

        public override void Serialize(Server.Common.Stream.MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            //
            writer.Write(MessageID ?? MessageId.Empty);

            //
            writer.Write(new byte[4]);
            writer.Write(ClanId);
            for (int i = 0; i < Constants.LADDERSTATS_MAXLEN; ++i) { writer.Write(i >= Stats.Length ? 0 : Stats[i]); }
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID: {MessageID} " +
                $"ClanId: {ClanId} " +
                $"DeltaStats: {Stats}";
        }
    }
}