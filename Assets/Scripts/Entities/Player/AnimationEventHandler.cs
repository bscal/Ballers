using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{

    public Player player;
    public PlayerControls pControls;

    public void ShootAnimationHighest(int seconds)
    {
        GameManager.GetBallHandling().BallFollowArc();
    }

    public void OnCrossover(int tooLeft)
    {
        
        if (player.HasBall)
        {
            player.IsBallInLeftHand = tooLeft != 0;
        }
    }
}
