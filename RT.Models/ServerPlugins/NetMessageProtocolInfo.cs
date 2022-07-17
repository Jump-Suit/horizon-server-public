using RT.Common;
using Server.Common;
using System.IO;

namespace RT.Models.ServerPlugins
{
    [MediusPluginMessage(NetMessageTypeIds.NetMessageProtocolInfo)]
    public class NetMessageProtocolInfo : BaseMediusPluginMessage
    {
        public override NetMessageTypeIds PacketType => NetMessageTypeIds.NetMessageProtocolInfo;

        public override NetMessageClass PacketClass => throw new System.NotImplementedException();

        public int protocolInfo;
        public int buildNumber;

        public void Deserialize(BinaryReader reader)
        {
            protocolInfo = reader.ReadInt32();
            buildNumber = reader.ReadInt32();

        }

        public void Serialize(BinaryWriter writer)
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