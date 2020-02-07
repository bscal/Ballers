using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : NetworkedBehaviour
{

    private Player m_player;
    private Animator m_animator;

    private bool m_shootCooldown = false;
    private bool m_jumpCooldown = false;

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

        if (GameManager.Singleton.HasStarted)
        {

        }

        m_player.isMoving = IsMoving();
        m_player.isSprinting = Input.GetKey(KeyCode.LeftShift);

        //
        // === Input Handling of Shooting ===
        //
        if (Input.GetKey(KeyCode.Y))
        {
            if (m_player.isShooting)
            {
                m_player.ReleaseBall();
                return;
            }
            else if (m_shootCooldown)
            {
                return;
            }

            m_player.ShootBall();
            m_animator.SetTrigger("Shoot");
            //m_animator.SetTrigger("Pump");
            StartCoroutine(WaitShoot(0.2f));
        }

        if (Input.GetKey(KeyCode.Space) && !m_jumpCooldown)
        {
            m_animator.SetTrigger("Jump");
            StartCoroutine(WaitJump(1.5f));
        }

        m_animator.SetBool("isDribbling", m_player.isDribbling);
        m_animator.SetBool("isSprinting", m_player.isSprinting);
        m_animator.SetBool("isWalking", m_player.isMoving);

        
    }
    private bool IsMoving()
    {
        return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
    }

    private IEnumerator WaitShoot(float delay)
    {
        m_shootCooldown = true;
        yield return new WaitForSeconds(delay);
        m_shootCooldown = false;
    }

    private IEnumerator WaitJump(float delay)
    {
        m_jumpCooldown = true;
        yield return new WaitForSeconds(delay);
        m_jumpCooldown = false;
    }


}
