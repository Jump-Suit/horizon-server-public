using RT.Common;
using Server.Common;

namespace RT.Models
{
    /// <summary>
    /// Version string of currently connected Medius Server
    /// </summary>
    [MediusMessage(NetMessageClass.MessageClassLobby, MediusLobbyMessageIds.VersionServerResponse)]
    public class MediusVersionServerResponse : BaseLobbyMessage, IMediusResponse
    {
        public override byte PacketType => (byte)MediusLobbyMessageIds.VersionServerResponse;

        public bool IsSuccess => StatusCode >= 0;
        public MediusCallbackStatus StatusCode;

        /// <summary>
        /// Message ID
        /// </summary>
        public MessageId MessageID { get; set; }
        /// <summary>
        /// Server version string, including null termination
        /// </summary>
        public string VersionServer; // VERSIONSERVER_MAXLEN

        public override void Deserialize(Server.Common.Stream.MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            //
            MessageID = reader.Read<MessageId>();

            // 
            VersionServer = reader.ReadString(Constants.VERSIONSERVER_MAXLEN);
        }

        public override void Serialize(Server.Common.Stream.MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            //
            writer.Write(MessageID ?? MessageId.Empty);

            // 
            writer.Write(VersionServer, Constants.VERSIONSERVER_MAXLEN);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID: {MessageID} " +
                $"VersionServer: {VersionServer}";
        }
    }
}