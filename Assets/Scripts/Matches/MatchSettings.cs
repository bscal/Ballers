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

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref GameMode);
        serializer.Serialize(ref TeamSize);
        serializer.Serialize(ref QuartersCount);
        serializer.Serialize(ref QuarterLength);
        serializer.Serialize(ref ShotClockLength);
        serializer.Serialize(ref ShotClockLength);
    }
}
