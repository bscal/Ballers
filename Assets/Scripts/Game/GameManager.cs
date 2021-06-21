using MLAPI;
using MLAPI.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TeamType
{
    NONE = -1,
    HOME = 0,
    AWAY = 1
}

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action Pregame;                // When all players connected -> all ready or timer
    public event Action GameStartedServer;      // Once Pregame ends -> timer
    public event Action GameStartedClient;      // Once Pregame ends -> timer
    public event Action GameTipoff;             // Once the tip starts. Players gain control here
    public event Action GameEnd;                // When game ends
    public event Action Postgame;               // After game ends timer
    public event Action<Player> PlayerRegistered;

    private static List<Player> Players = new List<Player>();
    private readonly static List<BasicDummy> Dummies = new List<BasicDummy>();
    private readonly static List<AIPlayer> AIs = new List<AIPlayer>();

    public Player BallHandler { get { return GetPlayerByNetworkID(ballController.PlayerWithBall); } }
    public Basket CurrentBasket { get { return Instance.baskets[ballController.PossessionOrHome]; } }
    public int Possession { get { return ballController.PossessionOrHome; } }
    public MatchTeam TeamHome { get { return Match.matchTeams[0]; } }
    public MatchTeam TeamAway { get { return Match.matchTeams[1]; } }
    public bool HasStarted => Match.HasGameStarted;
    public BasketballStateManager GameState { get; private set; }

    public BallController ballController;
    public bool lastShotMade;
    public bool isFreeThrow;
    public GameObject ball;
    public Vector3 centerCourt;
    public List<Vector3> inboundPositions;
    public Basket[] baskets = new Basket[2];
    public Vector3[] freethrowPos = new Vector3[2];
    public GameObject[] inbounds;
    public GameObject loaderPrefab;
    public bool isReady;

    private ShotManager m_shotManager;
    private bool m_startRan;
    private bool m_networkStartRan;

    [SerializeField]
    private GameObject m_ballPrefab;

    void Awake()
    {
        Instance = this;

        GameState = GetComponent<BasketballStateManager>();
        m_shotManager = GetComponent<ShotManager>();

        baskets[0] = GameObject.Find("BasketLeft").GetComponent<Basket>();
        baskets[0].id = 0;
        baskets[1] = GameObject.Find("BasketRight").GetComponent<Basket>();
        baskets[1].id = 1;

        centerCourt = GameObject.Find("CenterCourt").transform.position;

        inbounds = GameObject.FindGameObjectsWithTag("Inbound");

        //GameState.GameEnd += OnGameEnd;
        //GameState.QuarterEnd += OnQuarterEnd;
        GameState.HalfEnd += OnHalfEnd;
        //GameState.ShotClockViolation += OnShotClockViolations;

        Players = ServerManager.Instance.playersList;
    }

    private void Start()
    {
        if (IsServer)
        {
            m_startRan = true;
        }
    }

    public override void NetworkStart()
    {
        if (IsServer)
        {
            m_networkStartRan = true;
        }
    }

    public IEnumerator BeginStart()
    {
        ball = Instantiate(m_ballPrefab, new Vector3(1, 3, 1), Quaternion.identity);
        ballController = ball.GetComponent<BallController>();
        ball.name = "Ball";
        ball.GetComponent<NetworkObject>().Spawn();
        while (!m_startRan || !m_networkStartRan)
        {
            yield return null;
        }
        print("Fully starting");
        FullStart();
    }

    private void FullStart()
    {
        isReady = true;
        FullStartClientRpc();
    }

    [ClientRpc]
    private void FullStartClientRpc()
    {
        ClientPlayer.Instance.localPlayer.clientControlsEnabled = true;
        isReady = true;
    }

    void Update()
    {

    }

    public void BeginPregame()
    {
        if (IsServer)
        {
            GameState.MatchStateValue = EMatchState.PREGAME;
            StartCoroutine(PregameTimer());
        }
        Pregame?.Invoke();
    }

    public void StartGame()
    {
        if (IsServer)
        {
            GameState.MatchStateValue = EMatchState.INPROGRESS;
            Match.HasGameStarted = true;
            GameStartedServer?.Invoke();
            StartGameClientRpc();
        }
    }

    [ClientRpc]
    public void StartGameClientRpc()
    {
        Match.HasGameStarted = true;
        GameStartedClient?.Invoke();
        Debug.Log("Game Starting!");
    }

    private IEnumerator PregameTimer()
    {
        const float TIMEOUT = 5;
        float timer = 0;

        DebugController.Singleton.PrintConsole($"Starting pregame... Game starts in {TIMEOUT} seconds or all players ready.");
        while (GameState.MatchStateValue == EMatchState.PREGAME)
        {
            yield return new WaitForSeconds(1);

            if (AreAllPlayersReady())
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

    public void HandleTurnover(Player p)
    {
        ballController.ChangePossession(p.OtherTeam, true, false);
    }

    public void OnHalfEnd()
    {
        Basket homeb = baskets[0];
        Basket awayb = baskets[1];

        baskets[0] = awayb;
        baskets[1] = homeb;
    }

    /// <summary> Returns size 2 int array. index 0 = home, 1 = away </summary>
    public int[] GetScores()
    {
        return new int[] { TeamHome.teamData.points, TeamAway.teamData.points };
    }

    /// <summary> 
    /// Returns difference in points between each team. <br></br>
    /// Formula: Home points - Away points
    /// </summary>
    public int GetScoreDifference()
    {
        return TeamHome.teamData.points - TeamAway.teamData.points;
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
        ballController.ChangePossession(ballController.FlipTeam(), false, true);
    }

    public void AddScore(int id, int points)
    {
        if (id == 0)
            TeamHome.AddPoints(points);
        else
            TeamAway.AddPoints(points);
    }

    public static void AddAI(AIPlayer ai)
    {
        if (AIs.Contains(ai))
        {
            Debug.LogWarning("AI already added to list");
            return;
        }

        AIs.Add(ai);
    }

    public static void RemoveAI(AIPlayer ai)
    {
        AIs.Remove(ai);
    }

    public void OutOfBounds()
    {
        Player p;
        if (ballController.PlayerWithBall == BallController.NO_PLAYER)
        {
            if (ballController.PlayerLastTouched == BallController.NO_PLAYER)
            {
                // If this happens something probably went wrong. We just handle with a jumpball.
                Debug.LogError("Both PlayerWithBall and PlayerLastTouched = NO_PLAYER");
                return;
            }
            p = GetPlayerByNetworkID(ballController.PlayerLastTouched);
            HandleTurnover(p);
            return;
        }
        p = GetPlayerByNetworkID(ballController.PlayerWithBall);
        if (!p.HasBall && p.transform.position.y > 0.1f) return;
        HandleTurnover(p);
    }

    public ShotManager GetShotManager()
    {
        return m_shotManager;
    }

    public BallController GetBallHandling()
    {
        return ballController;
    }

    public static Player GetPlayer()
    {
        return ClientPlayer.Instance.localPlayer;
    }

    public static List<Player> GetPlayersInRadius(float radius)
    {
        List<Player> res = new List<Player>();
        foreach (Player p in Players)
        {
            if (ClientPlayer.Instance.localPlayer.Dist(p.transform.position) < radius)
                res.Add(p);
        }
        return res;
    }

    public static List<Player> GetEnemeiesInRadius(float radius)
    {
        List<Player> res = new List<Player>();
        foreach (Player p in Match.matchTeams[ClientPlayer.Instance.localPlayer.OtherTeam].slotToPlayer.Values)
        {
            if (ClientPlayer.Instance.localPlayer.Dist(p.transform.position) < radius)
                res.Add(p);
        }
        return res;
    }

    /// <summary>
    /// Takes a NetworkID and returns Player matching that id. This can return AIs from the player list.
    /// </summary>
    public static Player GetPlayerByNetworkID(ulong netID)
    {
        return Players.Find((player) => player.NetworkObjectId == netID);
    }

    /// <summary>
    /// Takes an OwnerClientID and returns a Player matching that id. Will not return AI only player controlled Players
    /// </summary>
    public static Player GetPlayerByClientID(ulong clientID)
    {
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].OwnerClientId == clientID) return Players[i];
        }
        return null;
    }

    /// <summary>
    /// Takes a steamid and returns a Player with that id. Does not work for AI.
    /// </summary>
    public static Player GetPlayerBySteam(ulong steamid)
    {
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].props.steamID == steamid) return Players[i];
        }
        return null;
    }

    public static List<Player> GetPlayers()
    {
        return Players;
    }

    public static List<BasicDummy> GetDummies()
    {
        return Dummies;
    }

    public static void AddDummy(BasicDummy dummy)
    {
        Dummies.Add(dummy);
    }

    public static Basket GetBasket()
    {
        return Instance.CurrentBasket;
    }

    public bool AreAllPlayersReady()
    {
        for (int i = 0; i < Players.Count; i++)
        {
            if (!Players[i].hasReadyUp)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Synces the state the the given state data.
    /// This sync is done throughout the game.
    /// </summary>
    /// <param name="state"></param>
    public void SyncState(SyncedMatchStateData state)
    {
        Match.HasGameStarted = state.HasStarted;
        ballController.PlayerWithBall = state.PlayerWithBall;
        ballController.Possession = state.TeamWithPossession;
    }


    [ClientRpc]
    public void ClientSyncTeamSlotsClientRpc()
    {
        //using (PooledNetworkReader reader = PooledNetworkReader.Get(stream))
        //{
        //    int teamid = reader.ReadInt32Packed();
        //    int count = reader.ReadInt32Packed();
        //    teams[teamid].ReadSyncTeamSlots(reader, count);
        //}
    }
}
