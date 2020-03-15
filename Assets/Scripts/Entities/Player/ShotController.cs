using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotController : MonoBehaviour
{
    private const float CLOSE_RANGE = 6.0f;
    private const float LONGE_RANGE = 20.0f;

    private float m_distFromBasket;

    private Player m_player;

    void Start()
    {
        m_player = GetComponentInChildren<Player>();

        m_player.Shoot += OnShoot;
    }

    private void OnShoot(Player p)
    {
        float f = Quaternion.Angle(transform.rotation, m_player.LookRotation);
        m_distFromBasket = Vector3.Distance(transform.position, m_player.OwnBasket.position);
        if (f > 45)
        {
            if (f > 125)
            {
                print("fade");
            }
            else
            {
                print("side");
            }
        }
        else
        {
            print("front");
        }
    }
}
