using Server.Common.Logging;
using System.Collections;

namespace Server.SVO.Config
{
    public class ServerSettings
    {
        /// <summary>
        /// How many milliseconds before refreshing the config.
        /// </summary>
        public int RefreshConfigInterval = 5000;

        public bool SVODebug = false;

        #region SVO SCE-RT Service Location

        /// <summary>
        /// Ip address of the NAT server.
        /// Provide the IP of the SCE-RT NAT Service
        /// Default is: natservice.pdonline.scea.com:10070
        /// </summary>
        public string? SVOIp { get; set; } = null;

        /// <summary>
        /// HTTP Port of the SCE-RT View Online Server.
        /// </summary>
        public int SVOHttpPort { get; set; } = 10060;

        /// <summary>
        /// HTTPS Port of the SCE-RT View Online Server.
        /// </summary>
        public int SVOHttpsPort { get; set; } = 10061;
        #endregion

        /// <summary>
        /// Root URL for the game's svo files
        /// </summary>
        public string[] SVOPath { get; set; } = new string[] { "title_SVML" , "DEMO_XML"};

        /// <summary>
        /// Logging settings.
        /// </summary>
        public LogSettings Logging { get; set; } = new LogSettings();
    }


}