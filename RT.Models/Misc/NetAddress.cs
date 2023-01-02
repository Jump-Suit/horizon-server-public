using RT.Common;
using Server.Common;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace RT.Models
{
    public class NetAddress : IStreamSerializer
    {
        public static readonly NetAddress Empty = new NetAddress() { AddressType = NetAddressType.NetAddressNone };

        public NetAddressType AddressType;
        public string Address;
        public uint BinaryAddress;
        public int Port;


        public void Deserialize(BinaryReader reader)
        {
            AddressType = reader.Read<NetAddressType>();

            
            if(AddressType == NetAddressType.NetAddressTypeBinaryExternal)
            {
                BinaryAddress = reader.ReadUInt32();
                Port = reader.ReadInt32();
            }

            if(AddressType == NetAddressType.NetAddressTypeInternal
                || AddressType == NetAddressType.NetAddressTypeExternal
                || AddressType == NetAddressType.NetAddressTypeNATService
                || AddressType == NetAddressType.NetAddressTypeSignalAddress
                || AddressType == NetAddressType.NetAddressNone)
            {
                Address = reader.ReadString(Constants.NET_MAX_NETADDRESS_LENGTH);
                Port = reader.ReadInt32();
            }
            
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(AddressType);
            if (AddressType == NetAddressType.NetAddressTypeBinaryExternal)  {
                writer.Write(BinaryAddress);
                writer.Write(Port);
            }

            if(AddressType == NetAddressType.NetAddressTypeInternal 
                || AddressType == NetAddressType.NetAddressTypeExternal 
                || AddressType == NetAddressType.NetAddressTypeNATService 
                || AddressType == NetAddressType.NetAddressTypeSignalAddress
                || AddressType == NetAddressType.NetAddressNone)
            {
                writer.Write(Address, Constants.NET_MAX_NETADDRESS_LENGTH);
                writer.Write(Port);
            }
        }

        public override string ToString()
        {
            if (AddressType == NetAddressType.NetAddressTypeBinaryExternal) {
                return base.ToString() + " " +
                $"AddressType: {AddressType} " +
                $"BinaryAddress: {BinaryAddress} " +
                $"Port: {Port}";
            } else {
                return base.ToString() + " " +
                $"AddressType: {AddressType} " +
                $"Address: {string.Join(" ", Address)} " +
                $"Port: {Port}";
            }

        }
    }
}