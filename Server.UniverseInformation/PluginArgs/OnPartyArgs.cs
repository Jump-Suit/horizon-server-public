using Server.UniverseInformation.Models;

namespace Server.UniverseInformation.PluginArgs
{
    public class OnPartyArgs
    {
        /// <summary>
        /// Party.
        /// </summary>
        public Party Party { get; set; }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"Party: {Party}";
        }
    }
}
