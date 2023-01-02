using DotNetty.Common.Internal.Logging;
using Microsoft.Extensions.Logging.Console;
using Newtonsoft.Json;
using NReco.Logging.File;
using Server.Common.Logging;
using Server.SVO.Config;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Server.SVO
{
    class Program
    {
        public const string CONFIG_FILE = "config.json";

        public static ServerSettings Settings = new ServerSettings();
        public static SVO SVOServer = new SVO();

        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<Program>();

        private static FileLoggerProvider? _fileLogger;

        static async Task Main(string[] args)
        {
            await StartServerAsync();
            // 
            Initialize();

            //InternalLoggerFactory.DefaultFactory.AddProvider(new ConsoleLoggerProvider((s, level) => level >= LogSettings.Singleton.LogLevel, true));

        }

        static async Task StartServerAsync()
        {
            


            Console.WriteLine($"Starting SVO on SCE-RT HTTP Port {SVOServer.HttpPort} HTTPS Port {SVOServer.HttpsPort}");
            await SVOServer.Start();
            Console.WriteLine($"SVO started.");

            while (SVOServer.IsRunning)
                Thread.Sleep(500);


        }

        static void Initialize()
        {
            var builder = WebApplication.CreateBuilder();

            // Add services to the container.
            builder.Services.AddRazorPages();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();

            Configure(app);

            RefreshConfig();
        }

        static void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World");
            });
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

            // Update file logger min level
            if (_fileLogger != null)
                _fileLogger.MinLevel = Settings.Logging.LogLevel;
        }
    }
}
