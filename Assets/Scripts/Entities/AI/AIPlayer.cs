using MLAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : NetworkedBehaviour
{

    protected Player m_player;
    protected AIDifficulty m_difficulty;
    protected GameObject m_object;

    // Start is called before the first frame update
    void Start()
    {
        m_difficulty = AIDifficulty.PRO;
    }

    public override void NetworkStart()
    {
        m_player = GetComponent<Player>();
        GameManager.AddAI(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwnedByServer) return;
        if (GameManager.Singleton.GameState.MatchStateValue == EMatchState.INPROGRESS)
        {
            if (GameManager.GetBallHandling().IsBallLoose())
            {
                // TODO should player try and get ball?
                return;
            }
            if (m_player.IsOnOffense())
            {
                if (Vector3.Distance(transform.position, m_player.OwnBasket.position) > 1f)
                {
                    float step = 3.0f * Time.deltaTime;
                    Vector3 move = Vector3.MoveTowards(transform.position, m_player.OwnBasket.position, step);
                    move.y = transform.position.y;
                    transform.position = move;
                }
                else
                {
                    if (m_player.HasBall)
                        Shoot();
                }
            }
            else if (m_player.IsOnDefense() && m_player.Assignment != null)
            {
                transform.position = m_player.Assignment.gameObject.transform.position + (m_player.Assignment.gameObject.transform.forward * 6);
                transform.LookAt(m_player.Assignment.gameObject.transform);
            }
        }
    }

    // An AI shooting method
    private void Shoot()
    {
        print("AI shoots the ball");
    }

    public Player GetPlayer()
    {
        return m_player;
    }

    internal void SetPlayer(Player p)
    {
        m_player = p;
    }
}
