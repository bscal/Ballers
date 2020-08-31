using MLAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerHandler : MonoBehaviour
{

    public event Action<CharacterData> CharacterFetched;

    private CharacterData m_localPlayer;
    private CharacterData m_focusedPlayer;

    private void Awake()
    {
    }

    /// <summary>
    /// Fetches a player's characters stats. Use for Client -> Backend
    /// </summary>
    public void GetClientPlayerStats(ulong steamid, int cid)
    {
        StartCoroutine(BackendManager.FetchCharacterFromServer(steamid, cid, (cData, status) => {
            if (status == BackendManager.STATUS_OK)
            {
                m_focusedPlayer = cData;
                CharacterFetched?.Invoke(cData);
            }
            Debug.LogError(status);
        }));
    }

    /// <summary>
    /// Server only. Fetches a players' character stats. Used for Server -> Backend
    /// </summary>
    public void GetPlayerStats(Player player)
    {
        if (!NetworkingManager.Singleton.IsServer) return;

        if (!ServerManager.Singleton.players.TryGetValue(player.SteamID, out ServerPlayer sPlayer)) return;

        StartCoroutine(BackendManager.FetchCharacterFromServer(player.SteamID, sPlayer.cid, (cData, status) => {
            if (status == BackendManager.STATUS_OK)
            {
                m_focusedPlayer = cData;
                CharacterFetched?.Invoke(cData);
            }
            Debug.LogError(status);
        }));
    }

    /// <summary>
    /// Server only. Fetches are players from ServerManager players map
    /// </summary>
    private void SyncCharacterData()
    {
        foreach (var pair in ServerManager.Singleton.players)
        {
            if (pair.Value.status == ServerPlayerStatus.CONNECTED)
            {
                StartCoroutine(BackendManager.FetchCharacterFromServer(pair.Value.steamId, pair.Value.cid, (cData, status) => {
                    if (status == BackendManager.STATUS_OK)
                    {
                        m_focusedPlayer = cData;
                        CharacterFetched?.Invoke(cData);
                    }
                    Debug.LogError(status);
                }));
            }
        }
    }

}
