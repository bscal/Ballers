using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{

    public Player player;
    public PlayerControls pControls;

    public void ShootAnimationHighest()
    {
        GameManager.GetBallHandling().InvokeServerRpc(GameManager.GetBallHandling().OnAnimationRelease);
    }

    public void OnCrossover(int tooLeft)
    {
        if (player.HasBall)
        {
            player.isBallInLeftHand = !player.isBallInLeftHand;
        }
    }
}
