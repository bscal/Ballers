using System;
using System.Collections.Generic;
using UnityEngine;

public class MatchTeam
{
    public List<ulong> playerIds;
    public Dictionary<ulong, Player> netIdsToPlayer;
    public Dictionary<int, ulong> slotToNetIds;
    public bool[] slots;
    public int teamSize;
    public int numOfPlayers;
    public int numOfAI;

    public MatchTeam(int size)
    {
        playerIds = new List<ulong>(size);
        netIdsToPlayer = new Dictionary<ulong, Player>(size);
        slotToNetIds = new Dictionary<int, ulong>(size);
        slots = new bool[size];
        teamSize = size;
    }

    public int GetNumOfOpenSlots()
    {
        int total = 0;
        for (int i = 0; i < teamSize; i++)
        {
            if (!slots[i])
                total++;
        }
        return total;
    }

    public bool IsSlotOpen(int slot)
    {
        return slots[Mathf.Clamp(slot, 0, teamSize-1)];
    }

    public int NextSlot()
    {
        for (int i = 0; i < teamSize; i++)
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
        netIdsToPlayer.Add(p.NetworkObjectId, p);
        slotToNetIds.Add(slot, p.NetworkObjectId);
        slots[slot] = true;
        p.props.slot = slot;
    }

    public void RemoveSlot(Player p)
    {
        netIdsToPlayer.Remove(p.NetworkObjectId);
        slotToNetIds.Remove(p.props.slot);
        slots[p.props.slot] = false;
        p.props.slot = -1;
    }
}
