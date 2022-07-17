using DotNetty.Common.Internal.Logging;

namespace RT.libAntiCheat.Main
{
    public class Init
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<Init>();
        //public static ServerSettings Settings = new ServerSettings();



        public virtual async void AntiCheatInit()
        {





        }



        #region AntiCheat 

        public static void anticheatGetVersion()
        {
            string anticheatVersion = "AntiCheat V2.9.2";
            /*
            if (Settings.AntiCheatOn != true)
            {
                Logger.Info("AntiCheat is not activated. \n Try setting AntiCheatOn=1 in your config.json file.");
            }
            else
            {
                Logger.Info($"{anticheatVersion}");
            }
            */
        }
        #endregion
    }
}