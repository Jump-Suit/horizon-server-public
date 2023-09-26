using Server.Pipeline.Udp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.BWPS.PluginArgs
{
    public class OnUdpMsg
    {

        public ScertDatagramPacket Packet { get; set; }

        public bool Ignore { get; set; }
    }
}
