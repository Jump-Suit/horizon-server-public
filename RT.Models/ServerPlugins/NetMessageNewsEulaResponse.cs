using RT.Common;
using Server.Common;
using System.IO;

namespace RT.Models.ServerPlugins
{
    public class NetMessageNewsEulaResponse : IStreamSerializer
    {

        public bool m_finished;
        public NetMessageNewsEulaResponseContentType m_type;
        public string m_content;
        public uint m_timestamp;

        public void Deserialize(BinaryReader reader)
        {
            m_finished = reader.ReadBoolean();
            m_type = reader.Read<NetMessageNewsEulaResponseContentType>();
            m_content = reader.ReadString();
            m_timestamp = reader.ReadUInt32();

        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(m_finished);
            writer.Write(m_type);
            writer.Write(m_content);
            writer.Write(m_timestamp);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"m_finished: {m_finished} " +
                $"m_type: {m_type} " +
                $"m_content: {m_content} " +
                $"m_timestamp: {m_timestamp}";
        }
    }
}