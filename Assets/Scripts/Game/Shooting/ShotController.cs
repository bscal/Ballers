using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShotType
{
    LAYUP,
    DUNK,
    SHOT,
    SHOT_CLOSE,
    POST_MOVE,
    POST_SHOT,
    FREETHROW,
    ALLEY_OOP
}

public enum ShotStyle
{
    LAYUP,
    LAYUP_FINGER_ROLL,
    LAYUP_EURO_STEP,
    LAYUP_SPIN,
    LAYUP_CONTACT,
    LAYUP_FLASHY,
    LAYUP_FLOATER,

    DUNK_ONE_H,
    DUNK_TWO_H,
    DUNK_FLASHY,
    DUNK_CONTACT,
    DUNK_SPIN,
    DUNK_EURO_STEP,

    SHOT,
    SHOT_CLOSE,
    SHOT_LONG,
    SHOT_FADE,
    SHOT_CONTESTED,
    SHOT_MOVING,
    SHOT_STEPBACK,
    SHOT_CATCH_SHOOT,

    POST_MOVE,
    POST_HOOK,
    POST_SPIN,
    POST_FADE,
    POST_STEPBACK,

    FREETHROW,
    ALLEY_LAYUP,
    ALLEY_DUNK
}

public enum ShotDirection
{
    FRONT,
    SIDE,
    BACK
}

public enum ShotRange
{
    CLOSE,
    MID,
    LONG
}

public enum BankType
{
    LEFT = 0,
    RIGHT = 1,
    NONE = 2,
}

public static class ShotController
{
    private const float RIM_RANGE = 6.0f;
    private const float CLOSE_RANGE = 9.0f;
    private const float LONGE_RANGE = 20.0f;

    /// <summary>
    /// Returns the type of shot the player should take. 
    /// </summary>
    public static ShotType GetShotType(Player p, float dist, ShotDirection direction)
    {
        if (p.props.isPostShot)
            return ShotType.POST_SHOT;

        if (p.props.isPostMove)
            return ShotType.POST_MOVE;

        if (dist < RIM_RANGE)
        {

            if (p.props.isSprinting) return ShotType.DUNK;
            else if (p.props.isMoving) return ShotType.LAYUP;
        }
        else if (dist < CLOSE_RANGE)
            return ShotType.SHOT_CLOSE;

        return ShotType.SHOT;
    }

    public static ShotStyle GetShotStyle(Player p, float dist, ShotDirection dir, ShotType type)
    {
        return ShotStyle.LAYUP;
    }

    public static ShotRange GetShotRange(ShotType type)
    {
        if (type == ShotType.SHOT)
            return ShotRange.LONG;
        return ShotRange.CLOSE;
    }

}
