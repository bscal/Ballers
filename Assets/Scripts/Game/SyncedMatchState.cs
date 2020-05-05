using MLAPI;
using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Build.Content;
using UnityEngine;

public class SyncedMatchState : NetworkedBehaviour, IBitWritable
{

    public bool HasStarted;
    public int TeamWithPossession;
    public ulong PlayerWithBall;
    public Team[] Teams;

    void Start()
    {

    }

    void Update()
    {

    }

    void NetworkedStart()
    {

    }

    public void Read(Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            HasStarted = reader.ReadBit();
            reader.SkipPadBits();
            TeamWithPossession = reader.ReadInt32Packed();
            PlayerWithBall = reader.ReadUInt64Packed();
        }
    }

    public void Write(Stream stream)
    {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream))
        {
            writer.WriteBit(HasStarted);
            writer.WritePadBits();
            writer.WriteInt32Packed(TeamWithPossession);
            writer.WriteUInt64Packed(PlayerWithBall);
        }
    }
}