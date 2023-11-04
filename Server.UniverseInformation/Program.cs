using DotNetty.Common.Internal.Logging;
using Haukcode.HighResolutionTimer;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NReco.Logging.File;
using RT.Common;
using RT.Models;
using Server.Common;
using Server.Common.Logging;
using Server.Database;
using Server.Plugins;
using Server.UniverseInformation.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

namespace Server.UniverseInformation
{
    class Program
    {
        private static string CONFIG_DIRECTIORY = "./";
        public static string CONFIG_FILE => Path.Combine(CONFIG_DIRECTIORY, "muis.json");
        public static string DB_CONFIG_FILE => Path.Combine(CONFIG_DIRECTIORY, "db.config.json");
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<Program>();

        public static ServerSettings Settings = new ServerSettings();
        public static DbController Database = null;

        public static MediusManager Manager = new MediusManager();
        public static PluginsManager Plugins = null;

        public static RSA_KEY GlobalAuthPublic = null;

        public static MUIS[] UniverseInfoServers = null;

        private static FileLoggerProvider _fileLogger = null;

        private static Dictionary<int, AppSettings> _appSettings = new Dictionary<int, AppSettings>();
        private static AppSettings _defaultAppSettings = new AppSettings(0);
        private static Dictionary<string, int[]> _appIdGroups = new Dictionary<string, int[]>();
        private static ulong _sessionKeyCounter = 0;
        private static readonly object _sessionKeyCounterLock = _sessionKeyCounter;
        private static DateTime? _lastSuccessfulDbAuth = null;
        private static int _ticks = 0;
        private static Stopwatch _sw = new Stopwatch();
        private static HighResolutionTimer _timer;

        private static async Task TickAsync()
        {



            // Attempt to authenticate with the db middleware
            // We do this every 24 hours to get a fresh new token
            if ((_lastSuccessfulDbAuth == null || (Utils.GetHighPrecisionUtcTime() - _lastSuccessfulDbAuth.Value).TotalHours > 24))
            {
                if (!await Database.Authenticate())
                {
                    // Log and exit when unable to authenticate
                    Logger.Error($"Unable to authenticate connection to Cache Server.");
                    return;
                }
                else
                {
                    _lastSuccessfulDbAuth = Utils.GetHighPrecisionUtcTime();

                    // pass to manager
                    await OnDatabaseAuthenticated();

                    // refresh app settings
                    await RefreshAppSettings();

                    #region Check Cache Server Simulated
                    if (Database._settings.SimulatedMode != true)
                    {
                        Logger.Info("Connected to Cache Server");
                    }
                    else
                    {
                        Logger.Info("Connected to Cache Server (Simulated)");
                    }
                    #endregion
                }
            }

        }

