using System.Threading.Tasks;

namespace Server.Medius
{
    public interface IMediusComponent
    {
        int TCPPort { get; }
        int UDPPort { get; }

        void Start();
        Task Stop();
        Task Tick();
    }
}