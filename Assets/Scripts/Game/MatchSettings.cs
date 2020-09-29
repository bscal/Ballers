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
    public int QuartersCount { get; set; }
    public double QuarterLength { get; set; }
    public double ShotClockLength { get; set; } 
    public int NumOfAIs { get; set; }
    public AIDifficulty Difficulty { get; set; }


    public MatchSettings() { }
    public MatchSettings(BallersGamemode gamemode, int teamSize, int quarterC, double quarterL, double shotClockL)
    {
        GameMode = gamemode;
        TeamSize = teamSize;
        QuartersCount = quarterC;
        QuarterLength = quarterL;
        ShotClockLength = shotClockL;
    }

    public void Read(Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            GameMode = (BallersGamemode)reader.ReadByte();
            TeamSize = reader.ReadByte();
            QuartersCount = reader.ReadByte();
            QuarterLength = reader.ReadDoublePacked();
            ShotClockLength = reader.ReadDoublePacked();
        }
    }

    public void Write(Stream stream)
    {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream))
        {
            writer.WriteByte((byte)GameMode);
            writer.WriteByte((byte)TeamSize);
            writer.WriteByte((byte)QuartersCount);
            writer.WriteDoublePacked(QuarterLength);
            writer.WriteDoublePacked(ShotClockLength);
        }
    }
}
