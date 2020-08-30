using MLAPI;
using MLAPI.Messaging;
using MLAPI.Spawning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ServerManager : NetworkedBehaviour
{

    public static ServerManager Singleton { get; private set; }

    private void Awake()
    {
        Singleton = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        NetworkingManager.Singleton.OnServerStarted += OnServerStarted;
    }

    public void AddPlayer(ulong steamid, int cid)
    {
        if (Match.HostServer)
            ServerState.HandlePlayerConnection(steamid, cid);
    }

    public void RemovePlayer(ulong steamid)
    {
        if (Match.HostServer)
            ServerState.Players.Remove(steamid);
    }

    public ServerPlayer GetPlayer(ulong steamid)
    {
        if (Match.HostServer)
        {
            if (ServerState.Players.TryGetValue(steamid, out ServerPlayer sPlayer))
                return sPlayer;
        }
        return null;
    }

    public void ResetDefaults()
    {
        ServerState.Players.Clear();
    }

    public void AssignPlayer(ulong steamid, int teamID, int slot)
    {
        if (Match.HostServer)
        {
            ServerState.Players[steamid].team = teamID;
            ServerState.Players[steamid].slot = slot;
        }
    }

    private void OnServerStarted()
    {
        NetworkingManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkingManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        // These statements are a temp solution because starting a server (or host)
        // will not fire OnClientConnectedCallback
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
