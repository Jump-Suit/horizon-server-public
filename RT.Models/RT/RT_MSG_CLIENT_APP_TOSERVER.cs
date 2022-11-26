using RT.Common;
using Server.Common;

namespace RT.Models
{
    [ScertMessage(RT_MSG_TYPE.RT_MSG_CLIENT_APP_TOSERVER)]
    public class RT_MSG_CLIENT_APP_TOSERVER : BaseScertMessage
    {
        public override RT_MSG_TYPE Id => RT_MSG_TYPE.RT_MSG_CLIENT_APP_TOSERVER;

        public BaseMediusMessage Message { get; set; } = null;

        //public BaseMediusGhsMessage GhsMessage { get; set; } = null;

        public override void Deserialize(Server.Common.Stream.MessageReader reader)
        {
            /*
            if(reader.AppId == 0)
            {
                GhsMessage = BaseMediusGhsMessage.Instantiate(reader);
            } else
            {
                Message = BaseMediusMessage.Instantiate(reader);
            }
            */
            Message = BaseMediusMessage.Instantiate(reader);
        }

        public override void Serialize(Server.Common.Stream.MessageWriter writer)
        {
            /*
            if(GhsMessage != null)
            {
                writer.Write(GhsMessage.PacketClass);
                writer.Write(GhsMessage.PacketType);
                GhsMessage.Serialize(writer);
            } else if (Message != null)
            {
                writer.Write(Message.PacketClass);
                writer.Write(Message.PacketType);
                Message.Serialize(writer);
            }
            */

            if (Message != null)
            {
                writer.Write(Message.PacketClass);
                writer.Write(Message.PacketType);
                Message.Serialize(writer);
            }
        }

        public override bool CanLog()
        {
            return base.CanLog() && (Message?.CanLog() ?? true);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"Message: {Message}";
        }
    }
}