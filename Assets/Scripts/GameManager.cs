using MLAPI;
using MLAPI.Spawning;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkedBehaviour
{
    public Team TeamHome { get; private set; }
    public Team TeamAway { get; private set; }

    public bool gameStarted = false;

    private static GameObject m_ball;
    private static BallHandling m_ballhandling;
    private static List<Player> m_players = new List<Player>();
    private static Dictionary<ulong, Player> m_playersByID = new Dictionary<ulong, Player>();

    private GameStateManager m_gameState;
    [SerializeField]
    private GameObject m_ballPrefab;
    private Vector3 m_centerCourt;

    public Basket m_basketLeft;
    private Basket m_basketRight;

    public override void NetworkStart()
    {
        if (m_ball == null)
        {
            m_ball = Instantiate(m_ballPrefab, new Vector3(1, 3, 1), Quaternion.identity);
            m_ball.GetComponent<NetworkedObject>().Spawn();
            m_ballhandling = m_ball.GetComponent<BallHandling>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_basketLeft = GameObject.Find("BasketLeft").GetComponent<Basket>();
        //m_basketRight = GameObject.Find("BasketRight").GetComponent<Basket>();
        m_basketLeft.isHome = true;
        //m_basketRight.isHome = false;
        m_gameState = GetComponent<GameStateManager>();
        m_centerCourt = GameObject.Find("CenterCourt").transform.position;
        TeamHome = new Team(5);
        TeamAway = new Team(5);
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameStarted)
        {
            OnStartGame();
        }
    }

    private void OnStartGame()
    {
        m_gameState.OnStartGame();
        foreach (Player p in m_players)
        {
            p.OnStartGame();
        }
    }

    internal void EndQuarter()
    {
    }

    internal void EndHalf()
    {
        m_basketLeft.isHome = !m_basketLeft.isHome;
        m_basketRight.isHome = !m_basketRight.isHome;
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

    public void AddScore(bool isHome, int points)
    {
        if (isHome)
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

}
