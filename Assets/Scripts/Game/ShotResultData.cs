using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum ShotResultType
{
    MADE,
    MISSED,
    BLOCKED
}

public class ShotResultData : IBitWritable
{

    public ShotResultType shotMissedType;
    public int grade;
    public float releaseDiff;
    public float shotDifficulty;

    public void Read(Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            shotMissedType = (ShotResultType)reader.ReadBits(2);
            grade = (int)reader.ReadBits(4);
            releaseDiff = reader.ReadSinglePacked();
            shotDifficulty = reader.ReadSinglePacked();
        }
    }

    public void Write(Stream stream)
    {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream))
        {
            writer.WriteBits((byte)shotMissedType, 2);
            writer.WriteBits((byte)grade, 4);
            writer.WriteSinglePacked(releaseDiff);
            writer.WriteSinglePacked(shotDifficulty);
        }
    }
}
