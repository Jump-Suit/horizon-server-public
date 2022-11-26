using RT.Common;
using Server.Common;
using Server.Common.Stream;
using System.IO;

namespace RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassApplication, NetMessageTypeIds.NetMessageTypeProtocolInfo)]
    public class NetMessageProtocolInfo : BaseApplicationMessage
    {
        public override NetMessageTypeIds PacketType => NetMessageTypeIds.NetMessageTypeProtocolInfo;

        public long protocolInfo;
        public long buildNumber;


        public override void DeserializePlugin(MessageReader reader)
        {
            protocolInfo = reader.ReadInt32();
            buildNumber = reader.ReadUInt32();

        }
        public override void SerializePlugin(MessageWriter writer)
        {
            writer.Write(protocolInfo);
            writer.Write(buildNumber);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"protocolInfo: {protocolInfo} " +
                $"buildNumber: {buildNumber}";
        }

    }
}