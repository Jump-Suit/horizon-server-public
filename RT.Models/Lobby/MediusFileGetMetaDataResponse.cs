using RT.Common;
using Server.Common;

namespace RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobby, MediusLobbyMessageIds.FileGetMetaDataResponse)]
    public class MediusFileGetMetaDataResponse : BaseLobbyMessage, IMediusResponse
    {

        public override byte PacketType => (byte)MediusLobbyMessageIds.FileGetMetaDataResponse;

        public bool IsSuccess => StatusCode >= 0;
        public MessageId MessageID { get; set; }
        public MediusCallbackStatus StatusCode;
        public bool EndOfList;

        public MediusFile MediusFileInfo;
        public MediusFileMetaData MediusMetaDataResponseKey;

        public override void Deserialize(Server.Common.Stream.MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            // 
            MediusFileInfo = reader.Read<MediusFile>();
            MediusMetaDataResponseKey = reader.Read<MediusFileMetaData>();
            StatusCode = reader.Read<MediusCallbackStatus>();

            //
            MessageID = reader.Read<MessageId>();
            EndOfList = reader.ReadBoolean();
            reader.ReadBytes(2);
        }

        public override void Serialize(Server.Common.Stream.MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            // 
            writer.Write(MediusFileInfo);
            writer.Write(MediusMetaDataResponseKey);

            //
            writer.Write(MessageID ?? MessageId.Empty);
            writer.Write(EndOfList);
            writer.Write(new byte[2]);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
             $"MessageID: {MessageID} " +
             $"MediusFileInfo: {MediusFileInfo} " +
             $"MediusFileMetaData: {MediusMetaDataResponseKey}";
        }
    }
}