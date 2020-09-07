using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class ShotBarData : IBitWritable
{

    public float speed;
    public float startOffset;
    public float endOffset;

    public float bad;
    public float ok;
    public float good;
    public float perfect;

    public float targetSize;
    public float targetHeight;

    public float BadLength => ShotMeter.MAX_TARGET_HEIGHT * bad;
    public float OkLength => ShotMeter.MAX_TARGET_HEIGHT * ok;
    public float GoodLength => ShotMeter.MAX_TARGET_HEIGHT * good;
    public float PerfectLength => ShotMeter.MAX_TARGET_HEIGHT * perfect;
    public float BonusHeight => good + perfect;
    public float FinalTargetHeight
    {
        get
        {
            return targetHeight + endOffset - startOffset;
        }
    }

    public void Read(Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            speed           = (float)reader.ReadDoublePacked();
            startOffset     = (float)reader.ReadDoublePacked();
            endOffset       = (float)reader.ReadDoublePacked();
            bad             = (float)reader.ReadDoublePacked();
            ok              = (float)reader.ReadDoublePacked();
            good            = (float)reader.ReadDoublePacked();
            perfect         = (float)reader.ReadDoublePacked();
            targetSize      = (float)reader.ReadDoublePacked();
            targetHeight    = (float)reader.ReadDoublePacked();
        }
    }
    public void Write(Stream stream)
    {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream))
        {
            writer.WriteDoublePacked(speed);
            writer.WriteDoublePacked(startOffset);
            writer.WriteDoublePacked(endOffset);
            writer.WriteDoublePacked(bad);
            writer.WriteDoublePacked(ok);
            writer.WriteDoublePacked(good);
            writer.WriteDoublePacked(perfect);
            writer.WriteDoublePacked(targetSize);
            writer.WriteDoublePacked(targetHeight);
        }
    }
}
