using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{

    public Player player;
    public PlayerControls pControls;

    public void ShootAnimationHighest()
    {
        GameManager.Instance.ballController.AnimationReleaseServerRpc();
    }

    public void OnCrossover(int tooLeft)
    {
        if (player.HasBall)
        {
            player.props.isBallInLeftHand = !player.props.isBallInLeftHand;
        }
    }
}
