using MLAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : NetworkBehaviour
{

    protected Player m_player;
    protected AIDifficulty m_difficulty;
    protected GameObject m_object;

    // Start is called before the first frame update
    void Start()
    {
        m_difficulty = AIDifficulty.PRO;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner || !Match.HasGameStarted) return;
        if (GameManager.Instance.GameState.MatchStateValue == EMatchState.INPROGRESS)
        {
            if (GameManager.Instance.ballController.IsBallLoose())
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
                transform.position = m_player.Assignment.gameObject.transform.position + (m_player.Assignment.gameObject.transform.forward * 6) + (m_player.Assignment.gameObject.transform.right * 2); ;
                transform.LookAt(m_player.Assignment.gameObject.transform);
            }
        }
    }

    public void InitPlayer(Player p, int teamID)
    {
        SetPlayer(p);
        m_player.props.isAI = true;
        m_player.props.teamID = teamID;
        m_player.hasReadyUp = true;

        GameManager.AddAI(this);
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