        static async Task StartServerAsync()
        {
            DateTime lastConfigRefresh = Utils.GetHighPrecisionUtcTime();

            string datetime = DateTime.Now.ToString("MMMM/dd/yyyy hh:mm:ss tt");

            Logger.Info("**************************************************");
            #region MediusGetBuildTimeStamp
            var MediusBuildTimeStamp = GetLinkerTime(Assembly.GetEntryAssembly());
            Logger.Info($"* MediusBuildTimeStamp at {MediusBuildTimeStamp}");
            #endregion

            string gpszVersionString = "3.05.201109161400";

            Logger.Info($"* Medius Universe Information Server Version {gpszVersionString}");
            Logger.Info($"* Launched on {datetime}");

            if (Database._settings.SimulatedMode == true)
            {
                Logger.Info("* Database Disabled Medius Stack");
            }
            else
            {
                Logger.Info("* Database Enabled Medius Stack");
            }

            UniverseInfoServers = new MUIS[Settings.Ports.Length];
            for (int i = 0; i < UniverseInfoServers.Length; ++i)
            {
                Logger.Info($"* Enabling MUIS on TCP Port = {Settings.Ports[i]}.");
                UniverseInfoServers[i] = new MUIS(Settings.Ports[i]);
                UniverseInfoServers[i].Start();
            }
            /*
            //* Process ID: %d , Parent Process ID: %d
            if (Database._settings.SimulatedMode == true)
            {
                Logger.Info("* Database Disabled Medius Universe Information Server");
            } else {
                Logger.Info("* Database Enabled Medius Universe Information Server");
            }
            */
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


            #region MediusGetVersion
            if (Settings.MediusServerVersionOverride == true)
            {
                // Use override methods in code to send our own version string from config
                Logger.Info("Using config input server version");
                Logger.Info($"MUISVersion Version: {Settings.MUISVersion}");

            }
            else
            {
                // Use hardcoded methods in code to handle specific games server versions
                Logger.Info("Using game specific server versions");
            }


            #endregion


            //* Diagnostic Profiling Enabled: %d Counts

            Logger.Info("**************************************************");

            if (Settings.NATIp != null)
            {
                IPAddress ip = IPAddress.Parse(Settings.NATIp);
                DoGetHostEntry(ip);
            }

            Logger.Info($"MUIS initalized.");

            try
            {
                while (true)
                {
                    // Tick
                    await TickAsync();
                    await Task.WhenAll(UniverseInfoServers.Select(x => x.Tick()));

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
                await Task.WhenAll(UniverseInfoServers.Select(x => x.Stop()));
            }
        }

        static async Task Main(string[] args)
        {
            // get path to config directory from first argument
            if (args.Length > 0)
                CONFIG_DIRECTIORY = args[0];

            // 
            Database = new DbController(DB_CONFIG_FILE);

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
                // Add default localhost entry
                Settings.Universes.Add(0, new UniverseInfo[] {
                    new UniverseInfo()
                    {
                        Name = "sample universe",
                        Endpoint = "url",
                        Port = 10075,
                        UniverseId = 1
                    }
                });

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

        public static void DoGetHostEntry(IPAddress address)
        {
            try
            {
                IPHostEntry host = Dns.GetHostEntry(address);

                Logger.Info($"* NAT Service IP: {address}");
                //Logger.Info($"GetHostEntry({address}) returns HostName: {host.HostName}");
            }
            catch (SocketException ex)
            {
                //unknown host or
                //not every IP has a name
                //log exception (manage it)
                Logger.Error($"* NAT not resolved {ex}");
            }
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
            ManagementObject mo = new ManagementObject(@"\\.\root\cimv2:Win32_OperatingSystem=@");
            DateTime lastBootUp = ManagementDateTimeConverter.ToDateTime(mo["LastBootUpTime"].ToString());
            return DateTime.Now.ToUniversalTime() - lastBootUp.ToUniversalTime();
        }
        #endregion

        public static async Task OnDatabaseAuthenticated()
        {
            // get supported app ids
            var appids = await Database.GetAppIds();

            // build dictionary of app ids from response
            _appIdGroups = appids.ToDictionary(x => x.Name, x => x.AppIds.ToArray());
        }

        static async Task RefreshAppSettings()
        {
            try
            {
                if (!await Database.AmIAuthenticated())
                    return;

                // get supported app ids
                var appIdGroups = await Database.GetAppIds();
                if (appIdGroups == null)
                    return;

                // get settings
                foreach (var appIdGroup in appIdGroups)
                {
                    foreach (var appId in appIdGroup.AppIds)
                    {
                        var settings = await Database.GetServerSettings(appId);
                        if (settings != null)
                        {
                            if (_appSettings.TryGetValue(appId, out var appSettings))
                            {
                                appSettings.SetSettings(settings);
                            }
                            else
                            {
                                appSettings = new AppSettings(appId);
                                appSettings.SetSettings(settings);
                                _appSettings.Add(appId, appSettings);

                                // we also want to send this back to the server since this is new locally
                                // and there might be new setting fields that aren't yet on the db
                                await Database.SetServerSettings(appId, appSettings.GetSettings());
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Generates a incremental session key number
        /// </summary>
        /// <returns></returns>
        public static string GenerateSessionKey()
        {
            lock (_sessionKeyCounterLock)
            {
                return (++_sessionKeyCounter).ToString();
            }
        }

        public static AppSettings GetAppSettingsOrDefault(int appId)
        {
            if (_appSettings.TryGetValue(appId, out var appSettings))
                return appSettings;

            return _defaultAppSettings;
        }
    }
}
