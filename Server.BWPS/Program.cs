using DotNetty.Common.Internal.Logging;
using Haukcode.HighResolutionTimer;
using Microsoft.Extensions.Logging.Console;
using Newtonsoft.Json;
using NReco.Logging.File;
using Server.BWPS.Config;
using Server.Common;
using Server.Common.Logging;
using Server.Plugins;
using System.Diagnostics;

namespace Server.BWPServer
{
    class Program
    {
        public const string CONFIG_FILE = "config.json";
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<Program>();

        public static ServerSettings Settings = new ServerSettings();
        public static BWPServer BWPS = new BWPServer();

        public static PluginsManager Plugins = null;

        private static FileLoggerProvider? _fileLogger;

        public static readonly Stopwatch Stopwatch = Stopwatch.StartNew();

        private static DateTime _timeLastPluginTick = Utils.GetHighPrecisionUtcTime();

        private static int _ticks = 0;
        private static Stopwatch _sw = new Stopwatch();
        private static HighResolutionTimer _timer;
        private static DateTime _lastConfigRefresh = Utils.GetHighPrecisionUtcTime();
        private static DateTime? _lastSuccessfulDbAuth = null;

        static int metricCooldownTicks = 0;
        static string metricPrintString = null;
        static int metricIndent = 0;

