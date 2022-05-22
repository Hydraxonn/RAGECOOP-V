﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Lidgren.Network;
using RageCoop.Core;
using GTA;
using GTA.Native;
using GTA.Math;

namespace RageCoop.Client
{
    public partial class Networking
    {
        /*
        public static void MakePlayer()
        {
            // Create entity for local player
            Ped p = Game.Player.Character;
            Main.Logger.Debug($"Creating SyncEntity for player, handle:{p.Handle}");
            CharacterEntity c = new CharacterEntity(p);
            Main.Characters.Add(c.ID, c);
            Main.MyPlayerID=c.OwnerID=c.ID;
            Main.Logger.Debug($"My player ID is:{c.ID}");
            Main.MainPlayerList.SetPlayer(new PlayerData { PedID=c.ID, Username=Main.MainSettings.Username });

        }
        */
        #region -- SEND --
        public void SendPed(SyncedPed c)
        {
            Ped p = c.MainPed;
            NetOutgoingMessage outgoingMessage = Client.CreateMessage();

            NetDeliveryMethod messageType = NetDeliveryMethod.UnreliableSequenced;
            int connectionChannel = (byte)ConnectionChannel.CharacterSync;
            
            var packet=new Packets.PedSync()
            {
                ID =c.ID,
                Health = p.Health,
                Position = p.Position.ToLVector(),
                Rotation = p.Rotation.ToLVector(),
                Velocity = p.Velocity.ToLVector(),
                Speed = p.GetPedSpeed(),
                CurrentWeaponHash = (uint)p.Weapons.Current.Hash,
                Flag = p.GetPedFlags(),
                Heading=p.Heading,
            };
            if (packet.Flag.HasFlag(PedDataFlags.IsAiming))
            {
                packet.AimCoords = p.GetAimCoord().ToLVector();
            }
            if (packet.Flag.HasFlag(PedDataFlags.IsRagdoll))
            {
                packet.RotationVelocity=p.RotationVelocity.ToLVector();
            }
            packet.Pack(outgoingMessage);
            Client.SendMessage(outgoingMessage, messageType, connectionChannel);
        }
        public void SendPedState(SyncedPed c)
        {
            Ped p = c.MainPed;

            NetOutgoingMessage outgoingMessage = Client.CreateMessage();

            NetDeliveryMethod messageType = NetDeliveryMethod.UnreliableSequenced;
            int connectionChannel = (byte)ConnectionChannel.CharacterSync;
            new Packets.PedStateSync()
            {
                ID = c.ID,
                OwnerID=c.OwnerID,
                Clothes=p.GetPedClothes(),
                ModelHash=p.Model.Hash,
                WeaponComponents=p.Weapons.Current.GetWeaponComponents(),
            }.Pack(outgoingMessage);
            Client.SendMessage(outgoingMessage, messageType, connectionChannel);
        }
        public void SendVehicle(SyncedVehicle v)
        {
            Vehicle veh = v.MainVehicle;

            NetOutgoingMessage outgoingMessage = Client.CreateMessage();

            NetDeliveryMethod messageType = NetDeliveryMethod.UnreliableSequenced;
            int connectionChannel = (byte)ConnectionChannel.VehicleSync;
            new Packets.VehicleSync()
            {
                ID =v.ID,
                SteeringAngle = veh.SteeringAngle,
                Position = veh.Position.ToLVector(),
                Rotation = veh.Rotation.ToLVector(),
                Velocity = veh.Velocity.ToLVector(),
                RotationVelocity=veh.RotationVelocity.ToLVector(),
                ThrottlePower = veh.ThrottlePower,
                BrakePower = veh.BrakePower,
            }.Pack(outgoingMessage);
            Client.SendMessage(outgoingMessage, messageType, connectionChannel);
        }
        public void SendVehicleState(SyncedVehicle v)
        {
            Vehicle veh = v.MainVehicle;

            NetOutgoingMessage outgoingMessage = Client.CreateMessage();

            NetDeliveryMethod messageType = NetDeliveryMethod.UnreliableSequenced;
            int connectionChannel = (byte)ConnectionChannel.VehicleSync;
            byte primaryColor = 0;
            byte secondaryColor = 0;
            unsafe
            {
                Function.Call<byte>(Hash.GET_VEHICLE_COLOURS, veh, &primaryColor, &secondaryColor);
            }
            new Packets.VehicleStateSync()
            {
                ID =v.ID,
                OwnerID = v.OwnerID,
                Flag = veh.GetVehicleFlags(),
                Colors=new byte[] { primaryColor, secondaryColor },
                DamageModel=veh.GetVehicleDamageModel(),
                LandingGear = veh.IsAircraft ? (byte)veh.LandingGearState : (byte)0,
                Mods = veh.Mods.GetVehicleMods(),
                ModelHash=veh.Model.Hash,
                EngineHealth=veh.EngineHealth,
                Passengers=veh.GetPassengers(),
                LockStatus=veh.LockStatus,
            }.Pack(outgoingMessage);
            Client.SendMessage(outgoingMessage, messageType, connectionChannel);
        }
        #region SYNC EVENTS
        public void Send(Packet p,ConnectionChannel channel=ConnectionChannel.Default)
        {
            NetOutgoingMessage outgoingMessage = Client.CreateMessage();
            NetDeliveryMethod messageType = NetDeliveryMethod.UnreliableSequenced;
            p.Pack(outgoingMessage);
            Client.SendMessage(outgoingMessage, messageType, (int)channel);
        }
        public void SendBulletShot(Vector3 start,Vector3 end,uint weapon,int ownerID)
        {
            NetOutgoingMessage outgoingMessage = Client.CreateMessage();

            NetDeliveryMethod messageType = NetDeliveryMethod.UnreliableSequenced;
            int connectionChannel = (byte)ConnectionChannel.SyncEvents;
            new Packets.BulletShot()
            {
                StartPosition = start.ToLVector(),
                EndPosition = end.ToLVector(),
                OwnerID = ownerID,
                WeaponHash=weapon,
            }.Pack(outgoingMessage);
            Client.SendMessage(outgoingMessage, messageType, connectionChannel);
        }
        public void SendOwnerChanged(int id,int newOwnerId)
        {
            NetOutgoingMessage outgoingMessage = Client.CreateMessage();

            NetDeliveryMethod messageType = NetDeliveryMethod.UnreliableSequenced;
            int connectionChannel = (byte)ConnectionChannel.SyncEvents;
            new Packets.OwnerChanged()
            {
                ID= id,
                NewOwnerID= newOwnerId,
            }.Pack(outgoingMessage);
            Client.SendMessage(outgoingMessage, messageType, connectionChannel);
        }
        public void SendEnteringVehicle(int pedId,int vehId,short seat)
        {
            NetOutgoingMessage outgoingMessage = Client.CreateMessage();

            NetDeliveryMethod messageType = NetDeliveryMethod.UnreliableSequenced;
            int connectionChannel = (byte)ConnectionChannel.SyncEvents;
            new Packets.EnteringVehicle()
            {
                PedID=pedId,
                VehicleID= vehId,
                VehicleSeat=seat,
            }.Pack(outgoingMessage);
            Client.SendMessage(outgoingMessage, messageType, connectionChannel);
        }
        public void SendLeaveVehicle(int id)
        {
            NetOutgoingMessage outgoingMessage = Client.CreateMessage();

            NetDeliveryMethod messageType = NetDeliveryMethod.UnreliableSequenced;
            int connectionChannel = (byte)ConnectionChannel.SyncEvents;
            new Packets.LeaveVehicle()
            {
                ID=id
            }.Pack(outgoingMessage);
            Client.SendMessage(outgoingMessage, messageType, connectionChannel);
        }
        #endregion
        public void SendChatMessage(string message)
        {
            NetOutgoingMessage outgoingMessage = Client.CreateMessage();

            new Packets.ChatMessage() { Username = Main.Settings.Username, Message = message }.Pack(outgoingMessage);

            Client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, (byte)ConnectionChannel.Chat);
            Client.FlushSendQueue();

#if DEBUG
            if (ShowNetworkInfo)
            {
                BytesSend += outgoingMessage.LengthBytes;
            }
#endif
        }

