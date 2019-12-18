using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{

    private Player m_player;
    private Animator m_animator;

    // Start is called before the first frame update
   void Start()
   {
        m_player = GetComponent<Player>();
        //m_animator = GetComponent<Animator>();
        m_animator = GetComponentInChildren<Animator>();
   }

    // Update is called once per frame
    void Update()
    {
        m_player.IsMoving = IsMoving();
        m_player.IsSprinting = Input.GetKey(KeyCode.LeftShift);

        m_animator.SetBool("isJumping", Input.GetKey(KeyCode.Space));
        m_animator.SetBool("isShooting", Input.GetKey(KeyCode.Y));
        m_animator.SetBool("isDribbling", m_player.IsDribbling);
        m_animator.SetBool("isSprinting", m_player.IsSprinting);
        m_animator.SetBool("isWalking", m_player.IsMoving);
    }
    private bool IsMoving()
    {
        return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
    }
}
