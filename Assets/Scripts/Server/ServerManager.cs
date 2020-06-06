using MLAPI;
using MLAPI.Messaging;
using MLAPI.Spawning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class ServerManager : NetworkedBehaviour
{

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        NetworkingManager.Singleton.OnServerStarted += OnServerStarted;
    }

    private void OnServerStarted()
    {
        NetworkingManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkingManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        // These statements are a temp solution because starting a server (or host)
        // will not fire OnClientConnectedCallback
        ServerState.HandlePlayerConnection(ClientPlayer.Singleton.SteamID);
        OnClientConnected(ClientPlayer.Singleton.SteamID);
        ServerState.Players.TryGetValue(ClientPlayer.Singleton.SteamID, out ServerPlayer sp);
        sp.state = ServerPlayerState.READY;
        sp.status = ServerPlayerStatus.CONNECTED;

        StartCoroutine(ServerState.PlayersLoadedCoroutine(30));
    }

    private void OnClientConnected(ulong steamId)
    {
        if (ServerState.Players.TryGetValue(steamId, out ServerPlayer player))
        {
            player.status = ServerPlayerStatus.CONNECTED;
            player.state = ServerPlayerState.READY;
        }
    }

    private void OnClientDisconnected(ulong steamId)
    {
        if (ServerState.Players.TryGetValue(steamId, out ServerPlayer player))
        {
            player.SetStatus(ServerPlayerStatus.DISCONNECTED);
        }
    }

}