        public void SendModData(long target, string modName, byte customID, byte[] bytes)
        {
            NetOutgoingMessage outgoingMessage = Client.CreateMessage();
            new Packets.Mod()
            {
                // NetHandle =  Main.LocalNetHandle,
                Target = target,
                Name = modName,
                CustomPacketID =  customID,
                Bytes = bytes
            }.Pack(outgoingMessage);
            Client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, (byte)ConnectionChannel.Mod);
            Client.FlushSendQueue();

#if DEBUG
            if (ShowNetworkInfo)
            {
                BytesSend += outgoingMessage.LengthBytes;
            }
#endif
        }

        public void SendDownloadFinish(byte id)
        {
            NetOutgoingMessage outgoingMessage = Client.CreateMessage();

            new Packets.FileTransferComplete() { ID = id }.Pack(outgoingMessage);

            Client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableUnordered, (byte)ConnectionChannel.File);
            Client.FlushSendQueue();

#if DEBUG
            if (ShowNetworkInfo)
            {
                BytesSend += outgoingMessage.LengthBytes;
            }
#endif
        }

        public void SendTriggerEvent(string eventName, params object[] args)
        {
            NetOutgoingMessage outgoingMessage = Client.CreateMessage();

            new Packets.ServerClientEvent()
            {
                EventName = eventName,
                Args = new List<object>(args)
            }.Pack(outgoingMessage);

            Client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableUnordered, (byte)ConnectionChannel.Event);
            Client.FlushSendQueue();
        }
        #endregion
    }
}
