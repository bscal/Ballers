using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[Serializable]
public class Team : IBitWritable
{

    /// <summary>
    /// Id of team used to determine if home/away. Home = 0, Away = 1
    /// </summary>
    public readonly int id;
    /// <summary>
    /// An array of the players in their designated position
    /// </summary>
    public Dictionary<int, Player> teamSlots;
    /// <summary>
    /// Team points;
    /// </summary>
    public int points = 0;
    /// <summary>
    /// Team fouls
    /// </summary>
    public int fouls = 0;

    public Team(int t_id, int t_size)
    {
        id = t_id;
        teamSlots = new Dictionary<int, Player>(t_size);
    }

    public void Read(Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            points = reader.ReadInt32Packed();
            fouls = reader.ReadInt32Packed();
        }
    }

    public void Write(Stream stream)
    {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream))
        {
            writer.WriteInt32Packed(points);
            writer.WriteInt32Packed(fouls);
        }
    }

    public void WriteSyncTeamSlots()
    {
        using (PooledBitStream stream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(stream))
            {
                writer.WriteInt32Packed(id);
                foreach (var pair in teamSlots)
                {
                    writer.WriteInt32Packed(pair.Key);
                    writer.WriteUInt64Packed(pair.Value.NetworkId);
                }
                GameManager.Singleton.InvokeClientRpcOnEveryone(GameManager.Singleton.ClientSyncTeamSlots, stream);
            }
        }
    }

    public void ReadSyncTeamSlots(PooledBitReader reader)
    {
        int slot = reader.ReadInt32Packed();
        ulong pid = reader.ReadUInt64Packed();
        teamSlots[slot] = GameManager.GetPlayer(pid);
    }
}