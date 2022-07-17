using Server.Dme.Models;

namespace Server.Dme.PluginArgs
{
    public class OnPlayerArgs
    {
        public ClientObject Player { get; set; }

        public World Game { get; set; }
    }
}
