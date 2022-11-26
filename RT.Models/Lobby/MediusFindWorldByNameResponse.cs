using RT.Common;
using Server.Common;

namespace RT.Models
{
    /// <summary>
    /// 
    /// </summary>
    [MediusMessage(NetMessageClass.MessageClassLobby, MediusLobbyMessageIds.FindWorldByNameResponse)]
    public class MediusFindWorldByNameResponse : BaseLobbyMessage, IMediusResponse
    {
        public override byte PacketType => (byte)MediusLobbyMessageIds.FindWorldByNameResponse;

        public bool IsSuccess => StatusCode >= 0;

        /// <summary>
        /// Message ID
        /// </summary>
        public MessageId MessageID { get; set; }
        /// <summary>
        /// Response status to find a world by name
        /// </summary>
        public MediusCallbackStatus StatusCode;
        /// <summary>
        /// Application ID of the world
        /// </summary>
        public int ApplicationID;
        /// <summary>
        /// Application name related to the app-ID
        /// </summary>
        public string ApplicationName;
        /// <summary>
        /// Application type (game or chat channel)
        /// </summary>
        public MediusApplicationType ApplicationType;
        /// <summary>
        /// World ID
        /// </summary>
        public int MediusWorldID;
        /// <summary>
        /// World Name
        /// </summary>
        public string WorldName;
        /// <summary>
        /// World Status
        /// </summary>
        public MediusWorldStatus WorldStatus;
        /// <summary>
        /// Flag 0 or 1 to determine the end of list.
        /// </summary>
        public bool EndOfList;

        public override void Deserialize(Server.Common.Stream.MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            // 
            MessageID = reader.Read<MessageId>();
            reader.ReadBytes(3);

            //
            StatusCode = reader.Read<MediusCallbackStatus>();
            ApplicationID = reader.ReadInt32();
            ApplicationName = reader.ReadString(Constants.APPNAME_MAXLEN);
            ApplicationType = reader.Read<MediusApplicationType>();
            MediusWorldID = reader.ReadInt32();
            WorldName = reader.ReadString(Constants.WORLDNAME_MAXLEN);
            EndOfList = reader.ReadBoolean();
            reader.ReadBytes(3);
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
            writer.Write(ApplicationID);
            writer.Write(ApplicationName, Constants.APPNAME_MAXLEN);
            writer.Write(ApplicationType);
            writer.Write(MediusWorldID);
            writer.Write(WorldName);
            writer.Write(EndOfList);
            writer.Write(new byte[3]);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID: {MessageID}" +
                $"StatusCode: {StatusCode} " +
                $"ApplicationID: {ApplicationID} " +
                $"ApplicationName: {ApplicationName} " +
                $"ApplicationType: {ApplicationType} " +
                $"MediusWorldID: {MediusWorldID} " +
                $"EndOfList: {EndOfList}";
        }
    }
}