using MLAPI;
using MLAPI.Messaging;
using MLAPI.Spawning;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Video;

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
    private readonly static List<Player> m_players = new List<Player>();
    private readonly static Dictionary<ulong, Player> m_playersByID = new Dictionary<ulong, Player>();
    private readonly static Dictionary<ulong, ulong> m_playersBySteam = new Dictionary<ulong, ulong>();
    private readonly static List<BasicDummy> m_dummies = new List<BasicDummy>();

    public Player BallHandler { get { return GetPlayer(m_ballhandling.PlayerWithBall); } }
    public Basket CurrentBasket { get { return Singleton.baskets[m_ballhandling.PossessionOrHome]; } }
    public int Possession { get { return m_ballhandling.PossessionOrHome; } }
    public Team TeamHome { get { return teams[0]; } }
    public Team TeamAway { get { return teams[1]; } }
    public bool HasStarted { get; private set; }

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

    private BasketballStateManager m_gameState;
    private ShotManager m_shotManager;
    private float m_pregameTime = 0;

    [SerializeField]
    private GameObject m_ballPrefab;

    void Awake()
    {
        Singleton = this;

        m_gameState = GetComponent<BasketballStateManager>();
        m_shotManager = GetComponent<ShotManager>();

        baskets[0] = GameObject.Find("BasketLeft").GetComponent<Basket>();
        baskets[1] = GameObject.Find("BasketRight").GetComponent<Basket>();
        baskets[0].id = 0;
        baskets[1].id = 1;

        centerCourt = GameObject.Find("CenterCourt").transform.position;

        teams[0] = new Team((int)TeamType.HOME, teamSize);
        teams[1] = new Team((int)TeamType.AWAY, teamSize);

        inbounds = GameObject.FindGameObjectsWithTag("Inbound");
    }

    void Start()
    {
        NetworkingManager.Singleton.OnClientConnectedCallback += OnConnected;

        m_gameState.OnHalfEnd += EndHalf;

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
            m_gameState.MatchStateValue = EMatchState.PREGAME;
            StartCoroutine(PregameTimer());
        }

    }

    public void StartGame()
    {
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

        if (IsServer)
        {
            m_gameState.MatchStateValue = EMatchState.INPROGRESS;
        }

        Debug.Log("Game Starting!");
    }

    private IEnumerator PregameTimer()
    {
        const float TIMEOUT = 30;
        float timer = 0;
        while (m_gameState.MatchStateValue == EMatchState.PREGAME)
        {
            yield return new WaitForSeconds(1);

            if (ServerState.AllPlayersReady())
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
        if (m_playersByID.TryGetValue(pid, out Player p)) 
            return; // Already registered

        NetworkedObject netObj = SpawnManager.GetPlayerObject(pid);
        AddPlayer(netObj, steamid);
        InvokeClientRpcOnEveryoneExcept(RegisterPlayerClient, pid, pid, steamid);
    }

    [ServerRPC]
    public void RegisterNPCServer(ulong pid, ulong steamid)
    {
        // Checks if already registered on server
        if (m_playersByID.TryGetValue(pid, out Player p))
            return; // Already registered

        NetworkedObject netObj = SpawnManager.GetPlayerObject(pid);
        AddPlayer(netObj, steamid);
        InvokeClientRpcOnEveryoneExcept(RegisterPlayerClient, pid, pid, steamid);
        PlayerConnected?.Invoke(pid);
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
        TeamHome.id = 1;
        TeamAway.id = 0;
    }

    /// <summary> Returns size 2 int array. index 0 = home, 1 = away </summary>
    public int[] GetScores()
    {
        return new int[] { TeamHome.points, TeamAway.points };
    }

    /// <summary> 
    /// Returns difference in points between each team. <br></br>
    /// Formula: Home points - Away points
    /// </summary>
    public int GetScoreDifference()
    {
        return TeamHome.points - TeamAway.points;
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
        m_ballhandling.ChangePossession(m_ballhandling.OtherTeam(), false, true);
    }

    public void AddScore(uint id, int points)
    {
        if (id == 0)
            TeamHome.points += points;
        else
            TeamAway.points += points;
    }

    public static void AddPlayer(NetworkedObject netObj, ulong steamid)
    {
        print("Adding player " + netObj.OwnerClientId + " " + netObj.NetworkId + " " + steamid);
        Player p = netObj.gameObject.GetComponent<Player>();

        if (m_players.Contains(p))
        {
            Debug.Log("GameManager: Attempted to re registered player. Skipping.");
            return;
        }

        m_players.Add(p);
        m_playersByID.Add(netObj.OwnerClientId, p);
        m_playersBySteam.Add(netObj.OwnerClientId, steamid);

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

    public bool GetIsServer()
    {
        return IsServer;
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
        m_playersByID.TryGetValue(NetworkingManager.Singleton.LocalClientId, out Player p);
        return p;
    }

    public static Player GetPlayer(ulong id)
    {
        m_playersByID.TryGetValue(id, out Player p);
        return p;
    }

    public static bool ContainsPlayer(ulong id)
    {
        return m_playersByID.ContainsKey(id);
    }

    public static List<Player> GetPlayers()
    {
        return m_players;
    }


    public static Player GetPlayerByPosition(Team team, int position)
    {
        if (position < 0 || position > team.playersInPosition.Length) return GetPlayer(team.playersInPosition[0]);
        return GetPlayer(team.playersInPosition[position]);
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

    public void InitLocalPlayer(ulong pid)
    {
        AddPlayer(SpawnManager.GetLocalPlayerObject(), ClientPlayer.Singleton.SteamID);
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
        m_ballhandling.PlayerWithBall = state.playerWithBall;
        m_ballhandling.Possession = state.teamWithPossession;
        teams[(int)TeamType.HOME] = state.teams[(int)TeamType.HOME];
        teams[(int)TeamType.AWAY] = state.teams[(int)TeamType.AWAY];
    }

    /// <summary>
    /// Syncs the essential data with the given client.
    /// Useful for reconnects, host changes, new connections.
    /// </summary>
    public void FullGameSync(ulong clientid)
    {

    }

}
