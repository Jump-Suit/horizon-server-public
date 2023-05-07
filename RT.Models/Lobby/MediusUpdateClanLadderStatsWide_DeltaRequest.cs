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
        public byte[] Stats = new byte[Constants.LADDERSTATSWIDE_MAXLEN];

        public override void Deserialize(Server.Common.Stream.MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            //
            MessageID = reader.Read<MessageId>();

            //
            reader.ReadBytes(4);
            Stats = reader.ReadBytes(Constants.LADDERSTATSWIDE_MAXLEN);
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
            writer.Write(Stats, Constants.LADDERSTATSWIDE_MAXLEN);
        }


        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID: {MessageID} " +
                $"ClanId: {ClanId} " +
                $"Stats: {BitConverter.ToString(Stats)}";
        }
    }
}