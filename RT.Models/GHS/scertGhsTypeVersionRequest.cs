using RT.Common;
using Server.Common;
using System.IO;

namespace RT.Models
{
    
    [MediusMessage(NetMessageClass.MessageClassGHS, GhsOpcode.ghs_ServerProtocolNegotiation)]
    public class scertGhsTypeVersionRequest : BaseMediusGHSMessage
    {
        public override NetMessageClass PacketClass => NetMessageClass.MessageClassLobby;

        public override GhsOpcode GhsOpcode => GhsOpcode.ghs_ServerProtocolNegotiation;

        public int version;

        public void Deserialize(BinaryReader reader)
        {
            version = reader.ReadInt32();

        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(version);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"version: {version} ";
        }

    }
    
}