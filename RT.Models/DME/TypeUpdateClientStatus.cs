using RT.Common;
using Server.Common;

namespace RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassDME, MediusDmeMessageIds.UpdateClientStatus)]
    public class TypeUpdateClientStatus : BaseDMEMessage
    {

        public override byte PacketType => (byte)MediusDmeMessageIds.UpdateClientStatus;

        public NetClientStatus ClientStatus = NetClientStatus.ClientStatusJoined;
        public byte SourceClientIndex;
        public short NetObjectBufferCount;
        public short NetDataStreamCount;
        public short bArbitrateJoinResponse;
        public long MigrateOrder;
        public long UserSpecified;


        public override void Deserialize(Server.Common.Stream.MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            // 
            ClientStatus = reader.Read<NetClientStatus>();
            SourceClientIndex = reader.ReadByte();
            NetObjectBufferCount = reader.ReadInt16();
            NetDataStreamCount = reader.ReadInt16();
            bArbitrateJoinResponse = reader.ReadInt16();
            MigrateOrder = reader.ReadUInt32();
            UserSpecified = reader.ReadUInt32();
        }

        public override void Serialize(Server.Common.Stream.MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            // 
            writer.Write(ClientStatus);
            writer.Write(SourceClientIndex);
            writer.Write(NetObjectBufferCount);
            writer.Write(NetDataStreamCount);
            writer.Write(bArbitrateJoinResponse);
            writer.Write(MigrateOrder);
            writer.Write(UserSpecified);
        }


        public override string ToString()
        {
            return base.ToString() + " " +
             $"ClientStatus: {ClientStatus} " +
             $"PlayerIndex: {SourceClientIndex} " +
             $"NetObjectBufferCount: {NetObjectBufferCount} " +
             $"NetDataStreamCount: {NetDataStreamCount} " +
             $"bArbitrateJoinResponse: {bArbitrateJoinResponse} " +
             $"MigrateOrder: {MigrateOrder} " +
             $"UserSpecified: {UserSpecified} ";
        }
    }
}
