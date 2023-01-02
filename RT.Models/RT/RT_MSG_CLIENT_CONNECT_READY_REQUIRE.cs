using RT.Common;

namespace RT.Models
{
    [ScertMessage(RT_MSG_TYPE.RT_MSG_CLIENT_CONNECT_READY_REQUIRE)]
    public class RT_MSG_CLIENT_CONNECT_READY_REQUIRE : BaseScertMessage
    {

        public override RT_MSG_TYPE Id => RT_MSG_TYPE.RT_MSG_CLIENT_CONNECT_READY_REQUIRE;

        // 
        public byte ServReq;
        public byte length;
        public byte pPassword;
        public ushort Password_Len;
        public string Password;

        public override void Deserialize(Server.Common.Stream.MessageReader reader)
        {
            ServReq = reader.ReadByte();
            length = reader.ReadByte();
            pPassword = reader.ReadByte();
            /*
            if (ServReq == 0x01)
            {
                Client = reader.ReadByte();
                Password_Len = reader.ReadByte();
                Password = reader.ReadString();
            }
            */
        }

        public override void Serialize(Server.Common.Stream.MessageWriter writer)
        {
            writer.Write(ServReq);
            writer.Write(length);
            writer.Write(pPassword);


            /*
            if (ServReq == 0x01)
            {
                writer.Write(Client);
                writer.Write(Password_Len);
                writer.Write(Password);
            }
            */
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"ServReq: {ServReq} " +
                $"length: {length} " +
                $"pPassword: {pPassword}";
        }
    }
}