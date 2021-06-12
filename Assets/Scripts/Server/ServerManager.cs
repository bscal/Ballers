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

    public const float SYNC_TIME = 1.0f;

    public static bool IsDedicated;

    public event Action AllPlayersLoaded;

    public GameObject playerPrefab;
    public GameObject aiPrefab;

    public GameObject redPrefab;
    public GameObject bluePrefab;

    public NetworkLobby m_lobby;
    public MatchSetup m_setup;

    [NonSerialized]
    public readonly Dictionary<ulong, ServerPlayer> players = new Dictionary<ulong, ServerPlayer>();
    [NonSerialized]
    public readonly Dictionary<ulong, GameObject> playerObjects = new Dictionary<ulong, GameObject>();
    [NonSerialized]
    public readonly List<Player> playersList = new List<Player>();

    private StartupState m_startupState = StartupState.NONE;
    private float m_syncCounter;


    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        m_lobby = GameObject.Find("NetworkManager").GetComponent<NetworkLobby>();
        m_setup = GameObject.Find("MatchManager").GetComponent<MatchSetup>();
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void Update()
    {
        if (IsServer)
        {
            if (m_startupState == StartupState.NONE)
                return;

            if (m_startupState == StartupState.LOADING && HaveAllPlayersLoaded())
            {
                print("loaded");
                m_startupState = StartupState.ENTER_GAME;

                CreateAI();
                LeanTween.delayedCall(1.0f, () => LoadAllPlayers());
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

            m_syncCounter += Time.deltaTime;
            if (m_syncCounter > SYNC_TIME)
            {
                m_syncCounter -= SYNC_TIME;
                playersList.ForEach((player) => player.SyncMatchClientRpc(Match.matchTeams[0], Match.matchTeams[1], RPCParams.ClientParamsOnlyClient(player.OwnerClientId)));

            }

        }
    }

    public void LoadAllPlayers()
    {
        AllPlayersLoaded?.Invoke();
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

    private void OnServerStarted()
    {
        if (m_lobby.IsDedicated)
        {
            Match.ResetDefaults();
            Match.InitMatch(new MatchSettings(BallersGamemode.SP_BOTS, 5, 4, 60.0 * 12.0, 24.0));
            Match.PlayersNeeded = 1;
            Match.MatchID = 1;
        }

        m_setup.SetServerManagerInstance(this);

        m_startupState = StartupState.LOADING;
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
            Player player = playerObject.GetComponent<Player>();
            Match.AssignPlayer(player);
            netObj.SpawnAsPlayerObject(id);
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
        //while (timeout > timer)
        //{
        yield return new WaitForSeconds(WAIT_TIME);
        //}
    }

    private bool HaveAllPlayersLoaded()
    {
        if (players.Count < Match.PlayersNeeded)
            return false;

        foreach (ServerPlayer sp in players.Values)
        {
            if (!sp.IsFullyLoaded())
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
            if (!sp.IsReady())
            {
                return false;
            }
        }
        return true;
    }

    private void CreateAI()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            // Initialize ai
            for (int tid = 0; tid < 2; tid++)
            {
                MatchTeam team = Match.matchTeams[tid];

                int aiToCreate = Match.MatchSettings.TeamSize - team.numOfPlayers;

                for (int i = 0; i < aiToCreate; i++)
                {
                    GameObject go = Instantiate(aiPrefab, Vector3.zero, Quaternion.identity);
                    GameObject modelObj = Instantiate(PrefabFromTeamID(tid), go.transform);

                    Player p = go.GetComponent<Player>();

                    Match.AssignPlayerWithTeam(p, tid);
                    p.InitilizeModel();

                    AIPlayer aiLogic = go.GetComponent<AIPlayer>();

                    aiLogic.InitPlayer(p, tid);

                    NetworkObject obj = go.GetComponent<NetworkObject>();
                    obj.Spawn();
                    
                }
            }
        }
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

