using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;
using System;
using System.IO;
using UnityEngine;

[Serializable]
public class ShotBarData : IBitWritable
{
    public const int GRADE_PERFECT  = 0;
    public const int GRADE_GOOD     = 1;
    public const int GRADE_OK       = 2;
    public const int GRADE_POOR     = 3;

    // ShotMeter fields
    public float speed;
    public float startOffset;
    public float endOffset;
    public float targetFadeSpd;
    
    // % of bar per shot grade
    public float bad;
    public float ok;
    public float good;
    public float perfect;

    // target values
    public float targetSize;
    public float targetHeight;

    public float BadLength => ShotMeter.MAX_TARGET_HEIGHT * bad;
    public float OkLength => ShotMeter.MAX_TARGET_HEIGHT * ok;
    public float GoodLength => ShotMeter.MAX_TARGET_HEIGHT * good;
    public float PerfectLength => ShotMeter.MAX_TARGET_HEIGHT * perfect;
    public float BonusHeight => good + perfect;
    public float FinalTargetHeight => targetHeight + endOffset - startOffset;

    public int GetShotGrade(float distance)
    {
        if (distance < PerfectLength)
            return 0;
        if (distance < GoodLength)
            return 1;
        if (distance < OkLength)
            return 2;
        return 3;
    }

    public void Read(Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            speed           = reader.ReadSinglePacked();
            startOffset     = reader.ReadSinglePacked();
            endOffset       = reader.ReadSinglePacked();
            targetFadeSpd   = reader.ReadSinglePacked();
            //bad             = reader.ReadSinglePacked();
            ok              = reader.ReadSinglePacked();
            good            = reader.ReadSinglePacked();
            perfect         = reader.ReadSinglePacked();
            targetSize      = reader.ReadSinglePacked();
            targetHeight    = reader.ReadSinglePacked();

            bad = Mathf.Clamp(ok + good + perfect - 1f, 0f, 1f);
        }
    }
    public void Write(Stream stream)
    {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream))
        {
            writer.WriteSinglePacked(speed);
            writer.WriteSinglePacked(startOffset);
            writer.WriteSinglePacked(endOffset);
            writer.WriteSinglePacked(targetFadeSpd);
            //writer.WriteSinglePacked(bad);
            writer.WriteSinglePacked(ok);
            writer.WriteSinglePacked(good);
            writer.WriteSinglePacked(perfect);
            writer.WriteSinglePacked(targetSize);
            writer.WriteSinglePacked(targetHeight);
        }
    }
}
