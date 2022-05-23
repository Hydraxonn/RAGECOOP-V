﻿using System;
using System.Collections.Generic;
using System.Text;

using Lidgren.Network;

namespace RageCoop.Core
{
    public partial class Packets
    {

        /// <summary>
        /// For non-critical properties, synced every 20 frames.
        /// </summary>
        public class PedStateSync : Packet
        {
            public int ID { get; set; }

            public int ModelHash { get; set; }

            public Dictionary<byte, short> Clothes { get; set; }

            public int OwnerID { get; set; }

            public Dictionary<uint, bool> WeaponComponents { get; set; }
            



            public override void Pack(NetOutgoingMessage message)
            {
                #region PacketToNetOutGoingMessage
                message.Write((byte)PacketTypes.CharacterStateSync);

                List<byte> byteArray = new List<byte>();

                // Write ID
                byteArray.AddInt(ID);

                // Write model hash
                byteArray.AddInt(ModelHash);

                // Write player clothes
                // Write the count of clothes
                byteArray.AddRange(BitConverter.GetBytes((ushort)Clothes.Count));
                // Loop the dictionary and add the values
                foreach (KeyValuePair<byte, short> cloth in Clothes)
                {
                    byteArray.Add(cloth.Key);
                    byteArray.AddRange(BitConverter.GetBytes(cloth.Value));
                }

                //Write OwnerID for this ped
                byteArray.AddRange(BitConverter.GetBytes(OwnerID));

                // Write player weapon components
                if (WeaponComponents != null)
                {
                    byteArray.Add(0x01);
                    byteArray.AddRange(BitConverter.GetBytes((ushort)WeaponComponents.Count));
                    foreach (KeyValuePair<uint, bool> component in WeaponComponents)
                    {
                        byteArray.AddRange(BitConverter.GetBytes(component.Key));
                        byteArray.AddRange(BitConverter.GetBytes(component.Value));
                    }
                }
                else
                {
                    // Player weapon doesn't have any components
                    byteArray.Add(0x00);
                }


                byte[] result = byteArray.ToArray();
                message.Write(result.Length);
                message.Write(result);
                #endregion
            }

            public override void Unpack(byte[] array)
            {
                #region NetIncomingMessageToPacket
                BitReader reader = new BitReader(array);

                // Read player netHandle
                ID = reader.ReadInt();

                // Read player model hash
                ModelHash = reader.ReadInt();

                // Read player clothes
                // Create new Dictionary
                Clothes = new Dictionary<byte, short>();
                // Read the count of clothes
                ushort clothCount = reader.ReadUShort();
                // For clothCount
                for (ushort i = 0; i < clothCount; i++)
                {
                    // Read cloth value
                    Clothes.Add(reader.ReadByte(), reader.ReadShort());
                }

                // Read ped OwnerID
                OwnerID= reader.ReadInt();

                // Read player weapon components
                if (reader.ReadBool())
                {
                    WeaponComponents = new Dictionary<uint, bool>();
                    ushort comCount = reader.ReadUShort();
                    for (ushort i = 0; i < comCount; i++)
                    {
                        WeaponComponents.Add(reader.ReadUInt(), reader.ReadBool());
                    }
                }

                #endregion
            }
        }


        public class PedSync : Packet
        {
            public int ID { get; set; }
            public PedDataFlags Flag { get; set; }

            public int Health { get; set; }

            public LVector3 Position { get; set; }

            public LVector3 Rotation { get; set; }

            public LVector3 Velocity { get; set; }

            public LVector3 RotationVelocity { get; set; }

            public byte Speed { get; set; }

            public LVector3 AimCoords { get; set; }

            public uint CurrentWeaponHash { get; set; }

            public float Heading { get; set; }

            public override void Pack(NetOutgoingMessage message)
            {
                #region PacketToNetOutGoingMessage
                message.Write((byte)PacketTypes.CharacterSync);
                
                List<byte> byteArray = new List<byte>();

                // Write ped ID
                byteArray.AddInt(ID);


                // Write ped flags
                byteArray.AddRange(BitConverter.GetBytes((ushort)Flag));

                // Write ped health
                byteArray.AddRange(BitConverter.GetBytes(Health));

                // Write ped position
                byteArray.AddLVector3(Position);

                // Write ped rotation
                byteArray.AddLVector3(Rotation);

                // Write ped velocity
                byteArray.AddLVector3(Velocity);

                if (Flag.HasFlag(PedDataFlags.IsRagdoll))
                {
                    byteArray.AddLVector3(RotationVelocity);
                }

                // Write ped speed
                byteArray.Add(Speed);

                // Write ped weapon hash
                byteArray.AddRange(BitConverter.GetBytes(CurrentWeaponHash));

                if (Flag.HasFlag(PedDataFlags.IsAiming))
                {
                    // Write ped aim coords
                    byteArray.AddLVector3(AimCoords);
                }

                byteArray.AddFloat(Heading);

                byte[] result = byteArray.ToArray();

                message.Write(result.Length);
                message.Write(result);
                #endregion
            }

            public override void Unpack(byte[] array)
            {
                #region NetIncomingMessageToPacket
                BitReader reader = new BitReader(array);

                // Read player netHandle
                ID = reader.ReadInt();

                // Read player flags
                Flag = (PedDataFlags)reader.ReadUShort();

                // Read player health
                Health = reader.ReadInt();

                // Read player position
                Position = reader.ReadLVector3();

                // Read player rotation
                Rotation = reader.ReadLVector3();

                // Read player velocity
                Velocity = reader.ReadLVector3();

                // Read rotation velocity if in ragdoll
                if (Flag.HasFlag(PedDataFlags.IsRagdoll))
                {
                    RotationVelocity=reader.ReadLVector3();
                }

                // Read player speed
                Speed = reader.ReadByte();

                // Read player weapon hash
                CurrentWeaponHash = reader.ReadUInt();

                // Try to read aim coords
                if (Flag.HasFlag(PedDataFlags.IsAiming))
                {
                    // Read player aim coords
                    AimCoords = reader.ReadLVector3();
                }

                Heading=reader.ReadFloat();
                #endregion
            }
        }




    }
}
