﻿using System;
using System.Collections.Generic;

using Lidgren.Network;

namespace CoopServer
{
    public class Client
    {
        public long ID = 0;
        public float Latency = 0.0f;
        public PlayerData Player;
        private readonly Dictionary<string, object> CustomData = new();
        internal readonly Dictionary<long, Action<object>> Callbacks = new();

        #region CUSTOMDATA FUNCTIONS
        public void SetData<T>(string name, T data)
        {
            if (HasData(name))
            {
                CustomData[name] = data;
            }
            else
            {
                CustomData.Add(name, data);
            }
        }

        public bool HasData(string name)
        {
            return CustomData.ContainsKey(name);
        }

        public T GetData<T>(string name)
        {
            return HasData(name) ? (T)CustomData[name] : default;
        }

        public void RemoveData(string name)
        {
            if (HasData(name))
            {
                CustomData.Remove(name);
            }
        }
        #endregion

        #region FUNCTIONS
        public void Kick(string[] reason)
        {
            Server.MainNetServer.Connections.Find(x => x.RemoteUniqueIdentifier == ID)?.Disconnect(string.Join(" ", reason));
        }

        public void SendChatMessage(string message, string from = "Server")
        {
            try
            {
                NetConnection userConnection = Server.MainNetServer.Connections.Find(x => x.RemoteUniqueIdentifier == ID);
                if (userConnection == null)
                {
                    return;
                }

                ChatMessagePacket packet = new()
                {
                    Username = from,
                    Message = message
                };

                NetOutgoingMessage outgoingMessage = Server.MainNetServer.CreateMessage();
                packet.PacketToNetOutGoingMessage(outgoingMessage);
                Server.MainNetServer.SendMessage(outgoingMessage, userConnection, NetDeliveryMethod.ReliableOrdered, 0);
            }
            catch (Exception e)
            {
                Logging.Error($">> {e.Message} <<>> {e.Source ?? string.Empty} <<>> {e.StackTrace ?? string.Empty} <<");
            }
        }

        public void SendNativeCall(ulong hash, params object[] args)
        {
            try
            {
                NetConnection userConnection = Server.MainNetServer.Connections.Find(x => x.RemoteUniqueIdentifier == ID);
                if (userConnection == null)
                {
                    return;
                }

                List<NativeArgument> arguments = Util.ParseNativeArguments(args);
                if (arguments == null)
                {
                    return;
                }

                NativeCallPacket packet = new()
                {
                    Hash = hash,
                    Args = arguments
                };

                NetOutgoingMessage outgoingMessage = Server.MainNetServer.CreateMessage();
                packet.PacketToNetOutGoingMessage(outgoingMessage);
                Server.MainNetServer.SendMessage(outgoingMessage, userConnection, NetDeliveryMethod.ReliableOrdered, 0);
            }
            catch (Exception e)
            {
                Logging.Error($">> {e.Message} <<>> {e.Source ?? string.Empty} <<>> {e.StackTrace ?? string.Empty} <<");
            }
        }

        public void SendNativeResponse(Action<object> callback, ulong hash, Type type, params object[] args)
        {
            try
            {
                NetConnection userConnection = Server.MainNetServer.Connections.Find(x => x.RemoteUniqueIdentifier == ID);
                if (userConnection == null)
                {
                    return;
                }

                NativeArgument returnType = null;
                Type typeOf = type;
                if (typeOf == typeof(int))
                {
                    returnType = new IntArgument();
                }
                else if (typeOf == typeof(bool))
                {
                    returnType = new BoolArgument();
                }
                else if (typeOf == typeof(float))
                {
                    returnType = new FloatArgument();
                }
                else if (typeOf == typeof(string))
                {
                    returnType = new StringArgument();
                }
                else if (typeOf == typeof(LVector3))
                {
                    returnType = new LVector3Argument();
                }
                else
                {
                    return;
                }

                List<NativeArgument> arguments = Util.ParseNativeArguments(args);
                if (arguments == null)
                {
                    return;
                }

                long id = 0;
                Callbacks.Add(id = Environment.TickCount64, callback);

                NativeResponsePacket packet = new()
                {
                    Hash = hash,
                    Args = arguments,
                    Type = returnType,
                    ID = id
                };

                NetOutgoingMessage outgoingMessage = Server.MainNetServer.CreateMessage();
                packet.PacketToNetOutGoingMessage(outgoingMessage);
                Server.MainNetServer.SendMessage(outgoingMessage, userConnection, NetDeliveryMethod.ReliableOrdered, 0);
            }
            catch (Exception e)
            {
                Logging.Error($">> {e.Message} <<>> {e.Source ?? string.Empty} <<>> {e.StackTrace ?? string.Empty} <<");
            }
        }

        public void SendModPacket(string mod, byte customID, byte[] bytes)
        {
            try
            {
                NetConnection userConnection = Server.MainNetServer.Connections.Find(x => x.RemoteUniqueIdentifier == ID);
                if (userConnection == null)
                {
                    return;
                }

                NetOutgoingMessage outgoingMessage = Server.MainNetServer.CreateMessage();
                new ModPacket()
                {
                    ID = 0,
                    Target = 0,
                    Mod = mod,
                    CustomPacketID = customID,
                    Bytes = bytes
                }.PacketToNetOutGoingMessage(outgoingMessage);
                Server.MainNetServer.SendMessage(outgoingMessage, userConnection, NetDeliveryMethod.ReliableOrdered, 0);
                Server.MainNetServer.FlushSendQueue();
            }
            catch (Exception e)
            {
                Logging.Error($">> {e.Message} <<>> {e.Source ?? string.Empty} <<>> {e.StackTrace ?? string.Empty} <<");
            }
        }
        #endregion
    }
}
