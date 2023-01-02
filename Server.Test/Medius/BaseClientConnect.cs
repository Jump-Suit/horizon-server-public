using DotNetty.Transport.Channels;
using RT.Models;
using Server.Pipeline.Attribute;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Test.Medius
{
    public abstract class BaseClientConnect : BaseClient
    {
        public BaseClientConnect(string serverIp, short serverPort) : base(serverIp, serverPort)
        {

        }

        /// <summary>
        /// When we connect we want to start the connection exchange.
        /// </summary>
        protected override async Task OnConnected(IChannel channel)
        {
            // Get ScertClient data
            if (!channel.HasAttribute(Pipeline.Constants.SCERT_CLIENT))
                channel.GetAttribute(Pipeline.Constants.SCERT_CLIENT).Set(new ScertClientAttribute());
            var scertClient = channel.GetAttribute(Pipeline.Constants.SCERT_CLIENT).Get();
            scertClient.RsaAuthKey = Program.Settings.Medius.Key;
            scertClient.CipherService.GenerateCipher(scertClient.RsaAuthKey);

            // Send hello
            await channel.WriteAndFlushAsync(new RT_MSG_CLIENT_HELLO()
            {
                Parameters = new ushort[]
                {
                    2,
                    0x71,
                    0x6E,
                    1,
                    1
                }
            });

            State = ClientState.HELLO;
        }

        /// <summary>
        /// Handle all connection messages.
        /// </summary>
        protected override async Task ProcessMessage(BaseScertMessage message, IChannel channel)
        {
            switch (message)
            {
                case RT_MSG_SERVER_HELLO serverHello:
                    {
                        if (State != ClientState.HELLO)
                            throw new Exception($"Unexpected RT_MSG_SERVER_HELLO from server. {serverHello}");

                        // Send public key
                        Queue(new RT_MSG_CLIENT_CRYPTKEY_PUBLIC()
                        {
                            PublicKey = AuthKey.N.ToByteArrayUnsigned().Reverse().ToArray()
                        });

                        State = ClientState.HANDSHAKE;
                        break;
                    }
                case RT_MSG_SERVER_CRYPTKEY_PEER serverCryptKeyPeer:
                    {
                        if (State != ClientState.HANDSHAKE)
                            throw new Exception($"Unexpected RT_MSG_SERVER_CRYPTKEY_PEER from server. {serverCryptKeyPeer}");

                        Queue(new RT_MSG_CLIENT_CONNECT_TCP()
                        {
                            AppId = ApplicationId
                        });

                        State = ClientState.CONNECT_TCP;
                        break;
                    }
                case RT_MSG_SERVER_CONNECT_ACCEPT_TCP serverConnectAcceptTcp:
                    {
                        if (State != ClientState.CONNECT_TCP)
                            throw new Exception($"Unexpected RT_MSG_SERVER_CONNECT_ACCEPT_TCP from server. {serverConnectAcceptTcp}");

                        // 

                        State = ClientState.AUTHENTICATED;
                        break;
                    }

                case RT_MSG_SERVER_ECHO serverEcho:
                    {
                        Queue(serverEcho);
                        break;
                    }

                case RT_MSG_SERVER_FORCED_DISCONNECT serverForcedDisconnect:
                case RT_MSG_CLIENT_DISCONNECT_WITH_REASON clientDisconnectWithReason:
                    {
                        await channel.CloseAsync();
                        State = ClientState.DISCONNECTED;
                        break;
                    }
            }
        }
    }
}
