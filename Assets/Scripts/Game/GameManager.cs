using MLAPI;
using MLAPI.Spawning;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkedBehaviour
{

    public static GameManager Singleton;

    public event Action OnStartGame;

    private static GameObject m_ball;
    private static BallHandling m_ballhandling;
    private static List<Player> m_players = new List<Player>();
    private static Dictionary<ulong, Player> m_playersByID = new Dictionary<ulong, Player>();

    public Team TeamHome { get; private set; }
    public Team TeamAway { get; private set; }

    public int teamSize = 5;
    public bool gameStarted = false;
    public bool lastShotMade = false;

    public Basket[] baskets = new Basket[2];
    public Vector3 centerCourt;
    public List<Vector3> inboundPositions;
    public Vector3[] teamInboundPos = new Vector3[2];
    public Vector3[] freethrowPos = new Vector3[2];

    private BasketballStateManager m_gameState;

    [SerializeField]
    private GameObject m_ballPrefab;



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
        m_gameState.OnHalfEnd += EndHalf;
    }

    public override void NetworkStart()
    {
    }

    void Update()
    {
    }

    public void StartGame()
    {
        OnStartGame();

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

        Debug.Log("Game Starting!");
    }

    internal void EndHalf()
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

    public static void AddPlayer(Player p, NetworkedObject netObj)
    {
        m_players.Add(p);
        m_playersByID.Add(netObj.OwnerClientId, p);
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

}
