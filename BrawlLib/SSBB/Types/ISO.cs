﻿using System;
using System.Runtime.InteropServices;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ISO
    {
        public const uint WiiTag = 0xA39E1C5D;
        public const uint GCTag = 0x3D9F33C2;

        public byte _console;
        public byte _title0;
        public byte _title1;
        public byte _region;
        public bushort _publisher;
        public bushort _unk1;
        public fixed byte _pad1[0x10];
        public uint _tagWii;
        public uint _tagGC;
        public fixed sbyte _name[0x60];

        public string GameName
        {
            get { return new string((sbyte*)Address + 0x20); }
            set { value.Write((sbyte*)Address + 0x20); }
        }
        public string GameID
        {
            get { return *(BinTag*)Address; }
            set { *(BinTag*)Address = value; }
        }
        public bool IsWii
        {
            get
            {
                return 
                    _console == 'R' || 
                    _console == '_' ||
                    _console == 'H' ||
                    _console == '0' ||
                    _console == '4';
            }
        }
        public bool IsGC
        {
            get
            {
                return
                  _console == 'G' ||
                  _console == 'D' ||
                  _console == 'P' ||
                  _console == 'U';
            }
        }

        public PartitionHeader* Partitions
        {
            get { return (PartitionHeader*)(Address + 0x40000); }
        }
        private VoidPtr Address { get { fixed (void* ptr = &this) return ptr; } }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PartitionHeader
    {
        public bint _partitionCount;
        public buint _partitionOffset;
        public bint _channelCount;
        public buint _channelOffset;

        public uint PartitionOffset
        {
            get { return _partitionOffset * 4; }
            set { _partitionOffset = value.Align(4) / 4; }
        }
        public uint ChannelOffset
        {
            get { return _channelOffset * 4; }
            set { _channelOffset = value.Align(4) / 4; }
        }

        private VoidPtr Address { get { fixed (void* ptr = &this) return ptr; } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PartitionTableEntry
    {
        public buint _offset;
        public bint _type;

        public enum Type
        {
            Data = 0,
            Update = 1,
            Installer = 2,
            VirtualConsole = 3,
        }

        public Type PartitionType { get { return (Type)(int)_type; } set { _type = (int)value; } }
        public string GameID
        {
            get { return *(BinTag*)_type.Address; }
            set { *(BinTag*)_type.Address = value; }
        }

        private VoidPtr Address { get { fixed (void* ptr = &this) return ptr; } }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PartitionInfo
    {
        public buint _tmdSize;
        public buint _tmdOffset;
        public buint _certSize;
        public buint _certOffset;

        public buint _h3Offset;
        public buint _dataOffset;
        public buint _dataLength;
        
        private VoidPtr Address { get { fixed (void* ptr = &this) return ptr; } }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct TMDInfo
    {
        public const int Size = 0xA4;

        public fixed byte _issuer[0x40];
        public byte _version;
        public byte _caCrlVersion;
        public byte _signerCrlVersion;
        public byte _pad0;
        public bint _sysVersionLo;
        public bint _sysVersionHi;
        public bshort _titleID0;
        public bshort _titleID1;
        public BinTag _titleTag;
        public bint _titleType;
        public bshort _groupID;
        public fixed byte _pad1[62];
        public bint _accessRights;
        public bshort _titleVersion;
        public bshort _numContents;
        public bshort _bootIndex;
        public bshort _pad2;

        private VoidPtr Address { get { fixed (void* ptr = &this) return ptr; } }
    }
    public unsafe struct TMDEntry
    {
        public const int Size = 0x30;

        public buint _cid;
        public bushort _index;
        public bushort _type;
        public blong _size;
        public fixed byte _hash[0x14];
        public fixed byte _pad[0xC];

        private VoidPtr Address { get { fixed (void* ptr = &this) return ptr; } }
    }
}