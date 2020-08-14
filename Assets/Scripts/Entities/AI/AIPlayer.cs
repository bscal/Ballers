using MLAPI;
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
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwnedByServer) return;
        if (GameManager.Singleton.GameState.MatchStateValue == EMatchState.INPROGRESS)
        {
            if (m_player.OnOffense())
            {
                if (Vector3.Distance(transform.position, m_player.OtherBasket.position) > 3f)
                {
                    float step = 5.0f * Time.deltaTime;
                    Vector3 move = Vector3.MoveTowards(transform.position, m_player.OtherBasket.position, step);
                    move.y = transform.position.y;
                    transform.position = move;
                }
                else
                {
                    Shoot();
                }
            }
            else
            {
                if (m_player.Assignment == null) return;
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
}
