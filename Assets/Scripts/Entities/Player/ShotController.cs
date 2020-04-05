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

    /// <summary>
    /// Returns the type of shot the player should take. 
    /// </summary>
    public ShotType GetTypeOfShot(Player p, float dist, ShotDirection direction)
    {
        if (dist < CLOSE_RANGE)
        {
            if (p.isMoving) return ShotType.LAYUP;
        }

        return ShotType.SHOT;
    }


}
