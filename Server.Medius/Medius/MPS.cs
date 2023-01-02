using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;
using Org.BouncyCastle.Asn1.Ocsp;
using RT.Common;
using RT.Cryptography;
using RT.Models;
using Server.Common;
using Server.Medius.Config;
using Server.Medius.Models;
using Server.Medius.PluginArgs;
using Server.Pipeline.Attribute;
using Server.Plugins.Interface;
using System;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Server.Medius
{
    public class MPS : BaseMediusComponent
    {
        static readonly IInternalLogger _logger = InternalLoggerFactory.GetInstance<MPS>();

        protected override IInternalLogger Logger => _logger;
        public override int TCPPort => Program.Settings.MPSPort;

        public override int UDPPort => 00000;

        DateTime lastSend = Utils.GetHighPrecisionUtcTime();

        public MPS()
        {

        }

        protected override Task OnConnected(IChannel clientChannel)
        {
            // Get ScertClient data
            if (!clientChannel.HasAttribute(Pipeline.Constants.SCERT_CLIENT))
                clientChannel.GetAttribute(Pipeline.Constants.SCERT_CLIENT).Set(new ScertClientAttribute());
            var scertClient = clientChannel.GetAttribute(Pipeline.Constants.SCERT_CLIENT).Get();
            scertClient.RsaAuthKey = Program.Settings.MPSKey;
            scertClient.CipherService.GenerateCipher(Program.Settings.MPSKey);


            return base.OnConnected(clientChannel);
        }

        protected override async Task ProcessMessage(BaseScertMessage message, IChannel clientChannel, ChannelData data)
        {
            // Get ScertClient data
            var scertClient = clientChannel.GetAttribute(Pipeline.Constants.SCERT_CLIENT).Get();
            scertClient.CipherService.EnableEncryption = Program.GetAppSettingsOrDefault(data.ApplicationId).EnableEncryption;


            // 
            switch (message)
            {
                case RT_MSG_CLIENT_HELLO clientHello:
                    {
                        if (data.State > ClientState.HELLO)
                            throw new Exception($"Unexpected RT_MSG_CLIENT_HELLO from {clientChannel.RemoteAddress}: {clientHello}");

                        data.State = ClientState.HELLO;
                        Queue(new RT_MSG_SERVER_HELLO(), clientChannel);
                        break;
                    }
                case RT_MSG_CLIENT_CRYPTKEY_PUBLIC clientCryptKeyPublic:
                    {
                        if (data.State > ClientState.HANDSHAKE)
                            throw new Exception($"Unexpected RT_MSG_CLIENT_CRYPTKEY_PUBLIC from {clientChannel.RemoteAddress}: {clientCryptKeyPublic}");
                        
                        /*
                        // Ensure key is correct
                        if (!clientCryptKeyPublic.PublicKey.Reverse().SequenceEqual(Program.Settings.MPSKey.N.ToByteArrayUnsigned()))
                        {
                            Logger.Error($"Client {clientChannel.RemoteAddress} attempting to authenticate with invalid key {Encoding.Default.GetString(clientCryptKeyPublic.PublicKey)}");
                            data.State = ClientState.DISCONNECTED;
                            await clientChannel.CloseAsync();
                            break;
                        }
                        */

                        data.State = ClientState.CONNECT_1;

                        // generate new client session key
                        scertClient.CipherService.GenerateCipher(CipherContext.RSA_AUTH, clientCryptKeyPublic.PublicKey.Reverse().ToArray());
                        scertClient.CipherService.GenerateCipher(CipherContext.RC_CLIENT_SESSION);

                        Queue(new RT_MSG_SERVER_CRYPTKEY_PEER() { SessionKey = scertClient.CipherService.GetPublicKey(CipherContext.RC_CLIENT_SESSION) }, clientChannel);
                        break;
                    }
                case RT_MSG_CLIENT_CONNECT_TCP clientConnectTcp:
                    {
                        if (data.State > ClientState.CONNECT_1)
                            throw new Exception($"Unexpected RT_MSG_CLIENT_CONNECT_TCP from {clientChannel.RemoteAddress}: {clientConnectTcp}");
                        /*
                        if (clientConnectTcp.AccessToken == null)
                            throw new Exception($"AccessToken in RT_MSG_CLIENT_CONNECT_TCP from {clientChannel.RemoteAddress}: {clientConnectTcp} is null");
                        */


                        data.ApplicationId = clientConnectTcp.AppId;
                        scertClient.ApplicationID = clientConnectTcp.AppId;

                        if(scertClient.MediusVersion != 110)
                        {
                            data.ClientObject = Program.Manager.GetClientByAccessToken(clientConnectTcp.AccessToken, data.ApplicationId);
                        }


                        if (clientConnectTcp.AccessToken == null && clientConnectTcp.SessionKey == null && scertClient.MediusVersion != 110)
                        {
                            var clientObjects = Program.Manager.GetClients(data.ApplicationId);
                            data.ClientObject = clientObjects.FirstOrDefault();

                            Logger.Warn($"clientobject: {data.ClientObject}");
                        }



                        data.State = ClientState.AUTHENTICATED;
                        Queue(new RT_MSG_SERVER_CONNECT_ACCEPT_TCP()
                        {
                            PlayerId = 0,
                            ScertId = GenerateNewScertClientId(),
                            PlayerCount = 0x0001,
                            IP = (clientChannel.RemoteAddress as IPEndPoint)?.Address
                        }, clientChannel);


                        //Queue(new RT_MSG_SERVER_CONNECT_COMPLETE() { ClientCountAtConnect = 0x0001 }, clientChannel);
                        
                        #region Amplitude SERVER_CONNECT_COMPLETE
                        // Complete MPS Connection on Amplitude, R&C3: Pubeta, My Street, or ATV Offroad Fury 2 
                        if (scertClient.MediusVersion < 109)
                        {
                            Queue(new RT_MSG_SERVER_CONNECT_COMPLETE() { ClientCountAtConnect = 0x0001 }, clientChannel);
                        }
                        #endregion
                        
                        break;
                    }
                case RT_MSG_CLIENT_CONNECT_READY_TCP clientConnectReadyTcp:
                    {
                        Queue(new RT_MSG_SERVER_CONNECT_COMPLETE() { ClientCountAtConnect = 0x0001 }, clientChannel);
                        Queue(new RT_MSG_SERVER_ECHO(), clientChannel);
                        break;
                    }
                case RT_MSG_SERVER_ECHO serverEchoReply:
                    {

                        break;
                    }
                case RT_MSG_CLIENT_ECHO clientEcho:
                    {
                        Queue(new RT_MSG_CLIENT_ECHO() { Value = clientEcho.Value }, clientChannel);
                        break;
                    }
                case RT_MSG_CLIENT_APP_TOSERVER clientAppToServer:
                    {
                        if (data.State != ClientState.AUTHENTICATED)
                            throw new Exception($"Unexpected RT_MSG_CLIENT_APP_TOSERVER from {clientChannel.RemoteAddress}: {clientAppToServer}");

                        await ProcessMediusMessage(clientAppToServer.Message, clientChannel, data);
                        break;
                    }
                case RT_MSG_CLIENT_DISCONNECT _:
                    {
                        //Logger.Info($"Client id = {data.ClientObject.AccountId} disconnected by request with no specific reason\n");
                        break;
                    }
                case RT_MSG_CLIENT_DISCONNECT_WITH_REASON clientDisconnectWithReason:
                    {
                        data.State = ClientState.DISCONNECTED;
                        _ = clientChannel.CloseAsync();
                        break;
                    }
                default:
                    {
                        Logger.Warn($"UNHANDLED RT MESSAGE: {message}");
                        break;
                    }
            }

            return;
        }

        protected virtual async Task ProcessMediusMessage(BaseMediusMessage message, IChannel clientChannel, ChannelData data)
        {
            if (message == null)
                return;


            switch (message)
            {
                #region MediusServerSetAttributesRequest
                // This is a bit of a hack to get our custom dme client to authenticate
                // Our client skips MAS and just connects directly to MPS with this message
                case MediusServerSetAttributesRequest dmeSetAttributesRequest:
                    {
                        // Create DME object
                        var dme = new DMEObject(dmeSetAttributesRequest);
                        dme.ApplicationId = data.ApplicationId;
                        dme.BeginSession();
                        Program.Manager.AddDmeClient(dme);

                        // 
                        data.ClientObject = dme;

                        // 
                        data.ClientObject.OnConnected();

                        Queue(new RT_MSG_SERVER_APP()
                        {
                            Message = new MediusServerSetAttributesResponse()
                            {
                                MessageID = dmeSetAttributesRequest.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_SUCCESS
                            }
                        }, clientChannel);

                        break;
                    }
                #endregion

                #region MediusServerCreateGameWithAttributesResponse
                case MediusServerCreateGameWithAttributesResponse createGameWithAttrResponse:
                    {
                        int gameId = int.Parse(createGameWithAttrResponse.MessageID.Value.Split('-')[0]);
                        int accountId = int.Parse(createGameWithAttrResponse.MessageID.Value.Split('-')[1]);
                        string msgId = createGameWithAttrResponse.MessageID.Value.Split('-')[2];
                        var game = Program.Manager.GetGameByGameId(gameId);
                        var rClient = Program.Manager.GetClientByAccountId(accountId, data.ClientObject.ApplicationId);

                        if (!createGameWithAttrResponse.IsSuccess)
                        {
                            rClient?.Queue(new MediusCreateGameResponse()
                            {
                                MessageID = new MessageId(msgId),
                                StatusCode = MediusCallbackStatus.MediusFail
                            });

                            await game.EndGame();
                        }
                        else
                        {
                            game.DMEWorldId = createGameWithAttrResponse.WorldID;
                            await game.GameCreated();
                            rClient?.Queue(new MediusCreateGameResponse()
                            {
                                MessageID = new MessageId(msgId),
                                StatusCode = MediusCallbackStatus.MediusSuccess,
                                MediusWorldID = game.Id
                            });

                            // Send to plugins
                            await Program.Plugins.OnEvent(PluginEvent.MEDIUS_GAME_ON_CREATED, new OnPlayerGameArgs() { Player = rClient, Game = game });
                        }

                        break;
                    }
                #endregion

                #region MediusServerJoinGameResponse
                case MediusServerJoinGameResponse joinGameResponse:
                    {
                        int gameId = int.Parse(joinGameResponse.MessageID.Value.Split('-')[0]);
                        int accountId = int.Parse(joinGameResponse.MessageID.Value.Split('-')[1]);
                        string msgId = joinGameResponse.MessageID.Value.Split('-')[2];
                        var game = Program.Manager.GetGameByGameId(gameId);
                        var rClient = Program.Manager.GetClientByAccountId(accountId, data.ClientObject.ApplicationId);


                        if (!joinGameResponse.IsSuccess)
                        {
                            rClient?.Queue(new MediusJoinGameResponse()
                            {
                                MessageID = new MessageId(msgId),
                                StatusCode = MediusCallbackStatus.MediusFail
                            });
                        }
                        else
                        {

                            if (game.GameHostType == MediusGameHostType.MediusGameHostPeerToPeer &&
                                game.netAddressList.AddressList[0].AddressType == NetAddressType.NetAddressTypeSignalAddress)
                            {

                                // Join game P2P
                                await rClient?.JoinGameP2P(game);

                                // 
                                rClient?.Queue(new MediusJoinGameResponse()
                                {
                                    MessageID = new MessageId(msgId),
                                    StatusCode = MediusCallbackStatus.MediusSuccess,
                                    GameHostType = game.GameHostType,
                                    ConnectInfo = new NetConnectionInfo()
                                    {
                                        AccessKey = joinGameResponse.AccessKey,
                                        SessionKey = rClient.SessionKey,
                                        WorldID = game.WorldID,
                                        ServerKey = joinGameResponse.pubKey,
                                        AddressList = new NetAddressList()
                                        {
                                            AddressList = new NetAddress[Constants.NET_ADDRESS_LIST_COUNT]
                                            {
                                                new NetAddress() { Address = game.netAddressList.AddressList[0].Address, Port = game.netAddressList.AddressList[0].Port, AddressType = NetAddressType.NetAddressTypeSignalAddress},
                                                new NetAddress() { AddressType = NetAddressType.NetAddressNone},
                                            }
                                        },
                                        Type = NetConnectionType.NetConnectionTypePeerToPeerUDP
                                    }
                                });
                            }
                            else if(game.GameHostType == MediusGameHostType.MediusGameHostPeerToPeer &&
                                game.netAddressList.AddressList[0].AddressType == NetAddressType.NetAddressTypeExternal &&
                                game.netAddressList.AddressList[1].AddressType == NetAddressType.NetAddressTypeInternal
                                )
                            {
                                // Join game P2P
                                await rClient?.JoinGameP2P(game);

                                // 
                                rClient?.Queue(new MediusJoinGameResponse()
                                {
                                    MessageID = new MessageId(msgId),
                                    StatusCode = MediusCallbackStatus.MediusSuccess,
                                    GameHostType = game.GameHostType,
                                    ConnectInfo = new NetConnectionInfo()
                                    {
                                        AccessKey = joinGameResponse.AccessKey,
                                        SessionKey = rClient.SessionKey,
                                        WorldID = game.WorldID,
                                        ServerKey = joinGameResponse.pubKey,
                                        AddressList = new NetAddressList()
                                        {
                                            AddressList = new NetAddress[Constants.NET_ADDRESS_LIST_COUNT]
                                            {
                                                new NetAddress() { Address = game.netAddressList.AddressList[0].Address, Port = game.netAddressList.AddressList[0].Port, AddressType = NetAddressType.NetAddressTypeExternal},
                                                new NetAddress() { Address = game.netAddressList.AddressList[1].Address, Port = game.netAddressList.AddressList[1].Port, AddressType = NetAddressType.NetAddressTypeInternal},
                                            }
                                        },
                                        Type = NetConnectionType.NetConnectionTypePeerToPeerUDP
                                    }
                                });
                            } else {

                                // Join game DME
                                await rClient?.JoinGame(game, joinGameResponse.DmeClientIndex);

                                if (data.ClientObject.MediusVersion == 108)
                                {

                                    // 
                                    rClient?.Queue(new MediusJoinGameResponse()
                                    {
                                        MessageID = new MessageId(msgId),
                                        StatusCode = MediusCallbackStatus.MediusSuccess,
                                        GameHostType = game.GameHostType,
                                        ConnectInfo = new NetConnectionInfo()
                                        {
                                            AccessKey = joinGameResponse.AccessKey,
                                            SessionKey = rClient.SessionKey,
                                            WorldID = game.DMEWorldId,
                                            ServerKey = joinGameResponse.pubKey,
                                            AddressList = new NetAddressList()
                                            {
                                                AddressList = new NetAddress[Constants.NET_ADDRESS_LIST_COUNT]
                                                {
                                                    new NetAddress() { Address = (data.ClientObject as DMEObject).IP.MapToIPv4().ToString(), Port = (data.ClientObject as DMEObject).Port, AddressType = NetAddressType.NetAddressTypeExternal},
                                                    new NetAddress() { AddressType = NetAddressType.NetAddressNone},
                                                }
                                            },
                                            Type = NetConnectionType.NetConnectionTypeClientServerTCP
                                        }
                                    });
                                } else {

                                    // 
                                    rClient?.Queue(new MediusJoinGameResponse()
                                    {
                                        MessageID = new MessageId(msgId),
                                        StatusCode = MediusCallbackStatus.MediusSuccess,
                                        GameHostType = game.GameHostType,
                                        ConnectInfo = new NetConnectionInfo()
                                        {
                                            AccessKey = joinGameResponse.AccessKey,
                                            SessionKey = rClient.SessionKey,
                                            WorldID = game.DMEWorldId,
                                            ServerKey = joinGameResponse.pubKey,
                                            AddressList = new NetAddressList()
                                            {
                                                AddressList = new NetAddress[Constants.NET_ADDRESS_LIST_COUNT]
                                                {
                                                new NetAddress() { Address = (data.ClientObject as DMEObject).IP.MapToIPv4().ToString(), Port = (data.ClientObject as DMEObject).Port, AddressType = NetAddressType.NetAddressTypeExternal},
                                                new NetAddress() { AddressType = NetAddressType.NetAddressNone},
                                                }
                                            },
                                            Type = NetConnectionType.NetConnectionTypeClientServerTCPAuxUDP
                                        }
                                    });
                                }

                            }

                            /// For Legacy Medius v1.50 clients that DO NOT 
                            /// send a ServerConnectNotificationConnect when creating a game
                            if(data.ClientObject.MediusVersion < 109 && data.ClientObject.ApplicationId != 10394)
                            {
                                await game.OnMediusJoinGameResponse(rClient.SessionKey);
                            }

                            /*
                            if(rClient.WorldId == null)
                            {
                                // 
                                rClient?.Queue(new MediusJoinGameResponse()
                                {
                                    MessageID = new MessageId(msgId),
                                    StatusCode = MediusCallbackStatus.MediusSuccess,
                                    GameHostType = game.GameHostType,
                                    ConnectInfo = new NetConnectionInfo()
                                    {
                                        AccessKey = joinGameResponse.AccessKey,
                                        SessionKey = rClient.SessionKey,
                                        WorldID = game.DMEWorldId,
                                        ServerKey = joinGameResponse.pubKey,
                                        AddressList = new NetAddressList()
                                        {
                                            AddressList = new NetAddress[Constants.NET_ADDRESS_LIST_COUNT]
                                            {
                                                new NetAddress() { Address = game.netAddressList.AddressList[1].Address, Port = game.netAddressList.AddressList[1].Port, AddressType = NetAddressType.NetAddressTypeExternal},
                                                new NetAddress() { AddressType = NetAddressType.NetAddressNone},
                                            }
                                        },
                                        Type = NetConnectionType.NetConnectionTypePeerToPeerUDP
                                    }
                                });
                            } else {
                                // 
                                rClient?.Queue(new MediusJoinGameResponse()
                                {
                                    MessageID = new MessageId(msgId),
                                    StatusCode = MediusCallbackStatus.MediusSuccess,
                                    GameHostType = game.GameHostType,
                                    ConnectInfo = new NetConnectionInfo()
                                    {
                                        AccessKey = joinGameResponse.AccessKey,
                                        SessionKey = rClient.SessionKey,
                                        WorldID = game.DMEWorldId,
                                        ServerKey = joinGameResponse.pubKey,
                                        AddressList = new NetAddressList()
                                        {
                                            AddressList = new NetAddress[Constants.NET_ADDRESS_LIST_COUNT]
                                            {
                                                new NetAddress() { Address = (data.ClientObject as DMEObject).IP.MapToIPv4().ToString(), Port = (uint)(data.ClientObject as DMEObject).Port, AddressType = NetAddressType.NetAddressTypeExternal},
                                                new NetAddress() { AddressType = NetAddressType.NetAddressNone},
                                            }
                                        },
                                        Type = NetConnectionType.NetConnectionTypeClientServerTCPAuxUDP
                                    }
                                });
                            }
                            */
                        }
                        break;
                    }
                #endregion

                #region MediusServerCreateGameOnSelfRequest
                case MediusServerCreateGameOnSelfRequest serverCreateGameOnSelfRequest:
                    {
                        data.ClientObject.ApplicationId = data.ApplicationId;

                        // Create DME object
                        var dme = new DMEObject(data.ClientObject, serverCreateGameOnSelfRequest);

                        dme.BeginSession();
                        Program.Manager.AddDmeClient(dme);

                        data.ClientObject.OnConnected();
                        data.ClientObject.KeepAliveUntilNextConnection();

                        // validate name
                        if (!Program.PassTextFilter(data.ApplicationId, TextFilterContext.GAME_NAME, Convert.ToString(serverCreateGameOnSelfRequest.GameName)))
                        {
                            data.ClientObject.Queue(new MediusServerCreateGameOnMeResponse()
                            {
                                MessageID = serverCreateGameOnSelfRequest.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_INVALID_ARG
                            });
                            return;
                        }

                        // Send to plugins
                        await Program.Plugins.OnEvent(PluginEvent.MEDIUS_PLAYER_ON_CREATE_GAME, new OnPlayerRequestArgs() { Player = data.ClientObject, Request = serverCreateGameOnSelfRequest });

                        await Program.Manager.CreateGameP2P(data.ClientObject, serverCreateGameOnSelfRequest, clientChannel, dme);
                        break;
                    }
                #endregion

                #region MediusServerCreateGameOnSelfRequest0
                case MediusServerCreateGameOnSelfRequest0 serverCreateGameOnSelfRequest0:
                    {
                        data.ClientObject.ApplicationId = data.ApplicationId;

                        // Create DME object
                        var dme = new DMEObject(data.ClientObject, serverCreateGameOnSelfRequest0);

                        dme.BeginSession();
                        Program.Manager.AddDmeClient(dme);

                        data.ClientObject.OnConnected();
                        data.ClientObject.KeepAliveUntilNextConnection();

                        // validate name
                        if (!Program.PassTextFilter(data.ApplicationId, TextFilterContext.GAME_NAME, Convert.ToString(serverCreateGameOnSelfRequest0.GameName)))
                        {
                            data.ClientObject.Queue(new MediusServerCreateGameOnMeResponse()
                            {
                                MessageID = serverCreateGameOnSelfRequest0.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_INVALID_ARG
                            });
                            return;
                        }

                        // Send to plugins
                        await Program.Plugins.OnEvent(PluginEvent.MEDIUS_PLAYER_ON_CREATE_GAME, new OnPlayerRequestArgs() { Player = data.ClientObject, Request = serverCreateGameOnSelfRequest0 });

                        await Program.Manager.CreateGameP2P(data.ClientObject, serverCreateGameOnSelfRequest0, clientChannel, dme);
                        break;
                    }
                #endregion

                #region MediusServerCreateGameOnMeRequest
                case MediusServerCreateGameOnMeRequest serverCreateGameOnMeRequest:
                    {;
                        data.ClientObject.ApplicationId = data.ApplicationId;

                        // Create DME object
                        var dme = new DMEObject(data.ClientObject, serverCreateGameOnMeRequest);

                        dme.BeginSession();
                        Program.Manager.AddDmeClient(dme);

                        // 
                        //data.ClientObject = dme;

                        data.ClientObject.OnConnected();

                        // validate name
                        if (!Program.PassTextFilter(data.ApplicationId, TextFilterContext.GAME_NAME, Convert.ToString(serverCreateGameOnMeRequest.GameName)))
                        {
                            data.ClientObject.Queue(new MediusServerCreateGameOnMeResponse()
                            {
                                MessageID = serverCreateGameOnMeRequest.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_INVALID_ARG
                            });
                            return;
                        }

                        // Send to plugins
                        await Program.Plugins.OnEvent(PluginEvent.MEDIUS_PLAYER_ON_CREATE_GAME, new OnPlayerRequestArgs() { Player = data.ClientObject, Request = serverCreateGameOnMeRequest });

                        await Program.Manager.CreateGameP2P(data.ClientObject, serverCreateGameOnMeRequest, clientChannel, dme);

                        break;
                    }

                #endregion

                #region MediusServerMoveGameWorldOnMeRequest
                case MediusServerMoveGameWorldOnMeRequest serverMoveGameWorldOnMeRequest:
                    {
                        //Fetch Current Game, and Update it with the new one
                        var game = Program.Manager.GetGameByGameId(serverMoveGameWorldOnMeRequest.CurrentMediusWorldID);
                        if(game.WorldID != serverMoveGameWorldOnMeRequest.CurrentMediusWorldID)
                        {
                            data.ClientObject.Queue(new MediusServerMoveGameWorldOnMeResponse()
                            {
                                MessageID = serverMoveGameWorldOnMeRequest.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_UNSUCCESSFUL,
                            });
                        } else {
                            game.WorldID = serverMoveGameWorldOnMeRequest.NewGameWorldID;
                            game.netAddressList.AddressList[0] = serverMoveGameWorldOnMeRequest.AddressList.AddressList[0];
                            game.netAddressList.AddressList[1] = serverMoveGameWorldOnMeRequest.AddressList.AddressList[1];

                            Logger.Info("MediusServerMoveGameWorldOnMeRequest Successful");
                            data.ClientObject.Queue(new MediusServerMoveGameWorldOnMeResponse()
                            {
                                MessageID = serverMoveGameWorldOnMeRequest.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_SUCCESS,
                                MediusWorldID = serverMoveGameWorldOnMeRequest.NewGameWorldID
                            });
                        }
                        break;
                    }
                #endregion 

                #region MediusServerEndGameOnMeRequest
                /// <summary>
                /// This structure uses the game world ID as MediusWorldID. This should not be confused with the net World ID on this host.
                /// </summary>
                case MediusServerEndGameOnMeRequest serverEndGameOnMeRequest:
                    {


                        data.ClientObject.Queue(new MediusServerEndGameOnMeResponse()
                        {
                            MessageID = serverEndGameOnMeRequest.MessageID,
                            Confirmation = MGCL_ERROR_CODE.MGCL_SUCCESS,
                        });
                        break;
                    }
                #endregion

                #region MediusServerReport
                case MediusServerReport serverReport:
                    {
                        (data.ClientObject as DMEObject)?.OnServerReport(serverReport);
                        data.ClientObject.OnConnected();
                        data.ClientObject.KeepAliveUntilNextConnection();
                        //Logger.Info($"ServerReport SessionKey {serverReport.SessionKey} MaxWorlds {serverReport.MaxWorlds} MaxPlayersPerWorld {serverReport.MaxPlayersPerWorld} TotalWorlds {serverReport.ActiveWorldCount} TotalPlayers {serverReport.TotalActivePlayers} Alert {serverReport.AlertLevel} ConnIndex {data.ClientObject.DmeId} WorldID {data.ClientObject.WorldId}");
                        break;
                    }
                #endregion

                #region MediusServerConnectNotification
                case MediusServerConnectNotification connectNotification:
                    {
                        Logger.Info("MediusServerConnectNotification Received");

                        //MediusServerConnectNotification -  sent Notify msg to MUM
                        //DmeServerGetConnectKeys
                        if (data.ClientObject.SessionKey == null)
                        {
                            Logger.Warn($"ServerConnectNotificationHandler - DmeServerGetConnectKeys error = {data.ClientObject.SessionKey} is null");
                            return;
                        } else {
                            Program.Manager.GetGameByDmeWorldId(connectNotification.PlayerSessionKey, (int)connectNotification.MediusWorldUID)?.OnMediusServerConnectNotification(connectNotification);
                        }
                        break;
                    }
                #endregion

                #region MediusServerDisconnectPlayerRequest UNIMPLEMENTED
                case MediusServerDisconnectPlayerRequest serverDisconnectPlayerRequest:
                    {

                        break;
                    }
                #endregion

                #region MediusServerEndGameRequest
                case MediusServerEndGameRequest endGameRequest:
                    {
                        var game = Program.Manager.GetGameByGameId(endGameRequest.WorldID);

                        if(game != null && endGameRequest.BrutalFlag == true)
                        {
                            await game.EndGame();

                            data.ClientObject.Queue(new MediusServerEndGameResponse()
                            {
                                MessageID = endGameRequest.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_SUCCESS
                            });
                        } else if (game != null && endGameRequest.BrutalFlag == false) {
                            data.ClientObject.Queue(new MediusServerEndGameResponse()
                            {
                                MessageID = endGameRequest.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_SUCCESS,
                            });
                        } else {
                            data.ClientObject.Queue(new MediusServerEndGameResponse()
                            {
                                MessageID = endGameRequest.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_INVALID_ARG
                            });
                        }
                        break;
                    }
                #endregion

                #region MediusServerSessionEndRequest
                case MediusServerSessionEndRequest sessionEndRequest:
                    {
                        Logger.Info("ServerSessionEndRequest Received");

                        //DmeServerGetConnectKeys
                        if(data.ClientObject.SessionKey == null)
                        {
                            Logger.Warn($"MediusServerSessionEndRequestHandler: DmeServerGetConnectKeys error {data.ClientObject.SessionKey} is null");
                            data?.ClientObject.Queue(new MediusServerSessionEndResponse()
                            {
                                MessageID = sessionEndRequest.MessageID,
                                ErrorCode = MGCL_ERROR_CODE.MGCL_SESSIONEND_FAILED
                            });
                        } else {

                            data?.ClientObject.KeepAliveUntilNextConnection();

                            //Success
                            Logger.Info("Server Session End Success");
                            data?.ClientObject.Queue(new MediusServerSessionEndResponse()
                            {
                                MessageID = sessionEndRequest.MessageID,
                                ErrorCode = MGCL_ERROR_CODE.MGCL_SUCCESS
                            });
                        }

                        break;
                    }
                #endregion

                default:
                    {
                        Logger.Warn($"Unhandled Medius Message: {message}");
                        break;
                    }
            }
        }

        public DMEObject GetFreeDme(int appId)
        {
            try
            {
                return _scertHandler.Group
                    .Select(x => _channelDatas[x.Id.AsLongText()]?.ClientObject)
                    .Where(x => x is DMEObject && x != null && (x.ApplicationId == appId || x.ApplicationId == 0))
                    .MinBy(x => (x as DMEObject).CurrentWorlds) as DMEObject;
            }
            catch (Exception e)
            {
                Logger.Error("No DME Game Server assigned to this AppId\n", e);
            }

            return null;
        }

        //Actual DME flow unimplemented
        public DMEObject ReserveDMEObject(MediusServerSessionBeginRequest request)
        {
            var dme = new DMEObject(request);
            dme.BeginSession();
            Program.Manager.AddDmeClient(dme);
            return dme;
        }

        public DMEObject ReserveDMEObject(ClientObject client, MediusServerCreateGameOnSelfRequest request)
        {
            var dme = new DMEObject(client, request);
            dme.BeginSession();
            Program.Manager.AddDmeClient(dme);
            return dme;
        }


        public DMEObject ReserveDMEPlayerObject(ClientObject client, MediusServerCreateGameOnSelfRequest0 request)
        {
            var dme = new DMEObject(client, request);
            dme.BeginSession();
            Program.Manager.AddDmeClient(dme);
            return dme;
        }

        public DMEObject ReserveDMEPlayerObject(ClientObject client, MediusServerCreateGameOnMeRequest request)
        {
            var dme = new DMEObject(client, request);
            dme.BeginSession();
            Program.Manager.AddDmeClient(dme);
            return dme;
        }

        public ClientObject ReserveClient(MediusServerSessionBeginRequest mgclSessionBeginRequest)
        {
            var client = new ClientObject();
            client.BeginSession();
            return client;
        }
    }
}
