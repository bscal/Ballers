using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum BallersGamemode
{
    SP_BOTS,
    COOP_BOTS,
    MP_PVP
}

public class MatchSettings : IBitWritable
{

    public BallersGamemode gamemode;
    public int teamSize;
    public int quarterLength;

    public void Read(Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            gamemode = (BallersGamemode)reader.ReadByte();
            teamSize = reader.ReadByte();
            quarterLength = reader.ReadByte();
        }
    }

    public void Write(Stream stream)
    {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream))
        {
            writer.WriteByte((byte)gamemode);
            writer.WriteByte((byte)teamSize);
            writer.WriteByte((byte)quarterLength);
        }
    }
}
