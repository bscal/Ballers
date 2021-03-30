using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;
using System;
using System.IO;
using UnityEngine;

public enum SpeedVariations
{
    NONE = 0,
    LINEAR = 1, // Linear increase of speed
    CURVED = 2, // Slow -> fast -> slow
    WAVE = 3,   // Linear increase with wave variation
}

[Serializable]
public class ShotBarData : INetworkSerializable
{
    public const int GRADE_PERFECT  = 0;
    public const int GRADE_GOOD     = 1;
    public const int GRADE_OK       = 2;
    public const int GRADE_POOR     = 3;

    // ShotMeter fields
    public float speed;
    public float targetOffset;
    public float targetFadeSpd;
    public float barShake;
    public int spdVariationID;
    
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
    public float FinalTargetHeight => targetHeight;

    public void SetBarValues(float perfect, float good, float ok)
    {
        this.perfect = Mathf.Clamp01(perfect);
        this.good = Mathf.Clamp01(good);
        this.ok = Mathf.Clamp01(ok);
        this.bad = Mathf.Clamp01(1f - ok);
    }

    public void SetBarValuesMultipleOf(float perfect)
    {
        SetBarValues(perfect, perfect * 3f, perfect * 6f);
    }

    public int GetShotGrade(float distance)
    {
        if (distance < PerfectLength / 2)
            return 0;
        else if (distance < GoodLength / 2)
            return 1;
        else if (distance < OkLength / 2)
            return 2;
        return 3;
    }

    public void Read(Stream stream)
    {
        using (PooledNetworkReader reader = PooledNetworkReader.Get(stream))
        {
            speed           = reader.ReadSinglePacked();
            targetOffset    = reader.ReadSinglePacked();
            targetFadeSpd   = reader.ReadSinglePacked();
            ok              = reader.ReadSinglePacked();
            good            = reader.ReadSinglePacked();
            perfect         = reader.ReadSinglePacked();
            targetSize      = reader.ReadSinglePacked();
            targetHeight    = reader.ReadSinglePacked();
            barShake        = reader.ReadSinglePacked();
            spdVariationID  = reader.ReadByte();
        }
        bad = Mathf.Clamp(1f - ok, 0f, 1f);
    }
    public void Write(Stream stream)
    {
        using (PooledNetworkWriter writer = PooledNetworkWriter.Get(stream))
        {
            writer.WriteSinglePacked(speed);
            writer.WriteSinglePacked(targetOffset);
            writer.WriteSinglePacked(targetFadeSpd);
            writer.WriteSinglePacked(ok);
            writer.WriteSinglePacked(good);
            writer.WriteSinglePacked(perfect);
            writer.WriteSinglePacked(targetSize);
            writer.WriteSinglePacked(targetHeight);
            writer.WriteSinglePacked(barShake);
            writer.WriteByte((byte)spdVariationID);
        }
    }

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref speed);
        serializer.Serialize(ref targetOffset);
        serializer.Serialize(ref targetFadeSpd);
        serializer.Serialize(ref ok);
        serializer.Serialize(ref good);
        serializer.Serialize(ref perfect);
        serializer.Serialize(ref targetSize);
        serializer.Serialize(ref targetHeight);
        serializer.Serialize(ref barShake);
        serializer.Serialize(ref spdVariationID);
    }
}
