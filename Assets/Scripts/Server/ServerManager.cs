using MLAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StartupState
{
    NONE,
    LOADING,
    SETUP,
    PREGAME,
    STARTED
}

public class ServerManager : NetworkBehaviour
{
    public static ServerManager Singleton { get ; private set; }

    public static event Action AllPlayersLoaded;

    public readonly Dictionary<ulong, ServerPlayer> players = new Dictionary<ulong, ServerPlayer>();

    private NetworkLobby m_lobby;
    private PlayerHandler m_playerHandler;
    private StartupState m_startupState = StartupState.NONE;


    private void Awake()
    {
        Singleton = this;
        m_lobby = GetComponent<NetworkLobby>();
        m_playerHandler = GameObject.Find("NetworkClient").GetComponent<PlayerHandler>();
    }

    public void Start()
    {
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void Update()
    {
        if (m_startupState == StartupState.NONE)
            return;

        if (m_startupState == StartupState.LOADING && HaveAllPlayersLoaded())
        {
            m_startupState = StartupState.PREGAME;

            AllPlayersLoaded?.Invoke();
        }

        if (m_startupState == StartupState.PREGAME && AllPlayersReady())
        {
            m_startupState = StartupState.STARTED;

            GameManager.Singleton.BeginPregame();
        }
    }

    public void SetupServer()
    {
        m_startupState = StartupState.LOADING;
        OnClientConnected(NetworkManager.Singleton.LocalClientId);
    }

    public ServerPlayer GetPlayer(ulong id)
    {
        return (IsServer) ? players[id] : null;
    }

    public StartupState GetStartupState()
    {
        return m_startupState;
    }

    public void ResetDefaults()
    {
        players.Clear();
    }

    public void AssignPlayer(ulong id, int teamID)
    {
        if (Match.HostServer)
        {
            players[id].team = teamID;
        }
    }

    public void SetCharIDs(ulong id, ulong steamId, int cid)
    {
        if (players.TryGetValue(id, out ServerPlayer sp))
        {
            sp.cid = cid;
            sp.steamId = steamId;
        }
    }

    private void OnServerStarted()
    {
        //m_startupState = StartupState.LOADING;
        // These statements are a temp solution because starting a server (or host)
        // will not fire OnClientConnectedCallback
        //if (Match.HostServer)
        //{
        //    OnClientConnected(NetworkManager.Singleton.LocalClientId);
        //}
        
        //players.TryGetValue(ClientPlayer.Singleton.SteamID, out ServerPlayer sp);
        //sp.status = ServerPlayerStatus.CONNECTED;
        //sp.state = ServerPlayerState.LOADING;

        //m_playerHandler.GetAllPlayersData();
    }

    private void OnClientConnected(ulong id)
    {
        if (players.ContainsKey(id))
            return;

        Debug.Log("OnClientConnected " + id);

        ServerPlayer sp = new ServerPlayer(id);
        sp.status = ServerPlayerStatus.CONNECTED;
        sp.state = ServerPlayerState.WAITING_FOR_IDS;
        players.Add(id, sp);

        m_lobby.RequestIdsClientRpc(RPCParams.ClientParamsOnlyClient(id));
    }

    private void OnClientDisconnected(ulong id)
    {
        if (players.TryGetValue(id, out ServerPlayer player))
        {
            player.status = ServerPlayerStatus.DISCONNECTED;
            player.state = ServerPlayerState.NONE;
            players.Remove(id);
        }
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
            if (!sp.IsFullyConnected())
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
        
    }

    private void DestroyMatch()
    {
        Debug.Log("destroying match");
    }

    private void BeginPregame()
    {
        Debug.Log("Beginning");
        GameManager.Singleton.BeginPregame();
    }

    private void MigrateHost()
    {

    }

    public static ulong GetRTT(ulong clientId)
    {
        if (NetworkManager.Singleton.NetworkConfig.NetworkTransport.ServerClientId == clientId)
            return 0;
        return NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(clientId);
    }
}
