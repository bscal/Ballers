using MLAPI.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MatchTeam : INetworkSerializable
{
    public const int SLOT_NULL = -1;
    public const int SLOT_PG = 0;
    public const int SLOT_SG = 1;
    public const int SLOT_SF = 2;
    public const int SLOT_PF = 3;
    public const int SLOT_C = 4;

    public List<ulong> playerIds;
    public Dictionary<int, Player> slotToPlayer;
    public bool[] slots;
    public int numOfPlayers;

    private int m_teamSize;

    /// <summary>
    /// Represents team's values throughout the game
    /// </summary>
    public TeamData teamData;

    public MatchTeam(int size)
    {
        teamData = new TeamData();
        playerIds = new List<ulong>(size);
        slotToPlayer = new Dictionary<int, Player>(size);
        slots = new bool[size];
        m_teamSize = size;
    }

    public int GetNumOfOpenSlots()
    {
        int total = 0;
        for (int i = 0; i < m_teamSize; i++)
        {
            if (!slots[i])
                total++;
        }
        return total;
    }

    public bool IsSlotOpen(int slot)
    {
        return slots[Mathf.Clamp(slot, 0, m_teamSize-1)];
    }

    public int NextSlot()
    {
        for (int i = 0; i < m_teamSize; i++)
        {
            if (!slots[i])
            {
                return i;
            }
        }
        return -1;
    }

    public void AddSlot(int slot, Player p)
    {
        playerIds.Add(p.NetworkObjectId);
        slotToPlayer.Add(slot, p);
        slots[slot] = true;
        p.props.slot = slot;
        numOfPlayers++;
    }

    public void RemoveSlot(Player p)
    {
        playerIds.Remove(p.NetworkObjectId);
        slotToPlayer.Remove(p.props.slot);
        slots[p.props.slot] = false;
        p.props.slot = -1;
    }

    public Player GetPlayerBySlot(int slot)
    {
        slot = Mathf.Clamp(slot, 0, m_teamSize - 1);
        return slots[slot] ? slotToPlayer[slot] : null;
    }

    public void SetPoints(int points)
    {
        teamData.points = points;
    }

    public void AddPoints(int points)
    {
        teamData.points += points;
    }

    public void SetFouls(int fouls)
    {
        teamData.fouls = fouls;
    }

    public void AddFouls(int fouls)
    {
        teamData.fouls += fouls;
    }

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        teamData.NetworkSerialize(serializer);
    }
}

[Serializable]
public struct TeamData : INetworkSerializable
{
    /// <summary>
    /// Team points;
    /// </summary>
    public int points;
    /// <summary>
    /// Team fouls
    /// </summary>
    public int fouls;

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref points);
        serializer.Serialize(ref fouls);
    }
}
