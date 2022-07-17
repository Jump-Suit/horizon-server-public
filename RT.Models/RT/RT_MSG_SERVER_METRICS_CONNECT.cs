using Server.Common;
using System.IO;

namespace RT.Models
{
    public class RT_MSG_SERVER_METRICS_CONNECT : IStreamSerializer
    {
        public int period_connections_attempted;
        public int period_connections_ignored;
        public int period_connections_refused;
        public int period_connections_rejected;
        public int period_connections_failed;
        public int period_connections_timeout;
        public int period_connections_closed;
        public int server_connections_attempted;
        public int server_connections_ignored;
        public int server_connections_refused;
        public int server_connections_rejected;
        public int server_connections_failed;
        public int server_connections_timeout;
        public int server_connections_closed;
        public int server_connections_friendly_ip;

        public void Deserialize(BinaryReader reader)
        {

        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(period_connections_attempted);
            writer.Write(period_connections_ignored);
            writer.Write(period_connections_refused);
            writer.Write(period_connections_rejected);
            writer.Write(period_connections_failed);
            writer.Write(period_connections_timeout);
            writer.Write(period_connections_closed);
            writer.Write(server_connections_attempted);
            writer.Write(server_connections_ignored);
            writer.Write(server_connections_refused);
            writer.Write(server_connections_rejected);
            writer.Write(server_connections_failed);
            writer.Write(server_connections_timeout);
            writer.Write(server_connections_closed);
            writer.Write(server_connections_friendly_ip);

        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"period_connections_attempted: {period_connections_attempted} " +
                $"period_connections_ignored: {period_connections_ignored} " +
                $"period_connections_refused: {period_connections_refused} " +
                $"period_connections_rejected: {period_connections_rejected} " +
                $"period_connections_failed: {period_connections_failed} " +
                $"period_connections_timeout: {period_connections_timeout} ";

        }
    }
}