using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkedVar.Collections;
using MLAPI.Serialization.Pooled;
using MLAPI.Spawning;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TeamType
{
    NONE = -1,
    HOME = 0,
    AWAY = 1
}

public class GameManager : NetworkedBehaviour
{

    public static GameManager Singleton { get; private set; }

    public event Action Pregame;
    public event Action GameStarted;
    public event Action GameBegin;
    public event Action GameEnd;
    public event Action<Player> PlayerLoaded;
    public event Action<Player> LocalPlayerLoaded;
    public event Action<ulong> PlayerConnected;
    public event Action<ulong> AllPlayersConnected;

    private static BallHandling m_ballhandling;
    private readonly static NetworkedList<Player> m_players = new NetworkedList<Player>(NetworkConstants.PLAYER_CHANNEL);

    private readonly static Dictionary<ulong, Player> m_playersByID = new Dictionary<ulong, Player>();
    private readonly static Dictionary<ulong, ulong> m_playersBySteam = new Dictionary<ulong, ulong>();
    private readonly static List<BasicDummy> m_dummies = new List<BasicDummy>();
    private readonly static List<AIPlayer> m_ais = new List<AIPlayer>();
    

    public Player BallHandler { get { return GetPlayerByNetworkID(m_ballhandling.PlayerWithBall); } }
    public Basket CurrentBasket { get { return Singleton.baskets[m_ballhandling.PossessionOrHome]; } }
    public int Possession { get { return m_ballhandling.PossessionOrHome; } }
    public Team TeamHome { get { return teams[0]; } }
    public Team TeamAway { get { return teams[1]; } }
    public bool HasStarted { get; private set; }
    public BasketballStateManager GameState { get; private set; }

    public int teamSize = 5;
    public bool lastShotMade = false;
    public GameObject ball;
    public Vector3 centerCourt;
    public List<Vector3> inboundPositions;
    public Team[] teams = new Team[2];
    public Basket[] baskets = new Basket[2];
    public Vector3[] freethrowPos = new Vector3[2];
    public GameObject[] inbounds;
    public GameObject loaderPrefab;

    private ShotManager m_shotManager;
    private float m_pregameTime = 0;

    [SerializeField]
    private GameObject m_ballPrefab;

    void Awake()
    {
        Singleton = this;

        GameState = GetComponent<BasketballStateManager>();
        m_shotManager = GetComponent<ShotManager>();

        baskets[0] = GameObject.Find("BasketLeft").GetComponent<Basket>();
        baskets[0].id = 0;
        baskets[1] = GameObject.Find("BasketRight").GetComponent<Basket>();
        baskets[1].id = 1;

        centerCourt = GameObject.Find("CenterCourt").transform.position;

        teams = new Team[2];
        teams[0] = new Team((int)TeamType.HOME, teamSize);
        teams[1] = new Team((int)TeamType.AWAY, teamSize);

        inbounds = GameObject.FindGameObjectsWithTag("Inbound");
    }

    void Start()
    {
        NetworkingManager.Singleton.OnClientConnectedCallback += OnConnected;
        NetworkingManager.Singleton.OnClientDisconnectCallback += OnDisconnected;

        GameState.OnHalfEnd += EndHalf;

        //m_players.ForEach((p) => { p.StartLoad(); });
    }

    public override void NetworkStart()
    {
        if (IsClient)
        {
            NetworkEvents.Singleton.RegisterEvent(NetworkEvent.GAME_START, this, OnStartGame);
        }

        if (IsServer)
        {
            ball = Instantiate(m_ballPrefab, new Vector3(1, 3, 1), Quaternion.identity);
            ball.GetComponent<NetworkedObject>().Spawn();
            m_ballhandling = ball.GetComponent<BallHandling>();
            ball.SetActive(false);
            ball.name = "Ball";
        }
        if (IsServer)
        {
            //StartCoroutine(DEBUG_SERVER_UPDATE());
        }
    }

