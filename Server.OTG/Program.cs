using DotNetty.Common.Internal.Logging;
using Server.Common;
using Server.Common.Logging;
using NReco.Logging.File;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Microsoft.Extensions.Logging.Console;
using Newtonsoft.Json;
using System.Management;
using Server.OTG.Config;

namespace Server.OTG
{
    class Program
    {
        public const string CONFIG_FILE = "config.json";
        public const string DB_CONFIG_FILE = "db.config.json";

        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<Program>();

        public static ServerSettings Settings = new ServerSettings();
        //public static DbController Database = new DbController(DB_CONFIG_FILE);

        public static OTG OTGServer = new OTG(Settings.Port);

        private static FileLoggerProvider _fileLogger = null;


        static async Task StartServerAsync()
        {
            DateTime lastConfigRefresh = Utils.GetHighPrecisionUtcTime();

            string datetime = DateTime.Now.ToString("MMMM/dd/yyyy hh:mm:ss tt");

            Logger.Info("**************************************************");
            #region MediusGetBuildTimeStamp
            var MediusBuildTimeStamp = GetLinkerTime(Assembly.GetEntryAssembly());
            Logger.Info($"* MediusBuildTimeStamp at {MediusBuildTimeStamp}");
            #endregion

            string gpszVersionString = "1.0.0";

            Logger.Info($"* Global Handle Service Version {gpszVersionString}");
            Logger.Info($"* Launched on {datetime}");

            Task.WaitAll(OTGServer.Start());
            /*
            //* Process ID: %d , Parent Process ID: %d
            if (Database._settings.SimulatedMode == true)
            {
                Logger.Info("* Database Disabled GHS");
            } else {
                Logger.Info("* Database Enabled GHS");
            }
            */
            Logger.Info($"* Server Key Type: {Settings.EncryptMessages}");

            //* Diagnostic Profiling Enabled: %d Counts

            Logger.Info("**************************************************");


            Logger.Info($"OTG Telemetry Service started.");


            try
            {
                while (true)
                {
                    // Tick
                    await Task.WhenAll(OTGServer.Tick());

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
            //Pipeline.Attribute.ScertClientAttribute.DefaultRsaAuthKey = Settings.DefaultKey;

            // Update file logger min level
            if (_fileLogger != null)
                _fileLogger.MinLevel = Settings.Logging.LogLevel;
        }


        #region System Time
        public static DateTime GetLinkerTime(Assembly assembly)
        {
            const string BuildVersionMetadataPrefix = "+build";

            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (attribute?.InformationalVersion != null)
            {
                var value = attribute.InformationalVersion;
                var index = value.IndexOf(BuildVersionMetadataPrefix);
                if (index > 0)
                {
                    value = value[(index + BuildVersionMetadataPrefix.Length)..];
                    return DateTime.ParseExact(value, "yyyy-MM-ddTHH:mm:ss:fffZ", CultureInfo.InvariantCulture);
                }
            }

            return default;
        }

        public static TimeSpan GetUptime()
        {
            ManagementObject mo = new(@"\\.\root\cimv2:Win32_OperatingSystem=@");
            DateTime lastBootUp = ManagementDateTimeConverter.ToDateTime(mo["LastBootUpTime"].ToString());
            return DateTime.Now.ToUniversalTime() - lastBootUp.ToUniversalTime();
        }
        #endregion
    }
}