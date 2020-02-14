using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{

    public Player player;
    public PlayerControls pControls;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShootAnimationHighest(int seconds)
    {
        GameManager.GetBallHandling().BallFollowArc();
    }
}
