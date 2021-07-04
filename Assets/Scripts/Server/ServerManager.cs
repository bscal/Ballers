using MLAPI;
using MLAPI.SceneManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ServerManager : NetworkBehaviour
{
    public const int STATE_NONE       = 0;
    public const int STATE_SETUP      = 1;
    public const int STATE_JOINING    = 2;
    public const int STATE_ENTERING   = 3;
    public const int STATE_LOADING    = 4;
    public const int STATE_PREGAME    = 5;
    public const int STATE_INPROGRESS = 6;
    public const int STATE_PAUSED     = 7;

    private const string LOG_PATH = "./ballers_server.log";
    private const float SYNC_TIME = 1.0f;

    public static ServerManager Instance { get; private set; }

    // I prefer this to be a static readonly, but unity wont allow this call in constuctors or fields.
    public static bool IS_DEDICATED_SERVER => Application.isBatchMode;

    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject aiPrefab;

    public GameObject redPrefab;
    public GameObject bluePrefab;

    [Header("Network Scripts")]
    public NetworkLobby m_lobby;
    public MatchSetup m_setup;

    [Header("Ballers Client")]
    public GameObject clientPrefab;
    public BallersClient ballersClient;

    // AI does not create a ServerPlayer object
    [NonSerialized]
    public readonly Dictionary<ulong, ServerPlayer> players = new Dictionary<ulong, ServerPlayer>();
    [NonSerialized]
    public readonly Dictionary<ulong, GameObject> playerObjects = new Dictionary<ulong, GameObject>();
    [NonSerialized]
    public readonly List<Player> playersList = new List<Player>();

    private int m_startupState;
    private float m_syncCounter;
    private FileStream m_file;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        m_lobby = GameObject.Find("NetworkManager").GetComponent<NetworkLobby>();
        m_setup = GameObject.Find("MatchManager").GetComponent<MatchSetup>();
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        if (IS_DEDICATED_SERVER)
        {
            
            if (File.Exists(LOG_PATH))
            {
                m_file = File.OpenWrite(LOG_PATH);
                m_file.SetLength(0);
                m_file.Flush();
            }
            else
            {
                m_file = File.Create(LOG_PATH);
            }
            m_file.Close();
            Application.logMessageReceivedThreaded += OnLogMessageCallback;
        }
        
    }

    private void Update()
    {
        if (IsServer)
        {
            if (m_startupState == STATE_NONE)
                return;
            
            if (m_startupState == STATE_JOINING && HaveAllPlayersJoined())
            {
                print("joined");
                m_startupState = STATE_ENTERING;

                CreateAI();
                LeanTween.delayedCall(1.0f, () => {
                    NetworkSceneManager.SwitchScene("Match");
                });
            }
            else if(m_startupState == STATE_ENTERING && HaveAllPlayersEntered())
            {
                print("entered");
                m_startupState = STATE_LOADING;

                foreach (var pair in playerObjects)
                {
                    Player p = pair.Value.GetComponent<Player>();
                    p.EnterGameServerSetup();
                    p.EnterGameClientRpc(p.props);
                }
            }
            else if(m_startupState == STATE_LOADING && HaveAllPlayersLoaded())
            {
                print("loaded");
                m_startupState = STATE_PREGAME;
                StartCoroutine(GameManager.Instance.BeginStart());
            }
            else if(m_startupState == STATE_PREGAME && GameManager.Instance.isReady && AllPlayersReady())
            {
                print("pregame");
                m_startupState = STATE_INPROGRESS;
                GameManager.Instance.BeginPregame();
            }

            if (ballersClient != null)
            {
                m_syncCounter += Time.deltaTime;
                if (m_syncCounter > SYNC_TIME)
                {
                    m_syncCounter -= SYNC_TIME;
                    ballersClient.SyncMatchClientRpc(Match.matchTeams[0].teamData, Match.matchTeams[1].teamData);
                }
            }
        }
    }

    private void OnLogMessageCallback(string condition, string stackTrace, UnityEngine.LogType type)
    {
        using FileStream fs = File.Open(LOG_PATH, FileMode.Append);
        string formattedSting = $"[{DateTime.Now.ToString("HH:mm:ss.ff")}][{type}]: {condition}\n";
        byte[] data = Encoding.UTF8.GetBytes(formattedSting);
        fs.Write(data, 0, data.Length);
        byte[] stackData = Encoding.UTF8.GetBytes(stackTrace + "\n");
        fs.Write(stackData, 0, stackData.Length);
    }

    public void SetupHost()
    {
        // Host does not call OnClientConnect so I call it manually
        OnClientConnected(NetworkManager.Singleton.LocalClientId);
    }

    public ServerPlayer GetServerPlayer(ulong id)
    {
        return (IsServer) ? players[id] : null;
    }

    public int GetStartupState()
    {
        return m_startupState;
    }

    private void OnServerStarted()
    {
        if (IS_DEDICATED_SERVER)
        {
            Match.ResetDefaults();
            Match.InitMatch(new MatchSettings(BallersGamemode.SP_BOTS, 5, 4, 60.0 * 12.0, 24.0));
            Match.PlayersNeeded = 1;
            Match.MatchID = 1;
        }

        GameObject client = Instantiate(clientPrefab);
        client.name = "BallersClient";
        client.GetComponent<NetworkObject>().Spawn();
        ballersClient = client.GetComponent<BallersClient>();

        m_setup.SetServerManagerInstance(this);

        m_startupState = STATE_JOINING;
    }

    private void OnClientConnected(ulong id)
    {
        if (players.ContainsKey(id))
            return;

        if (IsServer)
        {
            ServerPlayer sp = new ServerPlayer(id);
            sp.status = ServerPlayerStatus.CONNECTED;
            sp.hasJoined = true;
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
            player.hasJoined = false;
            player.hasLoaded = false;
            player.isReady = false;
            players.Remove(id);
        }
    }

    private bool HaveAllPlayersJoined()
    {
        // Checks if we have the correct number of players and AI if necessary
        if (playersList.Count < Match.PlayersNeeded)
            return false;

        foreach (ServerPlayer sp in players.Values)
        {
            if (!sp.hasJoined)
                return false;
        }
        return true;
    }

    private bool HaveAllPlayersEntered()
    {
        foreach (ServerPlayer sp in players.Values)
        {
            if (!sp.hasEnteredGame)
                return false;
        }
        return true;
    }

    private bool HaveAllPlayersLoaded()
    {
        foreach (ServerPlayer sp in players.Values)
        {
            if (!sp.hasLoaded)
                return false;
        }
        return true;
    }

    public void PlayerLoaded(ulong ownerClientId)
    {
        players[ownerClientId].hasLoaded = true;
    }

    public void PlayerReadyUp(ulong ownerClientId)
    {
        players[ownerClientId].isReady = true;
    }

    public void PlayerEnteredGame(ulong ownerClientId)
    {
        players[ownerClientId].hasEnteredGame = true;
    }

    public bool AllPlayersReady()
    {
        foreach (ServerPlayer sp in players.Values)
        {
            if (!sp.isReady)
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
        return (teamID == 1) ? Instance.bluePrefab : Instance.redPrefab;
    }
}

