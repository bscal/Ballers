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

    public GameObject ball;

    private Dictionary<int, Player> m_playersByID = new Dictionary<int, Player>();
    private GameStateManager m_gameState;
    [SerializeField]
    private GameObject m_ballPrefab;
    private Vector3 m_centerCourt;

    public override void NetworkStart()
    {
        if (ball == null)
        {
            ball = Instantiate(m_ballPrefab, new Vector3(1, 3, 1), Quaternion.identity);
            ball.GetComponent<NetworkedObject>().Spawn();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_gameState = GetComponent<GameStateManager>();
        m_centerCourt = GameObject.Find("CenterCourt").transform.position;
        TeamHome = new Team(5);
        TeamAway = new Team(5);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void EndQuarter()
    {
        throw new NotImplementedException();
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

    void AddNewPlayer(Player p)
    {
        m_playersByID.Add(p.id, p);
    }

}
