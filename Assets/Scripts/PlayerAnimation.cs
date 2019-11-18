using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public WalkState State { get; set; }

    private Animator m_animator;
    private Animation m_anim;

    // Start is called before the first frame update
    void Start()
    {
        State = WalkState.IDLE;

        m_animator = GetComponent<Animator>();
        m_anim = GetComponent<Animation>();
    }

    // Update is called once per frame
    void Update()
    {
        m_animator.SetBool("isWalking", IsWalking());
        m_animator.SetBool("isDribbling", Input.GetKey(KeyCode.G));
    }

    private bool IsWalking()
    {
        return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
    }
}

public enum WalkState
{
    NONE,
    IDLE
}
