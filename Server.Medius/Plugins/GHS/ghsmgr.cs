using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;

namespace Server.Medius.Plugins.GHS
{
    public class GHSMGR
    {
        static readonly IInternalLogger _logger = InternalLoggerFactory.GetInstance<MAS>();

        protected IInternalLogger Logger => _logger;

        public static async Task GhsInit()
        {


            return;
        }



        


        public void scertGhsPluginProtocolVersion()
        {
            Logger.Info("1.0");
        }

        public void scertGHSGetVersion()
        {
            Logger.Info("SCERT GHS Plugin 1.0.0");
        }
    }
}
