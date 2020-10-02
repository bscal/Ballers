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
using UnityEngine.Assertions;
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

    public event Action Pregame;            // When all players connected -> all ready or timer
    public event Action GameStarted;        // Once Pregame ends -> timer
    public event Action GameTipoff;         // Once the tip starts. Players gain control here
    public event Action GameEnd;            // When game ends
    public event Action Postgame;           // After game ends timer
    public event Action<Player> PlayerRegistered;
    public event Action<Player> LocalPlayerLoaded;
    public event Action AllPlayersConnected;

    private static BallHandling BallHandling;
    private readonly static NetworkedList<Player> Players = new NetworkedList<Player>(NetworkConstants.PLAYER_CHANNEL);
    private readonly static List<BasicDummy> Dummies = new List<BasicDummy>();
    private readonly static List<AIPlayer> AIs = new List<AIPlayer>();
    private static Player LocalPlayer;

    public Player BallHandler { get { return GetPlayerByNetworkID(BallHandling.PlayerWithBall); } }
    public Basket CurrentBasket { get { return Singleton.baskets[BallHandling.PossessionOrHome]; } }
    public int Possession { get { return BallHandling.PossessionOrHome; } }
    public Team TeamHome { get { return teams[0]; } }
    public Team TeamAway { get { return teams[1]; } }
    public bool HasStarted { get; private set; }
    public BasketballStateManager GameState { get; private set; }

    public bool lastShotMade;
    public bool isFreeThrow;
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
        teams[0] = new Team((int)TeamType.HOME, Match.MatchSettings.TeamSize);
        teams[1] = new Team((int)TeamType.AWAY, Match.MatchSettings.TeamSize);

        inbounds = GameObject.FindGameObjectsWithTag("Inbound");

        NetworkingManager.Singleton.OnClientConnectedCallback += OnConnected;
        NetworkingManager.Singleton.OnClientDisconnectCallback += OnDisconnected;
        //GameState.GameEnd += OnGameEnd;
        //GameState.QuarterEnd += OnQuarterEnd;
        GameState.HalfEnd += OnHalfEnd;
        //GameState.ShotClockViolation += OnShotClockViolations;
        AllPlayersConnected += OnAllPlayersConnected;
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
            BallHandling = ball.GetComponent<BallHandling>();
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

        if (BallHandling == null)
        {
            BallHandling = GameObject.FindGameObjectWithTag("Ball")?.GetComponent<BallHandling>();
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
        Assert.IsNotNull(SpawnManager.GetLocalPlayerObject().gameObject.GetComponent<Player>(), "LocalPlayerObject is null.");
        LocalPlayer = SpawnManager.GetLocalPlayerObject().gameObject.GetComponent<Player>();
        LocalPlayerLoaded?.Invoke(LocalPlayer);
    }

    public void StartPregame()
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

    public void OnAllPlayersConnected()
    {
        StartPregame();
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


    [ServerRPC]
    public void RegisterPlayerServer(ulong pid, ulong steamid)
    {
        // Checks if already registered on server
        NetworkedObject netPlayer = SpawnManager.GetPlayerObject(pid);

        Assert.IsNotNull(netPlayer, "NetworkedObject is null");

        Player player = netPlayer.GetComponent<Player>();

        if (!player.isAI)
        {
            player.teamID = ServerManager.Singleton.players[steamid].team;
            player.slot = ServerManager.Singleton.players[steamid].slot;
        }

        // Add the player to game.
        AddPlayer(player, steamid);
        InvokeClientRpcOnClient(RegisterPlayerClient, pid);
        DebugController.Singleton.PrintConsole($"Client registered id:{pid} steam:{steamid}.");
    }

    [ClientRPC]
    public void RegisterPlayerClient()
    {
        Assert.IsNotNull(LocalPlayer, "Local Player is null but client registered?");
        Debug.Log("Client successfully registered");
        Singleton.PlayerRegistered?.Invoke(LocalPlayer);
    }

    [ServerRPC]
    public void UnRegisterPlayerServer(ulong pid)
    {
        NetworkedObject netObj = SpawnManager.GetPlayerObject(pid);

        if (netObj.IsOwnedByServer) return;

        RemovePlayer(netObj);
    }

    public void HandleTurnover(Player p)
    {
        BallHandling.ChangePossession(p.OtherTeam, true, false);
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
        BallHandling.ChangePossession(BallHandling.FlipTeam(), false, true);
    }

    public void AddScore(int id, int points)
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

        if (Players.Contains(p))
        {
            Debug.LogWarning("GameManager: Attempted to re registered player. Skipping.");
            return;
        }

        if (NetworkingManager.Singleton.IsServer)
            Players.Add(p);

        p.steamID = steamid;
        p.slot = Singleton.teams[p.teamID].GetOpenSlot();
        Singleton.teams[p.teamID].teamSlots.Add(p.slot, p);
    }

    public static void RemovePlayer(NetworkedObject netObj)
    {
        print("Removing player " + netObj.OwnerClientId + " " + netObj.NetworkId);
        Player p = GetPlayerByClientID(netObj.OwnerClientId);

        if (p == null)
        {
            Debug.LogError("GameManager: Attempted to remove non registered/null player.");
            return;
        }

        Players.Remove(p);
    }

    public static void AddAI(AIPlayer ai)
    {
        if (AIs.Contains(ai))
        {
            Debug.LogWarning("AI already added to list");
            return;
        }

        AIs.Add(ai);
        AddPlayer(ai.GetPlayer(), 0);
    }

    public static void RemoveAI(AIPlayer ai)
    {
        AIs.Remove(ai);
    }

    public void OutOfBounds()
    {
        Player p;
        if (BallHandling.PlayerWithBall == BallHandling.NO_PLAYER)
        {
            if (BallHandling.PlayerLastTouched == BallHandling.NO_PLAYER)
            {
                // If this happens something probably went wrong. We just handle with a jumpball.
                Debug.LogError("Both PlayerWithBall and PlayerLastTouched = NO_PLAYER");
                return;
            }
            p = GetPlayerByNetworkID(BallHandling.PlayerLastTouched);
            HandleTurnover(p);
            return;
        }
        p = GetPlayerByNetworkID(BallHandling.PlayerWithBall);
        if (!p.HasBall && p.transform.position.y > 0.1f) return;
        HandleTurnover(p);
    }

    public ShotManager GetShotManager()
    {
        return m_shotManager;
    }

    public static BallHandling GetBallHandling()
    {
        return BallHandling;
    }

    public static Player GetPlayer()
    {
        return LocalPlayer;
    }

    /// <summary>
    /// Takes a NetworkID and returns Player matching that id. This can return AIs from the player list.
    /// </summary>
    public static Player GetPlayerByNetworkID(ulong netID)
    {
        for (int i = 0; i < Players.Count; i++)
        {
            Player p = Players[i];
            if (p.NetworkId == netID) return Players[i];
        }
        return null;
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
            if (Players[i].steamID == steamid) return Players[i];
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
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].OwnerClientId == id) return true;
        }
        return false;
    }

    public static NetworkedList<Player> GetPlayers()
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
        return Singleton.CurrentBasket;
    }

    private void OnConnected(ulong clientID)
    {
        if (IsServer)
        {
            if (NetworkingManager.Singleton.ConnectedClientsList.Count == Match.MatchSettings.TeamSize)
            {
                AllPlayersConnected?.Invoke();
            }
        }
        
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

    private void OnDisconnected(ulong clientID)
    {
        Debug.Log($"Disconnecting client {clientID}.");
        if (SpawnManager.GetLocalPlayerObject() == null || SpawnManager.GetLocalPlayerObject().OwnerClientId == clientID)
            SceneManager.LoadScene(0);
    }

    public void RegisterLocalPlayerToServer(ulong pid)
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
        BallHandling.PlayerWithBall = state.PlayerWithBall;
        BallHandling.Possession = state.TeamWithPossession;
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
