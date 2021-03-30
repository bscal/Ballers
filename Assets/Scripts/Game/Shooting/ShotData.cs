using MLAPI;
using MLAPI.NetworkVariable;
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
public class ShotData : INetworkSerializable
{
    public Vector3 position;
    public ShotType type;
    public ShotStyle style;
    public ShotDirection direction;
    public BankType bankshot;
    public ulong shooter;
    public int shotValue;
    public bool leftHanded;
    public float distance;
    public float contest;
    public float offSkill;
    public float defSkill;
    public float passRating;

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref position);
        serializer.Serialize(ref type);
        serializer.Serialize(ref style);
        serializer.Serialize(ref direction);
        serializer.Serialize(ref bankshot);
        serializer.Serialize(ref shooter);
        serializer.Serialize(ref shotValue);
        serializer.Serialize(ref leftHanded);
        serializer.Serialize(ref distance);
        serializer.Serialize(ref contest);
        serializer.Serialize(ref offSkill);
        serializer.Serialize(ref defSkill);
        serializer.Serialize(ref passRating);
    }

    public void Read(Stream stream)
    {
        using (PooledNetworkReader reader = PooledNetworkReader.Get(stream))
        {
            position = reader.ReadVector3Packed();
            type = (ShotType)reader.ReadByte();
            style = (ShotStyle)reader.ReadByte();
            direction = (ShotDirection)reader.ReadByte();
            bankshot = (BankType)reader.ReadByte();
            shooter = reader.ReadUInt64Packed();
            shotValue = reader.ReadInt32Packed();
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
        using (PooledNetworkWriter writer = PooledNetworkWriter.Get(stream))
        {
            writer.WriteVector3Packed(position);
            writer.WriteByte((byte)type);
            writer.WriteByte((byte)style);
            writer.WriteByte((byte)direction);
            writer.WriteByte((byte)bankshot);
            writer.WriteUInt64Packed(shooter);
            writer.WriteInt32Packed(shotValue);
            writer.WriteBool(leftHanded);
            writer.WriteDoublePacked(distance);
            writer.WriteDoublePacked(contest);
            writer.WriteDoublePacked(offSkill);
            writer.WriteDoublePacked(defSkill);
            writer.WriteDoublePacked(passRating);
        }
        Debug.LogWarning("ShotData size: " + stream.Length);
    }
}

/**

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
    public readonly NetworkVariableSettings Settings = new NetworkVariableSettings();
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
    private NetworkBehaviour networkedBehaviour;

    public NetworkedShotData() { }
    /// <summary>
    /// Creates a NetworkVariable with the default value and custom settings
    /// </summary>
    /// <param name="settings">The settings to use for the NetworkVariable</param>
    public NetworkedShotData(NetworkVariableSettings settings)
    {
        this.Settings = settings;
    }

    /// <summary>
    /// Creates a NetworkVariable with a custom value and custom settings
    /// </summary>
    /// <param name="settings">The settings to use for the NetworkVariable</param>
    /// <param name="value">The initial value to use for the NetworkVariable</param>
    public NetworkedShotData(NetworkVariableSettings settings, ShotData value)
    {
        this.Settings = settings;
        this.InternalValue = value;
    }

    [SerializeField]
    private ShotData InternalValue = default(ShotData);
    /// <summary>
    /// The value of the NetworkVariable container
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
            case NetworkVariablePermission.Everyone:
                return true;
            case NetworkVariablePermission.ServerOnly:
                return false;
            case NetworkVariablePermission.OwnerOnly:
                return networkedBehaviour.OwnerClientId == clientId;
            case NetworkVariablePermission.Custom:
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
        return Settings.SendNetworkChannel;
    }

    /// <inheritdoc />
    public bool IsDirty()
    {
        if (!isDirty) return false;
        if (Settings.SendTickrate == 0) return true;
        if (Settings.SendTickrate < 0) return false;
        if (NetworkManager.Singleton.NetworkTime - LastSyncedTime >= (1f / Settings.SendTickrate)) return true;
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
        LastSyncedTime = NetworkManager.Singleton.NetworkTime;
    }

    public void SetNetworkedBehaviour(NetworkBehaviour behaviour)
    {
        networkedBehaviour = behaviour;
    }

    /// <summary>
    /// Writes the variable to the writer
    /// </summary>
    /// <param name="stream">The stream to write the value to</param>
    public void WriteDelta(Stream stream) => WriteField(stream); // Based on default NetworkVariable implementation. This class doesnt need this

    public void WriteField(Stream stream)
    {
        InternalValue.Write(stream);
    }
}
**/

