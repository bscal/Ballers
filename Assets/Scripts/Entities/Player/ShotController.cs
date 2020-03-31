using MLAPI.NetworkedVar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShotType
{
    LAYUP,
    DUNK,
    SHOT
}

public enum ShotDirection
{
    FRONT,
    SIDE,
    BACK
}

public class ShotController : MonoBehaviour
{
    private const float CLOSE_RANGE = 6.0f;
    private const float LONGE_RANGE = 20.0f;

    private float m_distFromBasket;

    private Player m_player;

    void Awake()
    {
        m_player = GetComponentInChildren<Player>();
    }

    public ShotType HandleShotAnimation()
    {
        return ShotType.SHOT;
    }
}
