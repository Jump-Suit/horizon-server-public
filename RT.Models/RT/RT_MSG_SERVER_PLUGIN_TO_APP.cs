using RT.Common;
using Server.Common;

namespace RT.Models
{
    [ScertMessage(RT_MSG_TYPE.RT_MSG_SERVER_PLUGIN_TO_APP)]
    public class RT_MSG_SERVER_PLUGIN_TO_APP : BaseScertMessage
    {
        public override RT_MSG_TYPE Id => RT_MSG_TYPE.RT_MSG_SERVER_PLUGIN_TO_APP;

        public BaseMediusPluginMessage Message { get; set; } = null;

        public override bool SkipEncryption
        {
            get => Message?.SkipEncryption ?? base.SkipEncryption;
            set
            {
                if (Message != null) { Message.SkipEncryption = value; }
                base.SkipEncryption = value;
            }
        }

        public override void Deserialize(Server.Common.Stream.MessageReader reader)
        {
            Message = BaseMediusPluginMessage.Instantiate(reader);
        }

        public override void Serialize(Server.Common.Stream.MessageWriter writer)
        {
            if (Message != null)
            {
                writer.Write(Message.PacketClass);
                writer.Write(new byte[3]);
                writer.Write(Message.PacketType);
                Message.SerializePlugin(writer);
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