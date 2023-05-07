using RT.Common;
using Server.Common;

namespace RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobby, MediusLobbyMessageIds.JoinGameResponse)]
    public class MediusJoinGameResponse : BaseLobbyMessage, IMediusResponse
    {
        public override byte PacketType => (byte)MediusLobbyMessageIds.JoinGameResponse;

        public bool IsSuccess => StatusCode >= 0;

        /// <summary>
        /// Message ID
        /// </summary>
        public MessageId MessageID { get; set; }

        public MediusCallbackStatus StatusCode;
        public MediusGameHostType GameHostType;
        public NetConnectionInfo ConnectInfo;
        /// <summary>
        /// MaxPlayers
        /// </summary>
        public long MaxPlayers;

        public override void Deserialize(Server.Common.Stream.MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            //
            MessageID = reader.Read<MessageId>();

            //

            reader.ReadBytes(3);

            StatusCode = reader.Read<MediusCallbackStatus>();
            GameHostType = reader.Read<MediusGameHostType>();
            ConnectInfo = reader.Read<NetConnectionInfo>();

            if (reader.MediusVersion == 113 && reader.AppId == 24180 || reader.AppId == 24000 || reader.AppId == 22500 || reader.AppId == 22920)
                MaxPlayers = reader.ReadInt64();
        }

        public override void Serialize(Server.Common.Stream.MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            //
            writer.Write(MessageID ?? MessageId.Empty);

            // 
            writer.Write(new byte[3]);

            writer.Write(StatusCode);
            writer.Write(GameHostType);
            writer.Write(ConnectInfo);

            if (writer.MediusVersion == 113 /*&& writer.AppId == 24180 || writer.AppId == 24000 || writer.AppId == 22500 || writer.AppId == 22920*/)
                writer.Write(MaxPlayers);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID: {MessageID} " +
                $"StatusCode: {StatusCode} " +
                $"GameHostType: {GameHostType} " +
                $"ConnectInfo: {ConnectInfo} " +
                $"MaxPlayers: {MaxPlayers}";
        }
    }
}