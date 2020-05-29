using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            ServerState.Players.Add(steamId, new ServerPlayer(steamId));
        }
    }
}
