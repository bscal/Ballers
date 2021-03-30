using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Serialization.Pooled;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public struct InternalID
{
    public readonly ulong clientID;
    public readonly ulong networkID;

    public InternalID(ulong clientID, ulong NetworkID)
    {
        this.clientID = clientID;
        this.networkID = NetworkID;
    }
}
/**
[Serializable]
public class NetworkVarInternalID : INetworkedVar
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
    public delegate void OnValueChangedDelegate(InternalID previousValue, InternalID newValue);
    /// <summary>
    /// The callback to be invoked when the value gets changed
    /// </summary>
    public OnValueChangedDelegate OnValueChanged;
    private NetworkBehaviour networkedBehaviour;

    /// <summary>
    /// Creates a NetworkVariable with the default value and settings
    /// </summary>
    public NetworkVarInternalID()
    {

    }

    /// <summary>
    /// Creates a NetworkVariable with the default value and custom settings
    /// </summary>
    /// <param name="settings">The settings to use for the NetworkVariable</param>
    public NetworkVarInternalID(NetworkVariableSettings settings)
    {
        this.Settings = settings;
    }

    /// <summary>
    /// Creates a NetworkVariable with a custom value and custom settings
    /// </summary>
    /// <param name="settings">The settings to use for the NetworkVariable</param>
    /// <param name="value">The initial value to use for the NetworkVariable</param>
    public NetworkVarInternalID(NetworkVariableSettings settings, InternalID value)
    {
        this.Settings = settings;
        this.InternalValue = value;
    }

    /// <summary>
    /// Creates a NetworkVariable with a custom value and the default settings
    /// </summary>
    /// <param name="value">The initial value to use for the NetworkVariable</param>
    public NetworkVarInternalID(InternalID value)
    {
        this.InternalValue = value;
    }

    [SerializeField]
    private InternalID InternalValue = default;
    /// <summary>
    /// The value of the NetworkVariable container
    /// </summary>
    public InternalID Value
    {
        get
        {
            return InternalValue;
        }
        set
        {
            if (!EqualityComparer<InternalID>.Default.Equals(InternalValue, value))
            {
                isDirty = true;
                InternalID previousValue = InternalValue;
                InternalValue = value;
                if (OnValueChanged != null)
                    OnValueChanged(previousValue, InternalValue);
            }
        }
    }

    /// <inheritdoc />
    public void ResetDirty()
    {
        isDirty = false;
        LastSyncedTime = NetworkManager.Singleton.NetworkTime;
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
    /// Writes the variable to the writer
    /// </summary>
    /// <param name="stream">The stream to write the value to</param>
    public void WriteDelta(Stream stream) => WriteField(stream); //The NetworkVariable is built for simple data types and has no delta.

    /// <inheritdoc />
    public bool CanClientWrite(ulong clientId)
    {
        switch (Settings.WritePermission)
        {
            case NetworkVariablePermission.Everyone:
                return true;
            case NetworkVariablePermission.ServerOnly:
                return false;
            case NetworkVariablePermission.OwnerOnly:
                return networkedBehaviour.OwnerClientId == clientId;
            case NetworkVariablePermission.Custom:
                {
                    if (Settings.WritePermissionCallback == null) return false;
                    return Settings.WritePermissionCallback(clientId);
                }
        }

        return true;
    }

    /// <summary>
    /// Reads value from the reader and applies it
    /// </summary>
    /// <param name="stream">The stream to read the value from</param>
    /// <param name="keepDirtyDelta">Whether or not the container should keep the dirty delta, or mark the delta as consumed</param>
    public void ReadDelta(Stream stream, bool keepDirtyDelta)
    {
        using (PooledNetworkReader reader = PooledNetworkReader.Get(stream))
        {
            InternalID previousValue = InternalValue;
            InternalValue = new InternalID(reader.ReadUInt64Packed(), reader.ReadUInt64Packed());

            if (keepDirtyDelta) isDirty = true;

            if (OnValueChanged != null)
                OnValueChanged(previousValue, InternalValue);
        }
    }

    /// <inheritdoc />
    public void SetNetworkedBehaviour(NetworkBehaviour behaviour)
    {
        networkedBehaviour = behaviour;
    }

    /// <inheritdoc />
    public void ReadField(Stream stream)
    {
        ReadDelta(stream, false);
    }

    /// <inheritdoc />
    public void WriteField(Stream stream)
    {
        using (PooledNetworkWriter writer = PooledNetworkWriter.Get(stream))
        {
            writer.WriteUInt64Packed(InternalValue.clientID); //BOX
            writer.WriteUInt64Packed(InternalValue.networkID);
        }
    }

    /// <inheritdoc />
    public string GetChannel()
    {
        return Settings.SendNetworkChannel;
    }
}
*/
