using DotNetty.Common.Internal.Logging;
using Microsoft.Extensions.Logging.Console;
using Newtonsoft.Json;
using NReco.Logging.File;
using Server.Common;
using Server.Common.Logging;
using Server.Plugins;
using Server.UniverseManager.Config;
using System.Diagnostics;

namespace Server.UniverseManager
{
    class Program
    {
        public const string CONFIG_FILE = "config.json";
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<Program>();

        public static ServerSettings Settings = new ServerSettings();

        public static MUM UniverseManager = new(Settings.ServerPort);
        //public static PluginsManager Plugins = null;

        private static FileLoggerProvider _fileLogger = null;

        private static DateTime _lastConfigRefresh = Utils.GetHighPrecisionUtcTime();
        private static DateTime _lastComponentLog = Utils.GetHighPrecisionUtcTime();

        private static int _ticks = 0;
        private static Stopwatch _sw = new Stopwatch();

        public string MUMShortVersion = "2.10.0015";

        static async Task StartServerAsync()
        {
            DateTime lastConfigRefresh = Utils.GetHighPrecisionUtcTime();

            string MUMVersion = "Medius Universe Manager Version 2.10.0015";

            Logger.Info("**************************************************");

            #region MediusGetVersion
            if (Settings.MediusServerVersionOverride == true)
            {
                Logger.Info($"* {Settings.MUMVersion}");
            }
            else
            {
                // Use hardcoded methods in code to handle specific games server versions
                Logger.Info($"* {MUMVersion}");
            }
            #endregion
            string datetime = DateTime.Now.ToString("MMMM/dd/yyyy hh:mm:ss tt");
            Logger.Info($"* Launched on {datetime}");

            //* Process ID: %d , Parent Process ID: %d

            Logger.Info($"* Server Key Type: {Settings.EncryptMessages}");

            #region Remote Log Viewing
            if (Settings.RemoteLogViewPort == 0)
            {
                //* Remote log viewing setup failure with port %d.
                Logger.Info("* Remote log viewing disabled.");
            }
            else if (Settings.RemoteLogViewPort != 0)
            {
                Logger.Info($"* Remote log viewing enabled at port {Settings.RemoteLogViewPort}.");
            }
            #endregion

            if (Settings.Profiling > 0)
            {
                Logger.Info($"* Diagnostic Profiling Enabled: {Settings.Profiling} Counts");
            }

            //* Diagnostic Profiling Enabled: %d Counts

            Logger.Info("**************************************************");
            Logger.Info($"Enabling MUM on Server IP = {Settings.ServerIPAddress} TCP Port = {Settings.ServerPort}.");
            Logger.Info($"MUM started.");

            await UniverseManager.Start();

            try
            {
                while (true)
                {
                    await UniverseManager.Tick();

                    // Reload config
                    if ((Utils.GetHighPrecisionUtcTime() - lastConfigRefresh).TotalMilliseconds > Settings.RefreshConfigInterval)
                    {
                        RefreshConfig();
                        lastConfigRefresh = Utils.GetHighPrecisionUtcTime();
                    }

                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {

            }
        }

        static async Task Main(string[] args)
        {
            // 
            Initialize();

            // Add file logger if path is valid
            if (new FileInfo(LogSettings.Singleton.LogPath)?.Directory?.Exists ?? false)
            {
                var loggingOptions = new FileLoggerOptions()
                {
                    Append = false,
                    FileSizeLimitBytes = LogSettings.Singleton.RollingFileSize,
                    MaxRollingFiles = LogSettings.Singleton.RollingFileCount
                };
                InternalLoggerFactory.DefaultFactory.AddProvider(_fileLogger = new FileLoggerProvider(LogSettings.Singleton.LogPath, loggingOptions));
                _fileLogger.MinLevel = Settings.Logging.LogLevel;
            }

            // Optionally add console logger (always enabled when debugging)
#if DEBUG
            InternalLoggerFactory.DefaultFactory.AddProvider(new ConsoleLoggerProvider((s, level) => level >= LogSettings.Singleton.LogLevel, true));
#else
            if (Settings.Logging.LogToConsole)
                InternalLoggerFactory.DefaultFactory.AddProvider(new ConsoleLoggerProvider((s, level) => level >= LogSettings.Singleton.LogLevel, true));
#endif

            // 
            await StartServerAsync();
        }

        static async Task TickAsync()
        {
            try
            {
#if DEBUG || RELEASE
                if (!_sw.IsRunning)
                    _sw.Start();
#endif

#if DEBUG || RELEASE
                ++_ticks;
                if (_sw.Elapsed.TotalSeconds > 5f)
                {
                    // 
                    _sw.Stop();
                    float tps = _ticks / (float)_sw.Elapsed.TotalSeconds;
                    float error = MathF.Abs(Settings.TickRate - tps) / Settings.TickRate;

                    if (error > 0.1f)
                        Logger.Error($"Average TPS: {tps} is {error * 100}% off of target {Settings.TickRate}");

                    //var dt = DateTime.UtcNow - Utils.GetHighPrecisionUtcTime();
                    //if (Math.Abs(dt.TotalMilliseconds) > 50)
                    //    Logger.Error($"System clock and local clock are out of sync! delta ms: {dt.TotalMilliseconds}");

                    _sw.Restart();
                    _ticks = 0;
                }
#endif




                // Tick Profiling

                // prof:* Total Number of Connect Attempts (%d), Number Disconnects (%d), Total On (%d)
                // 
                //Logger.Info($"prof:* Total Server Uptime = {GetUptime()} Seconds == (%d days, %d hours, %d minutes, %d seconds)");

                //Logger.Info($"prof:* Total Available RAM = {} bytes");

                // Tick
                await Task.WhenAll(UniverseManager.Tick());

                // Tick manager
                //await Manager.Tick();

                // Tick plugins
                //await Plugins.Tick();

                // 
                if ((Utils.GetHighPrecisionUtcTime() - _lastComponentLog).TotalSeconds > 15f)
                {
                    UniverseManager.Log();
                    _lastComponentLog = Utils.GetHighPrecisionUtcTime();
                }

                // Reload config
                if ((Utils.GetHighPrecisionUtcTime() - _lastConfigRefresh).TotalMilliseconds > Settings.RefreshConfigInterval)
                {
                    RefreshConfig();
                    _lastConfigRefresh = Utils.GetHighPrecisionUtcTime();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                await UniverseManager.Stop();
            }
        }

        static void Initialize()
        {
            RefreshConfig();
        }

        /// <summary>
        /// 
        /// </summary>
        static void RefreshConfig()
        {
            // 
            var serializerSettings = new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
            };

            // Load settings
            if (File.Exists(CONFIG_FILE))
            {
                // Populate existing object
                JsonConvert.PopulateObject(File.ReadAllText(CONFIG_FILE), Settings, serializerSettings);
            }
            else
            {
                // Save defaults
                File.WriteAllText(CONFIG_FILE, JsonConvert.SerializeObject(Settings, Formatting.Indented));
            }

            // Set LogSettings singleton
            LogSettings.Singleton = Settings.Logging;

            // Update default rsa key
            Pipeline.Attribute.ScertClientAttribute.DefaultRsaAuthKey = Settings.DefaultKey;

            // Update file logger min level
            if (_fileLogger != null)
                _fileLogger.MinLevel = Settings.Logging.LogLevel;
        }

    }
}
