using RT.Common;
using Server.Common;

namespace RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobbyExt, MediusLobbyExtMessageIds.TicketLogin)]
    public class MediusTicketLoginRequest : BaseLobbyExtMessage, IMediusRequest
    {
        public override byte PacketType => (byte)MediusLobbyExtMessageIds.TicketLogin;

        /// <summary>
        /// Message ID
        /// </summary>
        public MessageId MessageID { get; set; }
        /// <summary>
        /// Session Key
        /// </summary>
        public string SessionKey; // SESSIONKEY_MAXLEN
        /// <summary>
        /// Ticket Size
        /// </summary>
        public long TicketSize; 
        public byte[] UNK0;
        /// <summary>
        /// Account Name
        /// </summary>
        public string AccountName;
        public string Password = "";
        public byte[] UNK1;
        /// <summary>
        /// NP Service ID
        /// </summary>
        public string ServiceID;

        public override void Deserialize(Server.Common.Stream.MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            //
            MessageID = reader.Read<MessageId>();

            // 
            SessionKey = reader.ReadString(Constants.SESSIONKEY_MAXLEN);
            reader.ReadBytes(2);
            TicketSize = reader.ReadInt32();
            UNK0 = reader.ReadBytes(82);
            AccountName = reader.ReadString(Constants.ACCOUNTNAME_MAXLEN);
            UNK1 = reader.ReadBytes(20);
            ServiceID = reader.ReadString(24);
        }

        public override void Serialize(Server.Common.Stream.MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            //
            writer.Write(MessageID ?? MessageId.Empty);

            // 
            writer.Write(SessionKey, Constants.SESSIONKEY_MAXLEN);
            writer.Write(2);
            writer.Write(TicketSize);
            writer.Write(UNK0 ?? new byte[82], 82);
            writer.Write(AccountName, Constants.ACCOUNTNAME_MAXLEN);
            writer.Write(UNK1 ?? new byte[20], 20);
            writer.Write(ServiceID ?? "", 24);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID:{MessageID} " +
                $"SessionKey:{SessionKey} " +
                $"TicketSize: {TicketSize} " +
                $"UNK0: {UNK0} " +
                $"AccountName: {AccountName} " +
                $"UNK1: {UNK1} " +
                $"ServiceID: {ServiceID}";
        }
    }
}