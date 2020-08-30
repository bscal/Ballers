using MLAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class PlayerManager : NetworkedBehaviour
{
    public static PlayerManager Singleton;

    // Stores Character data by player. This is used by the server to ensure
    // valid stats.
    private Dictionary<Player, CharacterData> m_playerStats;

    // Start is called before the first frame update
    void Start()
    {
        Singleton = this;

        if (IsServer)
        {
            m_playerStats = new Dictionary<Player, CharacterData>();
        }
    }

    public float GetPlayerStat(Player player, string stat)
    {
        

        return 0f;
    }

    public CharacterData GetPlayerStats(Player player)
    {
        if (IsServer)
        {

        }
        return null;
    }

    private void SyncCharacterData()
    {
    }

}