    void Update()
    {
        if (IsServer)
        {
            if (!HasStarted)
            {
                m_pregameTime += Time.deltaTime;
                if (m_pregameTime > 30.0 * 1000.0)
                {
                    HasStarted = true;
                }
            }
        }

        if (m_ballhandling == null)
        {
            m_ballhandling = GameObject.FindGameObjectWithTag("Ball")?.GetComponent<BallHandling>();
        }

    }

    private IEnumerator DEBUG_SERVER_UPDATE()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);
            foreach (var v in NetworkingManager.Singleton.ConnectedClients)
            {
                Debug.Log($"{v.Key} | {v.Value.ClientId}");
            }
        }
    }

    public void LocalPlayerInitilized()
    {
        LocalPlayerLoaded?.Invoke(GetPlayer());
    }

    public void StartPregame()
    {
        Pregame?.Invoke();

        if (IsServer)
        {
            GameState.MatchStateValue = EMatchState.PREGAME;
            StartCoroutine(PregameTimer());
        }

    }

    public void StartGame()
    {
        // TODO tip ball?
        GameState.MatchStateValue = EMatchState.INPROGRESS;
        if (IsClient)
        {
            NetworkEvents.Singleton.CallEventServer(NetworkEvent.GAME_START);
        }
        else if (IsServer)
        {
            NetworkEvents.Singleton.CallEventAllClients(NetworkEvent.GAME_START);
        }
    }

    public void OnStartGame()
    {
        GameStarted?.Invoke();
        HasStarted = true;
        Debug.Log("Game Starting!");
    }

    private IEnumerator PregameTimer()
    {
        const float TIMEOUT = 30;
        float timer = 0;
        while (GameState.MatchStateValue == EMatchState.PREGAME)
        {
            yield return new WaitForSeconds(1);

            if (ServerManager.Singleton.AllPlayersReady())
            {
                yield return StartCountDown();
                break;
            }

            timer += 1;
            if (TIMEOUT < timer)
            {
                yield return StartCountDown();
                break;
            }
        }
    }

    private IEnumerator StartCountDown()
    {
        Debug.Log("Game starting in 5 seconds...");
        yield return new WaitForSeconds(2);
        Debug.Log("Game starting in 3 seconds...");
        yield return new WaitForSeconds(1);
        Debug.Log("Game starting in 2 seconds...");
        yield return new WaitForSeconds(1);
        Debug.Log("Game starting in 1 seconds...");
        yield return new WaitForSeconds(1);
        Debug.Log("Game starting!");
        StartGame();
    }


    [ServerRPC]
    public void RegisterPlayerServer(ulong pid, ulong steamid)
    {
        // Checks if already registered on server
        NetworkedObject netPlayer = SpawnManager.GetPlayerObject(pid);
        if (netPlayer == null) return;
        Player player = netPlayer.GetComponent<Player>();

        //if (m_playersByID.TryGetValue(pid, out Player player)) 
        //    return; // Already registered

        if (!player.isAI)
        {
            player.TeamID = ServerManager.Singleton.players[steamid].team;
            player.slot = ServerManager.Singleton.players[steamid].slot;
        }

        AddPlayer(player, steamid);

        InvokeClientRpcOnEveryone(RegisterPlayerClient, pid, steamid);


    }

    [ClientRPC]
    public void RegisterPlayerClient(ulong pid, ulong steamid)
    {
        NetworkedObject netObj = SpawnManager.GetPlayerObject(pid);

        if (netObj.IsOwnedByServer) return;

        AddPlayer(netObj, steamid);
        PlayerConnected?.Invoke(pid);
    }

    [ServerRPC]
    public void UnRegisterPlayerServer(ulong pid)
    {
        NetworkedObject netObj = SpawnManager.GetPlayerObject(pid);

        if (netObj.IsOwnedByServer) return;

        RemovePlayer(netObj);
    }

    [ClientRPC]
    public void UnRegisterPlayerClient(ulong pid)
    {
        NetworkedObject netObj = SpawnManager.GetPlayerObject(pid);

        if (netObj.IsOwnedByServer) return;

        RemovePlayer(netObj);
    }

    // Registers existing players to newly connected client
    [ClientRPC]
    public void RegisterOtherPlayer(ulong[] pids, uint[] teamids, ulong[] steamids)
    {
        for (int i = 0; i < pids.Length; i++)
        {
            NetworkedObject netObj = SpawnManager.GetPlayerObject(pids[i]);

            if (netObj.IsOwnedByServer || netObj.IsLocalPlayer) return;

            AddPlayer(netObj, steamids[i]);
        }
    }

    public void Turnover()
    {
        print("turnover");
    }

    public void EndHalf()
    {
        Basket homeb = baskets[0];
        Basket awayb = baskets[1];

        baskets[0] = awayb;
        baskets[1] = homeb;
    }

    /// <summary> Returns size 2 int array. index 0 = home, 1 = away </summary>
    public int[] GetScores()
    {
        return new int[] { TeamHome.TeamData.points, TeamAway.TeamData.points };
    }

    /// <summary> 
    /// Returns difference in points between each team. <br></br>
    /// Formula: Home points - Away points
    /// </summary>
    public int GetScoreDifference()
    {
        return TeamHome.TeamData.points - TeamAway.TeamData.points;
    }

    public GameObject GetClosestInbound(Vector3 pos)
    {
        float closestDist = 0f;
        int index = 0;

        for (int i = 0; i < inbounds.Length; i++)
        {
            GameObject inbound = inbounds[i];

            float dist = Vector3.Distance(pos, inbound.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                index = i;
            }
        }

        return inbounds[index];
    }

    public void ChangePossession()
    {
        print(Possession);
        m_ballhandling.ChangePossession(m_ballhandling.FlipTeam(), false, true);
    }

    public void AddScore(uint id, int points)
    {
        if (id == 0)
            TeamHome.AddPoints(points);
        else
            TeamAway.AddPoints(points);
    }

    public static void AddPlayer(NetworkedObject obj, ulong steamid)
    {
        AddPlayer(obj.GetComponent<Player>(), steamid);
    }

    public static void AddPlayer(Player p, ulong steamid)
    {
        print("Adding player " + p.OwnerClientId + " " + p.NetworkId + " " + steamid);

        if (m_players.Contains(p))
        {
            Debug.Log("GameManager: Attempted to re registered player. Skipping.");
            return;
        }

        if (NetworkingManager.Singleton.IsServer)
        {
            m_players.Add(p);
            Singleton.teams[p.TeamID].teamSlots.Add(p.slot, p);
        }

        if (!p.isAI)
        {
            p.SteamID = steamid;
            m_playersByID.Add(p.OwnerClientId, p);
            m_playersBySteam.Add(p.OwnerClientId, steamid);
        }
        
        Singleton.PlayerLoaded?.Invoke(p);
    }

    public static void RemovePlayer(NetworkedObject netObj)
    {
        print("Removing player " + netObj.OwnerClientId + " " + netObj.NetworkId);
        m_playersByID.TryGetValue(netObj.OwnerClientId, out Player p);

        if (p == null)
        {
            Debug.LogError("GameManager: Attempted to remove non registered/null player.");
            return;
        }

        m_players.Remove(p);
        m_playersByID.Remove(netObj.OwnerClientId);
        m_playersBySteam.Remove(netObj.OwnerClientId);
    }

    public static void AddAI(AIPlayer ai)
    {
        if (m_ais.Contains(ai))
        {
            Debug.LogWarning("AI already added to list");
            return;
        }

        m_ais.Add(ai);
        AddPlayer(ai.GetPlayer(), 0);
    }

    public static void RemoveAI(AIPlayer ai)
    {
        m_ais.Remove(ai);
    }

    public ShotManager GetShotManager()
    {
        return m_shotManager;
    }

    public static BallHandling GetBallHandling()
    {
        return m_ballhandling;
    }

    public static Player GetPlayer()
    {
        return GetPlayerByClientID(NetworkingManager.Singleton.LocalClientId);
    }

    /// <summary>
    /// Takes a NetworkID and returns Player matching that id. This can return AIs from the player list.
    /// </summary>
    public static Player GetPlayerByNetworkID(ulong netID)
    {
        for (int i = 0; i < m_players.Count; i++)
        {
            Player p = m_players[i];
            if (p.NetworkId == netID) return m_players[i];
        }
        return null;
    }

    /// <summary>
    /// Takes an OwnerClientID and returns a Player matching that id. Will not return AI only player controlled Players
    /// </summary>
    public static Player GetPlayerByClientID(ulong clientID)
    {
        for (int i = 0; i < m_players.Count; i++)
        {
            if (m_players[i].OwnerClientId == clientID) return m_players[i];
        }
        return null;
    }

    /// <summary>
    /// Takes a steamid and returns a Player with that id. Does not work for AI.
    /// </summary>
    public static Player GetPlayerBySteam(ulong steamid)
    {
        for (int i = 0; i < m_players.Count; i++)
        {
            if (m_players[i].SteamID == steamid) return m_players[i];
        }
        return null;
    }

    /// <summary>
    /// Takes a team and slot id and returns a Player from that team slot.
    /// </summary>
    public static Player GetPlayerBySlot(int teamID, int slot)
    {
        int clampTeamID = Mathf.Clamp(teamID, 0, 1);
        int campSlot = Mathf.Clamp(slot, 0, Match.MatchSettings.TeamSize);
        return Singleton.teams[clampTeamID].teamSlots[campSlot];
    }

    public static bool ContainsPlayer(ulong id)
    {
        for (int i = 0; i < m_players.Count; i++)
        {
            if (m_players[i].OwnerClientId == id) return true;
        }
        return false;
    }

    public static NetworkedList<Player> GetPlayers()
    {
        return m_players;
    }

    public static List<BasicDummy> GetDummies()
    {
        return m_dummies;
    }

    public static void AddDummy(BasicDummy dummy)
    {
        m_dummies.Add(dummy);
    }

    public static Basket GetBasket()
    {
        return Singleton.CurrentBasket;
    }

    private void OnConnected(ulong client)
    {

    }
    private void OnDisconnected(ulong client)
    {
        Debug.Log($"Disconnecting client {client}.");
        if (SpawnManager.GetLocalPlayerObject() == null || SpawnManager.GetLocalPlayerObject().OwnerClientId == client)
            SceneManager.LoadScene(0);
    }

    public void InitLocalPlayer(ulong pid)
    {
        //AddPlayer(SpawnManager.GetLocalPlayerObject(), ClientPlayer.Singleton.SteamID);
        // Registers player to server
        InvokeServerRpc(RegisterPlayerServer, pid, ClientPlayer.Singleton.SteamID);
    }

    /// <summary>
    /// Synces the state the the given state data.
    /// This sync is done throughout the game.
    /// </summary>
    /// <param name="state"></param>
    public void SyncState(SyncedMatchStateData state)
    {
        HasStarted = state.HasStarted;
        m_ballhandling.PlayerWithBall = state.PlayerWithBall;
        m_ballhandling.Possession = state.TeamWithPossession;
        teams[(int)TeamType.HOME].TeamData = state.Teams[(int)TeamType.HOME];
        teams[(int)TeamType.AWAY].TeamData = state.Teams[(int)TeamType.AWAY];
    }


    [ClientRPC]
    public void ClientSyncTeamSlots(Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            int teamid = reader.ReadInt32Packed();
            int count = reader.ReadInt32Packed();
            teams[teamid].ReadSyncTeamSlots(reader, count);
        }
    }

    /// <summary>
    /// Syncs the essential data with the given client.
    /// Useful for reconnects, host changes, new connections.
    /// </summary>
    public void FullGameSync(ulong clientid)
    {

    }

}
