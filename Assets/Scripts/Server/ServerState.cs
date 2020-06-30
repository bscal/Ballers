using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class ServerState
{

    public static Dictionary<ulong, ServerPlayer> Players = new Dictionary<ulong, ServerPlayer>();

    public static void HandlePlayerConnection(ulong steamId)
    {
        if (GameManager.Singleton == null) return;
        if (GameManager.ContainsPlayer(steamId))
        {
            return;
        }
        else
        {
            Players.Add(steamId, new ServerPlayer(steamId));
        }
    }

    public static IEnumerator PlayersLoadedCoroutine(float timeout)
    {
        const float WAIT_TIME = 3f;
        float timer = 0f;
        while (timeout > timer)
        {
            timer += WAIT_TIME;
            if (HaveAllPlayersLoaded())
            {
                StartMatch();
                break;
            }
            if (timer > timeout)
            {
                DestroyMatch();
                break;
            }
            yield return new WaitForSeconds(WAIT_TIME);
        }
    }

    private static bool HaveAllPlayersLoaded()
    {
        if (Players.Count < Match.PlayersNeeded)
            return false;
        foreach (ServerPlayer sp in Players.Values)
        {
            if (sp.status == ServerPlayerStatus.DISCONNECTED)
            {
                return false;
            }
        }
        // All players have loaded
        return true;
    }

    public static bool AllPlayersReady()
    {
        foreach (ServerPlayer sp in Players.Values)
        {
            if (sp.state != ServerPlayerState.READY)
            {
                return false;
            }
        }
        return true;
    }

    private static void StartMatch()
    {
        Debug.Log("starting match");
    }

    private static void DestroyMatch()
    {
        Debug.Log("destroying match");
    }

    public static void MigrateHost()
    {

    }
}