        static async Task StartServerAsync()
        {
            DateTime lastConfigRefresh = Utils.GetHighPrecisionUtcTime();

            Logger.Info("**************************************************");
            string datetime = DateTime.Now.ToString("MMMM/dd/yyyy hh:mm:ss tt");
            Logger.Info($"* Launched on {datetime}");

            Task.WaitAll(BWPS.Start());
            //string gpszVersion = "rt_bwprobe ReleaseVersion 3.02.200704101920";
            string gpszVersion2 = "3.02.200704101920";
            Logger.Info($"* Bandwidth Probe Server Version {gpszVersion2}");

            //* Process ID: %d , Parent Process ID: %d

            Logger.Info($"* Server Key Type: {Settings.EncryptMessages}");

            //* Diagnostic Profiling Enabled: %d Counts

            Logger.Info("**************************************************");

            Logger.Info($"UDP BWPS started on port {Settings.BWPSPort}.");

            try
            {
                while (true)
                {
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

        static async Task TickAsync()
        {
            try
            {
#if DEBUG
                if (!_sw.IsRunning)
                    _sw.Start();
#endif

#if DEBUG
                ++_ticks;
                if (_sw.Elapsed.TotalSeconds > 5f)
                {
                    // 
                    _sw.Stop();
                    var averageMsPerTick = 1000 * (_sw.Elapsed.TotalSeconds / _ticks);
                    var error = Math.Abs(Settings.MainLoopSleepMs - averageMsPerTick) / Settings.MainLoopSleepMs;

                    //if (error > 0.1f)
                    //    Logger.Error($"Average Ms between ticks is: {averageMsPerTick} is {error * 100}% off of target {Settings.MainLoopSleepMs}");

                    //var dt = DateTime.UtcNow - Utils.GetHighPrecisionUtcTime();
                    //if (Math.Abs(dt.TotalMilliseconds) > 50)
                    //    Logger.Error($"System clock and local clock are out of sync! delta ms: {dt.TotalMilliseconds}");

                    _sw.Restart();
                    _ticks = 0;
                }
#endif

                await TimeAsync("in", async () =>
                {
                    // handle incoming
                    {
                        var tasks = new List<Task>()
                    {
                        BWPS.HandleIncomingMessages()
                    };


                        await Task.WhenAll(tasks);
                    }
                });

                await TimeAsync("plugins", async () =>
                {
                    // Tick plugins
                    if ((Utils.GetHighPrecisionUtcTime() - _timeLastPluginTick).TotalMilliseconds > Settings.PluginTickIntervalMs)
                    {
                        _timeLastPluginTick = Utils.GetHighPrecisionUtcTime();
                        await Plugins.Tick();
                    }
                });

                await TimeAsync("out", async () =>
                {
                    // handle outgoing
                    {
                        var tasks = new List<Task>()
                        {
                            BWPS.HandleOutgoingMessages()
                        };

                        await Task.WhenAll(tasks);
                    }
                });

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

                await BWPS.Stop();
                await Task.WhenAll(BWPS.Stop());
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
                /*
                // Add default localhost entry
                Settings.Universes.Add(0, new UniverseInfo()
                {
                    Name = "sample universe",
                    Endpoint = "url",
                    Port = 10075,
                    UniverseId = 1
                });
                */

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

        #region Metrics

        public static void Time(string name, Action action)
        {
            if (!Settings.Logging.LogMetrics || metricCooldownTicks > 0)
            {
                action();
                return;
            }

            // 
            long ticksAtStart = Stopwatch.ElapsedTicks;

            // insert row before action
            metricPrintString += $"({"".PadRight(metricIndent * 2, ' ') + name,-32}:    {100:#.000} ms)\n";
            int stringIndex = metricPrintString.Length - 5 - 7;

            // run
            ++metricIndent;
            try
            {
                action();
            }
            finally
            {
                --metricIndent;
            }

            //
            long ticksAfterAction = Stopwatch.ElapsedTicks;
            var actionDurationMs = (1000f * (ticksAfterAction - ticksAtStart)) / (float)System.Diagnostics.Stopwatch.Frequency;

            //
            var replacementString = actionDurationMs.ToString("#.000").PadLeft(7, ' ').Substring(0, 7);
            char[] charArr = metricPrintString.ToCharArray();
            replacementString.CopyTo(0, charArr, stringIndex, replacementString.Length);
            metricPrintString = new string(charArr);
        }

        public static async Task TimeAsync(string name, Func<Task> action)
        {
            if (!Settings.Logging.LogMetrics || metricCooldownTicks > 0)
            {
                await action();
                return;
            }

            // 
            long ticksAtStart = Stopwatch.ElapsedTicks;

            // insert row before action
            metricPrintString += $"({"".PadRight(metricIndent * 2, ' ') + name,-32}:    {100:#.000} ms)\n";
            int stringIndex = metricPrintString.Length - 5 - 7;

            // run
            ++metricIndent;
            try
            {
                await action();
            }
            finally
            {
                --metricIndent;
            }

            //
            long ticksAfterAction = Stopwatch.ElapsedTicks;
            var actionDurationMs = (1000f * (ticksAfterAction - ticksAtStart)) / (float)System.Diagnostics.Stopwatch.Frequency;

            //
            var replacementString = actionDurationMs.ToString("#.000").PadLeft(7, ' ').Substring(0, 7);
            char[] charArr = metricPrintString.ToCharArray();
            replacementString.CopyTo(0, charArr, stringIndex, replacementString.Length);
            metricPrintString = new string(charArr);
        }

        public static async Task<T> TimeAsync<T>(string name, Func<Task<T>> action)
        {
            T result;
            if (!Settings.Logging.LogMetrics || metricCooldownTicks > 0)
            {
                return await action();
            }

            // 
            long ticksAtStart = Stopwatch.ElapsedTicks;

            // insert row before action
            metricPrintString += $"({"".PadRight(metricIndent * 2, ' ') + name,-32}:    {100:#.000} ms)\n";
            int stringIndex = metricPrintString.Length - 5 - 7;

            // run
            ++metricIndent;
            try
            {
                result = await action();
            }
            finally
            {
                --metricIndent;
            }

            //
            long ticksAfterAction = Stopwatch.ElapsedTicks;
            var actionDurationMs = (1000f * (ticksAfterAction - ticksAtStart)) / (float)System.Diagnostics.Stopwatch.Frequency;

            //
            var replacementString = actionDurationMs.ToString("#.000").PadLeft(7, ' ').Substring(0, 7);
            char[] charArr = metricPrintString.ToCharArray();
            replacementString.CopyTo(0, charArr, stringIndex, replacementString.Length);
            metricPrintString = new string(charArr);

            return result;
        }

        #endregion
    }
}
