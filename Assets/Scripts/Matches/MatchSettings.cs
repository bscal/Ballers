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

public class MatchSettings : INetworkSerializable
{

    public BallersGamemode GameMode;
    public int TeamSize;
    public int QuartersCount;
    public double QuarterLength;
    public double ShotClockLength;
    public int NumOfAIs;
    public AIDifficulty Difficulty;


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
        using (PooledNetworkReader reader = PooledNetworkReader.Get(stream))
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
        using (PooledNetworkWriter writer = PooledNetworkWriter.Get(stream))
        {
            writer.WriteByte((byte)GameMode);
            writer.WriteByte((byte)TeamSize);
            writer.WriteByte((byte)QuartersCount);
            writer.WriteDoublePacked(QuarterLength);
            writer.WriteDoublePacked(ShotClockLength);
        }
    }

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref GameMode);
        serializer.Serialize(ref TeamSize);
        serializer.Serialize(ref QuartersCount);
        serializer.Serialize(ref QuarterLength);
        serializer.Serialize(ref ShotClockLength);
    }
}
