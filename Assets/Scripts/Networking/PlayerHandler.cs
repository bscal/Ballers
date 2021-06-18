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

    public event Action<ulong, CharacterData> CharacterFetched;

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
                CharacterFetched?.Invoke(steamid, cData);
            }
            else
            {
                Debug.LogError(status);
            }
        }));
    }

    /// <summary>
    /// Server only. Fetches a players' character stats. Used for Server -> Backend
    /// </summary>
    public void GetPlayerStats(Player player)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (!ServerManager.Instance.players.TryGetValue(player.props.steamID, out ServerPlayer sPlayer)) return;

        StartCoroutine(BackendManager.FetchCharacterFromServer(player.props.steamID, sPlayer.cid, (cData, status) => {
            if (status == BackendManager.STATUS_OK)
            {
                m_focusedPlayer = cData;
                CharacterFetched?.Invoke(player.props.steamID, cData);
            }
            else
            {
                Debug.LogError(status);
            }
        }));
    }

    /// <summary>
    /// Server only. Fetches are players from ServerManager players map
    /// </summary>
    public void GetAllPlayersData()
    {
        foreach (Player player in GameManager.GetPlayers())
        {
            if (!player.props.isAI)
            {
                ServerPlayer sp = ServerManager.Instance.GetPlayer(player.props.steamID);
                StartCoroutine(BackendManager.FetchCharacterFromServer(player.props.steamID, sp.cid, (cData, status) => {
                    if (status == BackendManager.STATUS_OK)
                    {
                        player.cData = cData;
                        CharacterFetched?.Invoke(player.props.steamID, cData);
                    }
                    else
                    {
                        Debug.LogError(status);
                    }
                }));
            }
            else
            {
                // Not implemented in backend yet.
                StartCoroutine(BackendManager.FetchAIFromServer(player.aiPlayerID, (cData, status) => {
                    if (status == BackendManager.STATUS_OK)
                    {
                        player.cData = cData;
                        CharacterFetched?.Invoke(player.props.steamID, cData);
                    }
                    else
                    {
                        Debug.LogError(status);
                    }
                    
                }));
            }

        }
    }
}
