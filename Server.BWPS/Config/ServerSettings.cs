using Server.Common.Logging;

namespace Server.BWPS.Config
{
    public class ServerSettings
    {
        /// <summary>
        /// How many milliseconds before refreshing the config.
        /// </summary>
        public int RefreshConfigInterval = 5000;

        /// <summary>
        /// Ports of the BWPS.
        /// </summary>
        public int BWPSPort { get; set; } = 50100;

        #region BWPS SCE-RT Service Location
        /// <summary>
        /// Ip address of the NAT server.
        /// Provide the IP of the SCE-RT NAT Service
        /// Default is: natservice.pdonline.scea.com:10070
        /// </summary>
        public string? BWPSIp { get; set; } = null;
        #endregion

        /// <summary>
        /// Whether or not to encrypt messages.
        /// </summary>
        public bool EncryptMessages { get; set; } = true;

        /// <summary>
        /// Number of milliseconds for main loop thread to sleep.
        /// </summary>
        public int MainLoopSleepMs { get; set; } = 5;

        /// <summary>
        /// Milliseconds between plugin ticks.
        /// </summary>
        public int PluginTickIntervalMs { get; set; } = 50;

        /// <summary>
        /// Logging settings.
        /// </summary>
        public LogSettings Logging { get; set; } = new LogSettings();
    }

}