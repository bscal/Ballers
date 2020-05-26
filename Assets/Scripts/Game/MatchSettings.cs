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

    public void Read(Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            MatchGlobals.GameMode = (BallersGamemode)reader.ReadByte();
            MatchGlobals.TeamSize = reader.ReadByte();
            MatchGlobals.QuarterLength = reader.ReadByte();
        }
    }

    public void Write(Stream stream)
    {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream))
        {
            writer.WriteByte((byte)MatchGlobals.GameMode);
            writer.WriteByte((byte)MatchGlobals.TeamSize);
            writer.WriteByte((byte)MatchGlobals.QuarterLength);
        }
    }
}
