using RT.Common;
using Server.Common;

namespace RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobby, MediusLobbyMessageIds.LadderList_ExtraInfo0)]
    public class MediusLadderList_ExtraInfoRequest0 : MediusLadderList_ExtraInfoRequest, IMediusRequest
    {
        public override byte PacketType => (byte)MediusLobbyMessageIds.LadderList_ExtraInfo0;

    }
}
