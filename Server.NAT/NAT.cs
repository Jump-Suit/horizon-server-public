﻿using DotNetty.Common.Internal.Logging;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Server.Pipeline.Udp;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Server.NAT
{
    /// <summary>
    /// SCE-RT NAT.
    /// </summary>
    public class NAT
    {
        public int Port => Program.Settings.Port;
        public bool IsRunning => _boundChannel != null && _boundChannel.Active;

        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<NAT>();

        protected IEventLoopGroup _workerGroup = null;
        protected IChannel _boundChannel = null;
        protected SimpleDatagramHandler _scertHandler = null;

        public NAT()
        {

        }

        /// <summary>
        /// Start the SCE-RT NAT UDP Server.
        /// </summary>
        public async Task Start()
        {
            //
            _workerGroup = new MultithreadEventLoopGroup();

            _scertHandler = new SimpleDatagramHandler();

            // Queue all incoming messages
            _scertHandler.OnChannelMessage += (channel, message) =>
            {
                // Send ip and port back if the last byte isn't 0xD4
                if (message.Content.ReadableBytes == 4 && message.Content.GetByte(message.Content.ReaderIndex + 3) != 0xD4)
                {
                    Logger.Info($"Recieved External IP {(message.Sender as IPEndPoint).Address.MapToIPv4()} & Port {(ushort)(message.Sender as IPEndPoint).Port} request, sending their IP & Port as response!");
                    var buffer = channel.Allocator.Buffer(6);

                    buffer.WriteBytes((message.Sender as IPEndPoint).Address.MapToIPv4().GetAddressBytes());
                    buffer.WriteUnsignedShort((ushort)(message.Sender as IPEndPoint).Port);
                    channel.WriteAndFlushAsync(new DatagramPacket(buffer, message.Sender));
                }
            };

            var bootstrap = new Bootstrap();
            bootstrap
                .Group(_workerGroup)
                .Channel<SocketDatagramChannel>()
                .Handler(new LoggingHandler(LogLevel.INFO))
                .Handler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;

                    pipeline.AddLast(_scertHandler);
                }));

            _boundChannel = await bootstrap.BindAsync(Port);
        }

        /// <summary>
        /// Stop the server.
        /// </summary>
        public virtual async Task Stop()
        {
            try
            {
                await _boundChannel.CloseAsync();
            }
            finally
            {
                await Task.WhenAll(
                        _workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
            }
        }

        public Task Tick()
        {
            return Task.CompletedTask;
        }
    }
}