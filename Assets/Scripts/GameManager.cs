using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    private Dictionary<int, Player> m_playersByID = new Dictionary<int, Player>();
    private GameStateManager m_gameState;
    public Team TeamHome { get; private set; }
    public Team TeamAway { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        m_gameState = GetComponent<GameStateManager>();
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
