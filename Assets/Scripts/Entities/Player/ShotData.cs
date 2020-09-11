using MLAPI;
using MLAPI.NetworkedVar;
using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Data Container for ShotData. Used in NetworkedShotData
/// </summary>
[Serializable]
public class ShotData : IBitWritable
{
    public ShotType type;
    public ShotDirection direction;
    public BankType bankshot;
    public Vector3 position;
    public ulong shooter;
    public bool leftHanded;
    public float distance;
    public float contest;
    public float offSkill;
    public float defSkill;
    public float passRating;

    public void Read(Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            position = reader.ReadVector3Packed();
            type = (ShotType)reader.ReadSByte();
            direction = (ShotDirection)reader.ReadSByte();
            bankshot = (BankType)reader.ReadSByte();
            shooter = reader.ReadUInt64Packed();
            leftHanded = reader.ReadBool();
            distance = reader.ReadSinglePacked();
            contest = reader.ReadSinglePacked();
            offSkill = reader.ReadSinglePacked();
            defSkill = reader.ReadSinglePacked();
            passRating = reader.ReadSinglePacked();
        }
    }

    public void Write(Stream stream)
    {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream))
        {
            writer.WriteVector3Packed(position);
            writer.WriteSByte((sbyte)type);
            writer.WriteSByte((sbyte)direction);
            writer.WriteSByte((sbyte)bankshot);
            writer.WriteUInt64Packed(shooter);
            writer.WriteBool(leftHanded);
            writer.WriteDoublePacked(distance);
            writer.WriteDoublePacked(contest);
            writer.WriteDoublePacked(offSkill);
            writer.WriteDoublePacked(defSkill);
            writer.WriteDoublePacked(passRating);
        }
    }
}

/// <summary>
/// ShotData contains data of the most recent shot. Shared by the server to clients.
/// </summary>
[Serializable]
public class NetworkedShotData : INetworkedVar
{
    /// <summary>
    /// Gets or sets Whether or not the variable needs to be delta synced
    /// </summary>
    public bool isDirty { get; set; }
    /// <summary>
    /// The settings for this var
    /// </summary>
    public readonly NetworkedVarSettings Settings = new NetworkedVarSettings();
    /// <summary>
    /// Gets the last time the variable was synced
    /// </summary>
    public float LastSyncedTime { get; internal set; }
    /// <summary>
    /// Delegate type for value changed event
    /// </summary>
    /// <param name="previousValue">The value before the change</param>
    /// <param name="newValue">The new value</param>
    public delegate void OnValueChangedDelegate(ShotData previousValue, ShotData newValue);
    /// <summary>
    /// The callback to be invoked when the value gets changed
    /// </summary>
    public OnValueChangedDelegate OnValueChanged;
    private NetworkedBehaviour networkedBehaviour;

    public NetworkedShotData() { }
    /// <summary>
    /// Creates a NetworkedVar with the default value and custom settings
    /// </summary>
    /// <param name="settings">The settings to use for the NetworkedVar</param>
    public NetworkedShotData(NetworkedVarSettings settings)
    {
        this.Settings = settings;
    }

    /// <summary>
    /// Creates a NetworkedVar with a custom value and custom settings
    /// </summary>
    /// <param name="settings">The settings to use for the NetworkedVar</param>
    /// <param name="value">The initial value to use for the NetworkedVar</param>
    public NetworkedShotData(NetworkedVarSettings settings, ShotData value)
    {
        this.Settings = settings;
        this.InternalValue = value;
    }

    [SerializeField]
    private ShotData InternalValue = default(ShotData);
    /// <summary>
    /// The value of the NetworkedVar container
    /// </summary>
    public ShotData Value
    {
        get
        {
            return InternalValue;
        }
        set
        {
            if (!EqualityComparer<ShotData>.Default.Equals(InternalValue, value))
            {
                isDirty = true;
                ShotData previousValue = InternalValue;
                InternalValue = value;
                OnValueChanged?.Invoke(previousValue, InternalValue);
            }
        }
    }

    /// <inheritdoc />
    public bool CanClientRead(ulong clientId)
    {
        switch (Settings.ReadPermission)
        {
            case NetworkedVarPermission.Everyone:
                return true;
            case NetworkedVarPermission.ServerOnly:
                return false;
            case NetworkedVarPermission.OwnerOnly:
                return networkedBehaviour.OwnerClientId == clientId;
            case NetworkedVarPermission.Custom:
                {
                    if (Settings.ReadPermissionCallback == null) return false;
                    return Settings.ReadPermissionCallback(clientId);
                }
        }
        return true;
    }

    /// <summary>
    /// Clients cannot write to ShotData.
    /// </summary>
    public bool CanClientWrite(ulong clientId)
    {
        return false;
    }

    /// <inheritdoc />
    public string GetChannel()
    {
        return Settings.SendChannel;
    }

    /// <inheritdoc />
    public bool IsDirty()
    {
        if (!isDirty) return false;
        if (Settings.SendTickrate == 0) return true;
        if (Settings.SendTickrate < 0) return false;
        if (NetworkingManager.Singleton.NetworkTime - LastSyncedTime >= (1f / Settings.SendTickrate)) return true;
        return false;
    }

    /// <summary>
    /// Reads value from the reader and applies it
    /// </summary>
    /// <param name="stream">The stream to read the value from</param>
    /// <param name="keepDirtyDelta">Whether or not the container should keep the dirty delta, or mark the delta as consumed</param>
    public void ReadDelta(Stream stream, bool keepDirtyDelta)
    {
        ShotData previousValue = InternalValue;

        InternalValue.Read(stream);

        if (keepDirtyDelta) isDirty = true;

        OnValueChanged?.Invoke(previousValue, InternalValue);
    }

    public void ReadField(Stream stream)
    {
        ReadDelta(stream, false);
    }

    /// <inheritdoc />
    public void ResetDirty()
    {
        isDirty = false;
        LastSyncedTime = NetworkingManager.Singleton.NetworkTime;
    }

    public void SetNetworkedBehaviour(NetworkedBehaviour behaviour)
    {
        networkedBehaviour = behaviour;
    }

    /// <summary>
    /// Writes the variable to the writer
    /// </summary>
    /// <param name="stream">The stream to write the value to</param>
    public void WriteDelta(Stream stream) => WriteField(stream); // Based on default NetworkedVar implementation. This class doesnt need this

    public void WriteField(Stream stream)
    {
        InternalValue.Write(stream);
    }
}
