using RT.Common;
using Server.Common;

namespace RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobby, MediusLobbyMessageIds.FileUpdateMetaData)]
    public class MediusFileUpdateMetaDataRequest : BaseLobbyMessage, IMediusRequest
    {

        public override byte PacketType => (byte)MediusLobbyMessageIds.FileUpdateMetaData;

        public MessageId MessageID { get; set; }

        public MediusFile MediusFileInfo;
        public MediusFileMetaData MediusUpdateMetaData;

        public override void Deserialize(Server.Common.Stream.MessageReader reader)
        {
            // 
            MediusFileInfo = reader.Read<MediusFile>();
            MediusUpdateMetaData = reader.Read<MediusFileMetaData>();

            // 
            base.Deserialize(reader);

            //
            MessageID = reader.Read<MessageId>();
            reader.ReadBytes(3);
        }

        public override void Serialize(Server.Common.Stream.MessageWriter writer)
        {
            // 
            writer.Write(MediusFileInfo);
            writer.Write(MediusUpdateMetaData);

            // 
            base.Serialize(writer);

            //
            writer.Write(MessageID ?? MessageId.Empty);
            writer.Write(new byte[3]);
        }


        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID: {MessageID} " +
                $"MediusFileInfo: {MediusFileInfo} "  +
                $"MediusUpdateMetaData: {MediusUpdateMetaData}";
        }
    }
}