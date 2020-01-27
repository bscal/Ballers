using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : NetworkedBehaviour
{

    private Player m_player;
    private Animator m_animator;

    // Start is called before the first frame update
   void Start()
   {
        m_player = GetComponent<Player>();
        m_animator = GetComponentInChildren<Animator>();
   }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner || IsServer && !IsHost)
            return;

        m_player.isMoving = IsMoving();
        m_player.isSprinting = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetKey(KeyCode.Y))
        {
            m_player.ShootBall();
            m_animator.SetBool("isShooting", true);
        }
        else
        {
            m_animator.SetBool("isShooting", false);
        }

        m_animator.SetBool("isJumping", Input.GetKey(KeyCode.Space));
        
        m_animator.SetBool("isDribbling", m_player.isDribbling);
        m_animator.SetBool("isSprinting", m_player.isSprinting);
        m_animator.SetBool("isWalking", m_player.isMoving);
    }
    private bool IsMoving()
    {
        return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
    }
}
