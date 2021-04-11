using MLAPI;
using MLAPI.Connection;
using MLAPI.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StartupState
{
    NONE,
    SETUP,
    LOADING,
    ENTER_GAME,
    PREGAME,
    STARTED
}

public class ServerManager : NetworkBehaviour
{
    public static ServerManager Singleton { get; private set; }

    public static bool IsDedicated;

    public event Action AllPlayersLoaded;

    public GameObject playerPrefab;
    public GameObject aiPrefab;

    public GameObject redPrefab;
    public GameObject bluePrefab;

    [NonSerialized]
    public readonly Dictionary<ulong, ServerPlayer> players = new Dictionary<ulong, ServerPlayer>();
    [NonSerialized]
    public readonly Dictionary<ulong, GameObject> playerObjects = new Dictionary<ulong, GameObject>();
    [NonSerialized]
    public readonly List<Player> playersList = new List<Player>();

    private NetworkLobby m_lobby;
    private MatchSetup m_setup;
    private StartupState m_startupState = StartupState.NONE;


    private void Awake()
    {
        Singleton = this;
        m_lobby = GetComponent<NetworkLobby>();
        m_setup = GameObject.Find("MatchManager").GetComponent<MatchSetup>();
    }

    private void Start()
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
            print("loaded");
            m_startupState = StartupState.ENTER_GAME;

            LoadAllPlayers();
        }

        if (m_startupState == StartupState.ENTER_GAME && HaveAllPlayersEntered())
        {
            print("entering");
            m_startupState = StartupState.PREGAME;

            AllPlayersEnteredGame();
        }

        if (m_startupState == StartupState.PREGAME && AllPlayersReady())
        {
            print("starting");
            m_startupState = StartupState.STARTED;

            GameManager.Singleton.BeginPregame();
        }
    }

    public void LoadAllPlayers()
    {
        AllPlayersLoaded?.Invoke();
    }

    public void CreateModel(ulong clientId)
    {
    }

    public void SetupHost()
    {
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
        if (IsServer)
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
        if (m_lobby.isDedicated)
        {
            Match.ResetDefaults();

            Match.MatchSettings = new MatchSettings(BallersGamemode.SP_BOTS, 5, 4, 60.0 * 12.0, 24.0);
            Match.PlayersNeeded = 1;
            Match.MatchID = 1;
        }

        m_setup.SetServerManagerInstance(this);

        m_startupState = StartupState.LOADING;

        print("starting");
    }

    private void OnClientConnected(ulong id)
    {
        if (players.ContainsKey(id))
            return;

        if (IsServer)
        {
            ServerPlayer sp = new ServerPlayer(id);
            sp.status = ServerPlayerStatus.CONNECTED;
            sp.state = ServerPlayerState.WAITING_FOR_IDS;
            players.Add(id, sp);

            GameObject playerObject = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            NetworkObject netObj = playerObject.GetComponent<NetworkObject>();
            netObj.SpawnAsPlayerObject(id);

            AddPlayer(netObj.NetworkObjectId, playerObject, playerObject.GetComponent<Player>());
        }
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


    private bool HaveAllPlayersEntered()
    {
        foreach (ServerPlayer sp in players.Values)
        {
            if (sp.state != ServerPlayerState.ENTERED_GAME)
            {
                return false;
            }
        }
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

    public void AddPlayer(ulong id, GameObject playerObj, Player player)
    {
        playerObjects.Add(id, playerObj);
        playersList.Add(player);
    }

    public static ulong GetRTT(ulong clientId)
    {
        if (NetworkManager.Singleton.NetworkConfig.NetworkTransport.ServerClientId == clientId)
            return 0;
        return NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(clientId);
    }
    public static GameObject PrefabFromTeamID(int teamID)
    {
        if (teamID == 1)
            return Singleton.bluePrefab;
        else
            return Singleton.redPrefab;
    }

    public void PlayerSceneChanged(ulong ownerClientId, ulong networkObjectId)
    {
        players[ownerClientId].state = ServerPlayerState.ENTERED_GAME;
    }

    private void AllPlayersEnteredGame()
    {
        foreach (var pairs in playerObjects)
        {
            Player p = pairs.Value.GetComponent<Player>();
            p.EnterGameClientRpc(p.props);
        }
    }
}
