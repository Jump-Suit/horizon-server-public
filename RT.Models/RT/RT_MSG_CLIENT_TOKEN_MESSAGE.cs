using RT.Common;
using Server.Common;
using System;

namespace RT.Models
{
    [ScertMessage(RT_MSG_TYPE.RT_MSG_CLIENT_TOKEN_MESSAGE)]
    public class RT_MSG_CLIENT_TOKEN_MESSAGE : BaseScertMessage
    {
        public override RT_MSG_TYPE Id => RT_MSG_TYPE.RT_MSG_CLIENT_TOKEN_MESSAGE;

        public RT_TOKEN_MESSAGE_TYPE RT_TOKEN_MESSAGE_TYPE;
        public ushort targetToken;

        public override void Deserialize(Server.Common.Stream.MessageReader reader)
        {
            RT_TOKEN_MESSAGE_TYPE = reader.Read<RT_TOKEN_MESSAGE_TYPE>();
            targetToken = reader.ReadUInt16();
        }

        public override void Serialize(Server.Common.Stream.MessageWriter writer)
        {
            writer.Write(RT_TOKEN_MESSAGE_TYPE);
            writer.Write(targetToken);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"RT_TOKEN_MESSAGE_TYPE: {RT_TOKEN_MESSAGE_TYPE} " +
                $"targetToken: {targetToken}";
        }
    }
}