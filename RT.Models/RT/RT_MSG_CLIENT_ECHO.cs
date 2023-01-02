using RT.Common;
using Server.Common;
using System;

namespace RT.Models
{
    [ScertMessage(RT_MSG_TYPE.RT_MSG_CLIENT_ECHO)]
    public class RT_MSG_CLIENT_ECHO : BaseScertMessage
    {
        //
        public override RT_MSG_TYPE Id => RT_MSG_TYPE.RT_MSG_CLIENT_ECHO;

        //
        public byte[] Value;

        public override void Deserialize(Server.Common.Stream.MessageReader reader)
        {
            Value = reader.ReadRest();
        }

        public override void Serialize(Server.Common.Stream.MessageWriter writer)
        {
            writer.Write(Value);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"Value: {BitConverter.ToString(Value)}";
        }

    }
}