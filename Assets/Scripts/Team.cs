using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Assertions;

[Serializable]
public class TeamData : IBitWritable
{
    /// <summary>
    /// Team points;
    /// </summary>
    public int points = 0;
    /// <summary>
    /// Team fouls
    /// </summary>
    public int fouls = 0;

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
}

[Serializable]
public class Team
{

    public const int SLOT_NULL = -1;
    public const int SLOT_PG = 0;
    public const int SLOT_SG = 1;
    public const int SLOT_SF = 2;
    public const int SLOT_PF = 3;
    public const int SLOT_C = 4;

    /// <summary>
    /// Id of team used to determine if home/away. Home = 0, Away = 1
    /// </summary>
    public readonly int id;
    /// <summary>
    /// Max team size
    /// </summary>
    public readonly int maxTeamSize;
    /// <summary>
    /// An array of the players in their designated position
    /// </summary>
    public readonly Dictionary<int, Player> teamSlots;

    /// <summary>
    /// Represents team's values throughout the game
    /// </summary>
    public TeamData TeamData { get; set; }

    public Team(int t_id, int t_size)
    {
        id = t_id;
        maxTeamSize = t_size;
        teamSlots = new Dictionary<int, Player>(t_size);
        TeamData = new TeamData();
    }

    public void WriteSyncTeamSlots()
    {
        using (PooledBitStream stream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(stream))
            {
                writer.WriteInt32Packed(id);
                writer.WriteInt32Packed(teamSlots.Count);
                foreach (var pair in teamSlots)
                {
                    writer.WriteInt32Packed(pair.Key);
                    writer.WriteUInt64Packed(pair.Value.NetworkId);
                }
                GameManager.Singleton.InvokeClientRpcOnEveryone(GameManager.Singleton.ClientSyncTeamSlots, stream);
            }
        }
    }

    public void ReadSyncTeamSlots(PooledBitReader reader, int count)
    {
        for (int i = 0; i < count; i++)
        {
            int slot = reader.ReadInt32Packed();
            ulong netID = reader.ReadUInt64Packed();
            teamSlots[slot] = GameManager.GetPlayerByNetworkID(netID);
        }
    }

    public int GetOpenSlot()
    {
        for (int i = 0; i < maxTeamSize; i++)
        {
            if (!teamSlots.ContainsKey(i))
            {
                return i;
            }
        }
        Assert.IsTrue(true, "No slots opens");
        return SLOT_NULL;
    }

    public void SetPoints(int points)
    {
        TeamData.points = points;
    }

    public void AddPoints(int points)
    {
        TeamData.points += points;
    }

    public void SetFouls(int fouls)
    {
        TeamData.fouls = fouls;
    }

    public void AddFouls(int fouls)
    {
        TeamData.fouls += fouls;
    }
}