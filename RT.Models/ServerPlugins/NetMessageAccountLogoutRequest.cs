﻿using RT.Common;
using Server.Common;
using Server.Common.Stream;
using System.IO;

namespace RT.Models.ServerPlugins
{
    [MediusMessage(NetMessageClass.MessageClassApplication, NetMessageTypeIds.NetMessageTypeAccountLogoutRequest)]
    public class NetMessageAccountLogoutRequest : BaseApplicationMessage
    {
        public override NetMessageTypeIds PacketType => NetMessageTypeIds.NetMessageTypeAccountLogoutRequest;

        public override byte IncomingMessage => 0;
        public override byte Size => 0;

        public override byte PluginId => 0;

        public override void DeserializePlugin(MessageReader reader)
        {

        }

        public override void SerializePlugin(MessageWriter writer)
        {

        }

        public override string ToString()
        {
            return base.ToString() + " ";
        }

    }
}