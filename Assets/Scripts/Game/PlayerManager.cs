using MLAPI;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerStats
{

}

public class PlayerManager : NetworkedBehaviour
{
    private const float UPDATE_TIME = 5.0f;

    private Dictionary<Player, PlayerStats> m_playerStats;

    private float m_timer;

    // Start is called before the first frame update
    void Start()
    {
        if (IsServer)
        {
            m_playerStats = new Dictionary<Player, PlayerStats>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            m_timer += Time.deltaTime;

            if (m_timer > UPDATE_TIME)
            {
                m_timer = 0;
            }
        }
    }

    public static float GetPlayerStat(Player player, string stat)
    {
        return 0f;
    }

    public static PlayerStats GetPlayerStats(Player player)
    {
        return null;
    }

    public static float FetchPlayerStat(ulong steamid, int cid, string stat)
    {
        return 0;
    }

    public static PlayerStats FetchPlayerAllStats(ulong steamid, int cid)
    {
        return null;
    }

    public static PlayerStats[] FetchEveryPlayersStats(ulong[] steamids, int[] cids)
    {
        return null;
    }

    private IEnumerator FetchPlayerFromServer(ulong steamid, int cid)
    {
        yield return null;
    }

}
