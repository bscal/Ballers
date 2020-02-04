using MLAPI;
using MLAPI.Messaging;
using MLAPI.Spawning;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkedBehaviour
{

    public static GameManager Singleton;

    public event Action Pregame;
    public event Action GameStarted;

    private static GameObject m_ball;
    private static BallHandling m_ballhandling;
    private static List<Player> m_players = new List<Player>();
    private static Dictionary<ulong, Player> m_playersByID = new Dictionary<ulong, Player>();
    private static Dictionary<ulong, uint> m_playersByTeam = new Dictionary<ulong, uint>();

    public Team TeamHome { get; private set; }
    public Team TeamAway { get; private set; }

    public int teamSize = 5;
    public bool lastShotMade = false;

    public int possession = 0;
    public Basket[] baskets = new Basket[2];
    public Vector3 centerCourt;
    public List<Vector3> inboundPositions;
    public Vector3[] teamInboundPos = new Vector3[2];
    public Vector3[] freethrowPos = new Vector3[2];

    private BasketballStateManager m_gameState;
    private float m_pregameTime = 0;

    [SerializeField]
    private GameObject m_ballPrefab;

    private bool HasGameStarted { get; set; }

    void Awake()
    {
        Singleton = this;

        m_gameState = GetComponent<BasketballStateManager>();

        baskets[0] = GameObject.Find("BasketLeft").GetComponent<Basket>();
        baskets[1] = GameObject.Find("BasketRight").GetComponent<Basket>();
        baskets[0].id = 0;
        baskets[1].id = 1;

        centerCourt = GameObject.Find("CenterCourt").transform.position;

        TeamHome = new Team(0, teamSize);
        TeamAway = new Team(1, teamSize);
    }
    void Start()
    {
        NetworkingManager.Singleton.OnClientConnectedCallback += OnConnected;
        m_gameState.OnHalfEnd += EndHalf;
    }

    public override void NetworkStart()
    {
    }

    void Update()
    {
        if (IsServer)
        {
            if (!HasGameStarted)
            {
                m_pregameTime += Time.deltaTime;
                if (m_pregameTime > 30.0 * 1000.0)
                {
                    HasGameStarted = true;
                }
            }
        }
    }

    public void StartGame()
    {
        GameStarted();

        if (IsServer)
        {
            if (m_ball == null)
            {
                m_ball = Instantiate(m_ballPrefab, new Vector3(1, 3, 1), Quaternion.identity);
                m_ball.GetComponent<NetworkedObject>().Spawn();
                m_ballhandling = m_ball.GetComponent<BallHandling>();
            }
            else
            {
                m_ballhandling.StopBall();
                m_ball.transform.position = new Vector3(1, 3, 1);
            }
        }

        Debug.Log("Game Starting!");
    }

    [ServerRPC]
    private void RegisterPlayerServer(ulong pid)
    {
        NetworkedObject netObj = SpawnManager.GetPlayerObject(pid);

        if (netObj.IsOwnedByServer) return;

        AddPlayer(netObj);
    }

    [ClientRPC]
    private void RegisterPlayerClient(ulong pid)
    {
        NetworkedObject netObj = SpawnManager.GetPlayerObject(pid);

        if (netObj.IsOwnedByServer) return;

        AddPlayer(netObj);
    }

    [ServerRPC]
    private void UnRegisterPlayerServer(ulong pid)
    {
        NetworkedObject netObj = SpawnManager.GetPlayerObject(pid);

        if (netObj.IsOwnedByServer) return;

        RemovePlayer(netObj);
    }

    [ClientRPC]
    private void UnRegisterPlayerClient(ulong pid)
    {
        NetworkedObject netObj = SpawnManager.GetPlayerObject(pid);

        if (netObj.IsOwnedByServer) return;

        RemovePlayer(netObj);
    }

    // Registers existing players to newly connected client
    [ClientRPC]
    private void RegisterOtherPlayer(ulong[] pids)
    {
        for (int i = 0; i < pids.Length; i++)
        {
            NetworkedObject netObj = SpawnManager.GetPlayerObject(pids[i]);

            if (netObj.IsOwnedByServer || netObj.IsLocalPlayer) return;

            AddPlayer(netObj);
        }
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

    public void AddScore(uint id, int points)
    {
        if (id == 0)
            TeamHome.points += points;
        else
            TeamAway.points += points;
    }

    public static void AddPlayer(NetworkedObject netObj)
    {
        print("Adding player " + netObj.OwnerClientId + " " + netObj.NetworkId);
        Player p = netObj.gameObject.GetComponent<Player>();

        if (m_players.Contains(p))
        {
            Debug.Log("GameManager: Attempted to re registered player. Skipping.");
            return;
        }

        m_players.Add(p);
        m_playersByID.Add(netObj.OwnerClientId, p);
        m_playersByTeam.Add(netObj.OwnerClientId, 0);
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
        m_playersByTeam.Remove(netObj.OwnerClientId);
    }

    public bool GetIsServer()
    {
        return IsServer;
    }

    public static GameObject GetBall()
    {
        return m_ball;
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

    public static List<Player> GetPlayers()
    {
        return m_players;
    }

    public static Dictionary<ulong, uint> GetPlayersByTeam()
    {
        return m_playersByTeam;
    }

    private void OnConnected(ulong client)
    {

    }

    private void InitLocalPlayer(ulong pid)
    {
        // Registers player to server
        InvokeServerRpc(RegisterPlayerServer, pid);
        // Registers players to all connected clients.
        InvokeClientRpcOnEveryoneExcept(RegisterPlayerClient, pid, pid);
    }

    private void StartPregame()
    {

    }
}
