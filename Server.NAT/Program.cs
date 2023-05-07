using DotNetty.Common.Internal.Logging;
using Newtonsoft.Json;
using Server.Common.Logging;
using Server.NAT.Config;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NReco.Logging.File;
using Microsoft.Extensions.Logging.Console;

namespace Server.NAT
{
    class Program
    {
        public const string CONFIG_FILE = "config.json";

        public static ServerSettings Settings = new ServerSettings();
        public static NAT NATServer = new NAT();

        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<Program>();


        private static FileLoggerProvider? _fileLogger;

        static async Task StartServerAsync()
        {

            Logger.Info($"Starting NAT on port {NATServer.Port}.");
            Task.WaitAll(NATServer.Start());
            Logger.Info($"NAT started.");


            try
            {
                while (true)
                {
                    await NATServer.Tick();

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
            await Initialize();

            // 
            await StartServerAsync();
        }

        static async Task Initialize()
        {
            RefreshConfig();

            // 
            var serializerSettings = new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };


            // Add file logger if path is valid
            if (new FileInfo(NATLogSettings.Singleton.LogPath)?.Directory?.Exists ?? false)
            {
                var loggingOptions = new FileLoggerOptions()
                {
                    Append = false,
                    FileSizeLimitBytes = NATLogSettings.Singleton.RollingFileSize,
                    MaxRollingFiles = NATLogSettings.Singleton.RollingFileCount
                };
                InternalLoggerFactory.DefaultFactory.AddProvider(_fileLogger = new FileLoggerProvider(NATLogSettings.Singleton.LogPath, loggingOptions));
                _fileLogger.MinLevel = Settings.Logging.LogLevel;
            }

            // Optionally add console logger (always enabled when debugging)
#if DEBUG
            InternalLoggerFactory.DefaultFactory.AddProvider(new ConsoleLoggerProvider((s, level) => level >= NATLogSettings.Singleton.LogLevel, true));
#else
            if (Settings.Logging.LogToConsole)
                InternalLoggerFactory.DefaultFactory.AddProvider(new ConsoleLoggerProvider((s, level) => level >= NATLogSettings.Singleton.LogLevel, true));
#endif

            string root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string subdir = root + "/logs";
            // If directory does not exist, create it. 
            if (!Directory.Exists(subdir))
            {
                Directory.CreateDirectory(subdir);
            }

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
            NATLogSettings.Singleton = Settings.Logging;

            // Update default rsa key
            //Pipeline.Attribute.ScertClientAttribute.DefaultRsaAuthKey = Settings.DefaultKey;

            // Update file logger min level
            if (_fileLogger != null)
                _fileLogger.MinLevel = Settings.Logging.LogLevel;
        }
    }
}
