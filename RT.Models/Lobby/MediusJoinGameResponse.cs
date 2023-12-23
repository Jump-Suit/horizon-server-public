using RT.Common;
using Server.Common;
using System;
using System.Collections.Generic;

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

        public List<int> approvedMaxPlayersAppIds = new List<int>() { 20371, 20374, 20624, 22500, 22920, 22924, 22930, 24000, 23360, 24180 };

        public override void Deserialize(Server.Common.Stream.MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            //
            MessageID = reader.Read<MessageId>();
            reader.ReadBytes(3);

            //
            StatusCode = reader.Read<MediusCallbackStatus>();
            GameHostType = reader.Read<MediusGameHostType>();
            ConnectInfo = reader.Read<NetConnectionInfo>();

            if (reader.MediusVersion == 113 && approvedMaxPlayersAppIds.Contains(reader.AppId))
            {
                MaxPlayers = reader.ReadInt64();
            }
        }

        public override void Serialize(Server.Common.Stream.MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            //
            writer.Write(MessageID ?? MessageId.Empty);
            writer.Write(new byte[3]);

            // 
            writer.Write(StatusCode);
            writer.Write(GameHostType);
            writer.Write(ConnectInfo);

            
            if (writer.MediusVersion == 113 && approvedMaxPlayersAppIds.Contains(writer.AppId))
            {
                writer.Write(MaxPlayers);
            }
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