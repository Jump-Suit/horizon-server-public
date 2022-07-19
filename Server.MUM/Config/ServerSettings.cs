using Org.BouncyCastle.Math;
using RT.Cryptography;
using Server.Common.Logging;

namespace Server.UniverseManager.Config
{
    /// <summary>
    /// Base Medius Universe Manager Configuration
    /// Version 2.9.x base
    /// </summary>
    public class ServerSettings
    {
        /// <summary>
        /// How many milliseconds before refreshing the config.
        /// </summary>
        public int RefreshConfigInterval = 5000;

        /// <summary>
        /// Number of ticks per second.
        /// </summary>
        public int TickRate { get; set; } = 10;

        #region Server IP/Port
        /// <summary>
        /// IP address to bind with
        /// </summary>
        public string ServerIPAddress { get; set; } = "127.0.0.1";

        /// <summary>
        /// Port of the MUM server.
        /// The specified port should not be used by any other process on this system
        /// </summary>
        public int ServerPort { get; set; } = 10076;
        #endregion

        #region Medius Version
        public bool MediusServerVersionOverride { get; set; } = false;

        public string MUMVersion { get; set; } = "Medius Universe Manager Version 2.10.0015";
        #endregion

        #region Universe Configuration
        // ***Defines the Hard Limits***

        /// <summary>
        /// Defines the MAXIMUM number of total players
        /// </summary>
        public int ConfigWMMaxPlayers = 100000;

        /// <summary>
        /// Defines the MAXIMUM number of Auth Servers
        /// </summary>
        public int ConfigWMMaxNumAuthServers = 20;

        /// <summary>
        /// Defines the MAXIMUM number of Proxy Servers
        /// </summary>
        public int ConfigWMMaxNumProxyServers = 20;

        /// <summary>
        /// Defines the MAXIMUM number of Lobby Servers
        /// </summary>
        public int ConfigWMMaxNumLobbyServers = 20;

        /// <summary>
        /// Defines the MAXIMUM number of Game Servers
        /// Recall that this could be equal to the number of players in-game where host
        /// migration occurs.
        /// </summary>
        public int ConfigWMMaxNumGameServers = 100000;

        /// <summary>
        /// Defines the MAXIMUM number of players-per-chat-channel
        /// </summary>
        public int ConfigWMDefaultMaxPlayersPerLobbyWorld = 256;

        /// <summary>
        /// Defines the MAXIMUM number of GameWorlds per LobbyWorlds
        /// </summary>
        public int ConfigWMMaxGameWorldsPerLobbyWorld = 256;
        #endregion

        #region WorldID 0 Characteristics

        /// <summary>
        /// Define the maximum number of sockets awaiting "connected" state to this server
        /// </summary>
        public int MaxDefaultQueued = 32;

        /// <summary>
        /// Define the maximum number of sockets considered "connected" but not yet
        /// migrated to the desired World
        /// </summary>
        public int MaxDefaultClients = 32;

        /// <summary>
        /// Define the maximum amount of time a client will be kept "holding" in a connected
        /// state without being processed - should be increased for heavy-load conditions,
        /// but decreased to mitigate DOS/DDOS attack thresholds (default 5000)
        /// </summary>
        public int DefaultWorldTimeout = 30000;

        #endregion

        #region Diagnostic Profiling
        /// <summary>
        /// Set this to 0 to disable diagnostic profiling.  Setting this to a value greater
        /// than 0 will result in a heartbeat log message after this many loops through the
        /// main() loop of the server.  For instance, if this is set to 100, a heartbeat
        /// will be printed out every 100 times through the main() loop.
        /// </summary>
        public int Profiling = 0;
        #endregion

        #region ApplicationID Compatibility List
        /// <summary>
        /// Compatible application ids. Null means all are accepted.
        /// </summary>
        public int[] AppIDCompatibilityList { get; set; } = null;

        /// <summary>
        /// Set the IP and Port to bind to for the command port.
        /// ":Port" binds to all ip addresses at the specified port.
        /// "IP:Port" binds to only the specified IP and port.
        /// "localhost:Port" binds specifically to the localhost (usually 127.0.0.1)
        /// and not to an externally reachable IP address.
        /// Caution: I don't suggest using localhost.
        /// </summary>
        public string HTTPCommandPort = ":11076";

        /// <summary>
        /// Determine the number of simulatenous connections to the command port.
        /// </summary>
        public int HTTPCommandPortNumThreads = 5;

        /// <summary>
        /// Set a list of IP addresses and subnet masks in the form of:
        /// IPA/SubnetA;IPB/SubnetB;
        /// The IP Address is followed by a forward slash, then by the subnet mask, then a
        /// semicolon to finish the entry.  Every entry requires a semicolon to
        /// terminate it.
        /// </summary>
        public string[] HTTPCommandPortValidIPAndSubnet = { "208.236.15.196", "255.255.255.255", "208.236.15.197", "255.255.255.255" };

        /// <summary>
        /// If this value is set to 1, the account stats (i.e., player reports) will be 
        /// treated as strings and all values after the first null byte will be zero'ed 
        /// out.
        /// If all titles running on a given universe are using strings for their
        /// account stats then this should be set to 1.  If any title running on the
        /// universe has binary stats then this should be set to 0.
        /// </summary>
        public int AccountStatsAsString = 0;
        #endregion

        #region Remote Log Viewer Port To Listen
        /// <summary>
        /// Any value greater than 0 will enable remote logging with the SCE-RT logviewer
        /// on that port, which must not be in use by other applications (default 0)
        /// </summary>
        public int RemoteLogViewPort = 0;
        #endregion

        /// <summary>
        /// Key used to authenticate clients.
        /// </summary>
        public RsaKeyPair DefaultKey { get; set; } = new RsaKeyPair(
            new BigInteger("10315955513017997681600210131013411322695824559688299373570246338038100843097466504032586443986679280716603540690692615875074465586629501752500179100369237", 10),
            new BigInteger("17", 10),
            new BigInteger("4854567300243763614870687120476899445974505675147434999327174747312047455575182761195687859800492317495944895566174677168271650454805328075020357360662513", 10)
            );

        /// <summary>
        /// Whether or not to encrypt messages.
        /// </summary>
        public bool EncryptMessages { get; set; } = true;

        /// <summary>
        /// Logging settings.
        /// </summary>
        public LogSettings Logging { get; set; } = new LogSettings();
    }
}
