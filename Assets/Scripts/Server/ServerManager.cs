using MLAPI;
using MLAPI.Messaging;
using MLAPI.Spawning;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ServerManager : NetworkedBehaviour
{
    public static ServerManager Singleton { get; private set; }

    public readonly Dictionary<ulong, ServerPlayer> players = new Dictionary<ulong, ServerPlayer>();

    private PlayerHandler m_playerHandler;

    private void Awake()
    {
        Singleton = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        NetworkingManager.Singleton.OnServerStarted += OnServerStarted;

        m_playerHandler = GameObject.Find("NetworkClient").GetComponent<PlayerHandler>();
    }

    public void AddPlayer(ulong steamid, int cid)
    {
        if (Match.HostServer)
            HandlePlayerConnection(steamid, cid);
    }

    public void RemovePlayer(ulong steamid)
    {
        if (Match.HostServer)
            players.Remove(steamid);
    }

    public ServerPlayer GetPlayer(ulong steamid)
    {
        if (Match.HostServer)
        {
            if (players.TryGetValue(steamid, out ServerPlayer sPlayer))
                return sPlayer;
        }
        return null;
    }

    public void ResetDefaults()
    {
        players.Clear();
    }

    public void AssignPlayer(ulong steamid, int teamID, int slot)
    {
        if (Match.HostServer)
        {
            players[steamid].team = teamID;
            players[steamid].slot = slot;
        }
    }

    private void OnServerStarted()
    {
        NetworkingManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkingManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        // These statements are a temp solution because starting a server (or host)
        // will not fire OnClientConnectedCallback
        OnClientConnected(ClientPlayer.Singleton.SteamID);
        players.TryGetValue(ClientPlayer.Singleton.SteamID, out ServerPlayer sp);
        sp.state = ServerPlayerState.READY;
        sp.status = ServerPlayerStatus.CONNECTED;

        m_playerHandler.GetAllPlayersData();

        StartCoroutine(PlayersLoadedCoroutine(30));
    }

    private void OnClientConnected(ulong steamId)
    {
        if (players.TryGetValue(steamId, out ServerPlayer player))
        {
            player.status = ServerPlayerStatus.CONNECTED;
            player.state = ServerPlayerState.READY;
        }
    }

    private void OnClientDisconnected(ulong steamId)
    {
        if (players.TryGetValue(steamId, out ServerPlayer player))
        {
            player.SetStatus(ServerPlayerStatus.DISCONNECTED);
        }
    }

    public void HandlePlayerConnection(ulong steamId, int cid)
    {
        if (GameManager.ContainsPlayer(steamId))
            return;
        else
            players.Add(steamId, new ServerPlayer(steamId, cid));
    }

    private IEnumerator PlayersLoadedCoroutine(float timeout)
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

    private bool HaveAllPlayersLoaded()
    {
        if (players.Count < Match.PlayersNeeded)
            return false;
        foreach (ServerPlayer sp in players.Values)
        {
            if (sp.status == ServerPlayerStatus.DISCONNECTED)
            {
                return false;
            }
        }
        // All players have loaded
        return true;
    }

    public bool AllPlayersReady()
    {
        foreach (ServerPlayer sp in players.Values)
        {
            if (sp.state != ServerPlayerState.READY)
            {
                return false;
            }
        }
        return true;
    }

    private void StartMatch()
    {
        Debug.Log("starting match");
    }

    private void DestroyMatch()
    {
        Debug.Log("destroying match");
    }

    public void MigrateHost()
    {

    }
}
