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

    public BallersGamemode GameMode { get; set; }
    public int TeamSize { get; set; }
    public float QuarterLength { get; set; }
    public int QuartersCount { get; set; }

    public MatchSettings() { }
    public MatchSettings(BallersGamemode gamemode, int teamSize, float quarterL, int quarterC)
    {
        this.GameMode = gamemode;
        this.TeamSize = teamSize;
        this.QuarterLength = quarterL;
        this.QuartersCount = quarterC;
    }

    public void Read(Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            GameMode = (BallersGamemode)reader.ReadByte();
            TeamSize = reader.ReadByte();
            QuarterLength = reader.ReadByte();
            QuartersCount = reader.ReadByte();
        }
    }

    public void Write(Stream stream)
    {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream))
        {
            writer.WriteByte((byte)GameMode);
            writer.WriteByte((byte)TeamSize);
            writer.WriteByte((byte)QuarterLength);
            writer.WriteByte((byte)QuartersCount);
        }
    }
}
