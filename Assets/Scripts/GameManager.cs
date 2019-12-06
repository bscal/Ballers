using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private bool gameStarted = false;
    private bool gameStopped = false;

    private Dictionary<int, Player> m_playersByID = new Dictionary<int, Player>();
    private Gamemode m_gamemode;
    private Team m_teamHome;
    private Team m_teamAway;

    // Start is called before the first frame update
    void Start()
    {
        m_gamemode = new Gamemode(60 * 5);
        m_teamHome = new Team(5);
        m_teamAway = new Team(5);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStarted)
        {
            if (gameStopped)
            {
                m_gamemode.IncrementTime(Time.deltaTime);
            }
        }
    }

    void SyncGameManager()
    {

    }

    void AddNewPlayer(Player p)
    {
        m_playersByID.Add(p.id, p);
    }

}
