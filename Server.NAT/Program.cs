using DotNetty.Common.Internal.Logging;
using Newtonsoft.Json;
using Server.NAT.Config;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Server.NAT
{
    class Program
    {
        public const string CONFIG_FILE = "config.json";

        public static ServerSettings Settings = new ServerSettings();
        public static NAT NATServer = new NAT();

        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<Program>();


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
            // 
            var serializerSettings = new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };


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
    }
}
