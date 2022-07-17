using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Server.Pipeline.Udp;

namespace Server.SVO
{
    /// <summary>
    /// implemented SVO.
    /// </summary>
    public class SVO
    {
        public int HttpPort => Program.Settings.SVOHttpPort;
        public int HttpsPort => Program.Settings.SVOHttpsPort;

        public bool IsRunning => _boundChannel != null && _boundChannel.Active;

        protected IEventLoopGroup _workerGroup;
        protected IChannel _boundChannel;
        protected SimpleDatagramHandler _scertHandler;

        public SVO()
        {

        }

        /// <summary>
        /// Start the SVO HTTP/HTTPS Server.
        /// </summary>
        public async Task Start()
        {
            //
            _workerGroup = new MultithreadEventLoopGroup();

            _scertHandler = new SimpleDatagramHandler();

            // Queue all incoming messages
            _scertHandler.OnChannelMessage += (channel, message) =>
            {

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


            _boundChannel = await bootstrap.BindAsync(HttpPort);
            _boundChannel = await bootstrap.BindAsync(HttpsPort);
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
