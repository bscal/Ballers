using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotController : MonoBehaviour
{

    private float m_distFromBasket;
    private float m_angleFromBasket;

    private Player m_player;

    void Start()
    {
        m_player = GetComponentInChildren<Player>();
    }

    void Update()
    {
        if (m_player.HasBall)
        {
            m_distFromBasket = Vector3.Distance(transform.position, m_player.OwnBasket.position);
            m_angleFromBasket = Quaternion.Angle(transform.rotation, m_player.OwnBasket.rotation);




            if (m_player.isSprinting)
            {

            }
            else if (m_player.isMoving)
            {

            }
        }

        Debugger.Instance.Print(string.Format("Dist: {0}, Angle:{1}, left: {2}", m_distFromBasket, m_angleFromBasket, m_player.OnLeftSide), 3);
    }
}
