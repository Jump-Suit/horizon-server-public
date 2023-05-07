using RT.Common;
using Server.Common;
using Server.Common.Stream;
using System;
using System.IO;

namespace RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassApplication, NetMessageTypeIds.NetMessageTypeProtocolInfo)]
    public class NetMessageTypeProtocolInfo : BaseApplicationMessage
    {
        public override NetMessageTypeIds PacketType => NetMessageTypeIds.NetMessageTypeProtocolInfo;

        public override byte IncomingMessage => 0;
        public override byte Size => 5;

        public override byte PluginId => 31;

        public int protocolInfo;
        public int buildNumber;


        public override void DeserializePlugin(MessageReader reader)
        {
            protocolInfo = reader.ReadInt32();
            buildNumber = reader.ReadInt32();

        }
        public override void SerializePlugin(MessageWriter writer)
        {
            writer.Write(protocolInfo);
            writer.Write(buildNumber);
        }

        public override string ToString()
        {
            var ProtoBytesReversed = ReverseBytes(protocolInfo);

            return base.ToString() + " " +
                $"protocolInfo: {ProtoBytesReversed} " +
                $"buildNumber: {buildNumber}";
        }
    }
}