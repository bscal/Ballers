using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Player m_player;
    private Animator m_animator;

    // Start is called before the first frame update
    void Start()
    {
        m_player = GetComponent<Player>();
        m_animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        m_animator.SetBool("isDribbling", m_player.IsDribbling);
        m_animator.SetBool("isSprinting", m_player.IsSprinting);
        m_animator.SetBool("isWalking", m_player.IsWalking);
    }

}
