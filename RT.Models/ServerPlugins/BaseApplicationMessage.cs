using RT.Common;

namespace RT.Models
{
    public abstract class BaseApplicationMessage : BaseMediusPluginMessage
    {
        public override NetMessageClass PacketClass => NetMessageClass.MessageClassApplication;

        public BaseApplicationMessage()
        {

        }

    }
}
