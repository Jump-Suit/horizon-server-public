using RT.Common;
using Server.Common;
using Server.Common.Stream;
using System.IO;

namespace RT.Models.ServerPlugins
{
    [MediusMessage(NetMessageClass.MessageClassApplication, NetMessageTypeIds.NetMessageTypeAccountLogoutRequest)]
    public class NetMessageAccountLogoutRequest : BaseApplicationMessage
    {
        public override NetMessageTypeIds PacketType => NetMessageTypeIds.NetMessageTypeAccountLogoutRequest;

        public byte Unk;

        public override void DeserializePlugin(MessageReader reader)
        {
            reader.ReadBytes(67);
            Unk = reader.ReadByte();
        }

        public override void SerializePlugin(MessageWriter writer)
        {
            writer.Write(new byte[67]);
            writer.Write(Unk);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"Unk: {Unk} ";
        }

    }
}