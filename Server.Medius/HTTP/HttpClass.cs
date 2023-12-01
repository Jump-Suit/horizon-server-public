// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DotNetty.Codecs.Http;
using DotNetty.Common.Internal.Logging;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
namespace Server.Medius.HTTPService
{
    public class HttpClass
    {
        public static bool httpstarted = false;

        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<HttpClass>();

        public static async Task RunServerAsync(int port, bool https = false)
        {
            IEventLoopGroup group;
            IEventLoopGroup workGroup;
            group = new MultithreadEventLoopGroup(1);
            workGroup = new MultithreadEventLoopGroup();

            X509Certificate2? tlsCertificate = null;
            if (https)
                tlsCertificate = new X509Certificate2(Program.Settings.HTTPSCertificateFile, "qwerty");
            try
            {
                var bootstrap = new ServerBootstrap();
                bootstrap.Group(group, workGroup);

                bootstrap.Channel<TcpServerSocketChannel>();

                bootstrap
                    .Option(ChannelOption.SoBacklog, 8192)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        if (tlsCertificate != null)
                            pipeline.AddLast(TlsHandler.Server(tlsCertificate));
                        pipeline.AddLast("encoder", new HttpResponseEncoder());
                        pipeline.AddLast("decoder", new HttpRequestDecoder(4096, 8192, 8192, false));
                        pipeline.AddLast("aggregator", new HttpObjectAggregator(1048576));
                        pipeline.AddLast("handler", new CrudServerHandler());
                    }));

                IChannel? bootstrapChannel = null;

                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

                var ippaddress = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

                bootstrapChannel = await bootstrap.BindAsync(ippaddress, port);

                httpstarted = true;

                Logger.Info($"[HTTPSERVICE] - Server Listening on {bootstrapChannel.LocalAddress}");

                while (httpstarted)
                {

                }

                await bootstrapChannel.CloseAsync();
            }
            finally
            {
                group.ShutdownGracefullyAsync().Wait();
            }
        }
    }
}
