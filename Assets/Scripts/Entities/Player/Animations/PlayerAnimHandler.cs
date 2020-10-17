using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Any animations that starts with player@ is an animations that is duplicated and edited in unity.
/// </summary>
public static class AnimNames
{
    public const string IDLE = "player@idle";
    public const string WALK = "player@walk";
    public const string JOG = "player@jog";
    public const string RUN = "player@run";

    public const string IDLE_DRIB = "player@idle_dribble";
    public const string WALK_DRIB = "player@walk_dribble";
    public const string JOG_DRIB = "player@jog_dribble";
    public const string RUN_DRIB = "player@run_dribble";

    public const string REG_PUMPFAKE = "player@reg_pumpfake";
    public const string REG_JUMPSHOT = "player@reg_jumpshot";
}

public class PlayerAnimHandler : MonoBehaviour
{
    private Player m_player;
    private Animator m_animator;

    private string m_curState;

    void Awake()
    {
        m_player = GetComponent<Player>();
        Assert.IsNotNull(m_player);
    }

    public void SetAnimator(Animator animator)
    {
        m_animator = animator;
    }

    public void PlayAnim(string newState)
    {
        PlayAnim(newState, true);
    }

    public void PlayAnim(string newState, bool interrupt)
    {
        ChangeAnimState(newState, interrupt);
    }

    private void ChangeAnimState(string newState, bool interrupt)
    {
        if (m_curState == newState)
            return;

        if (!interrupt && m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            return;

        m_animator.Play(newState);

        m_curState = newState;
    }
}

public class AnimContainer
{
    public string name;
    public int weight;
    public bool interruptible;

    public bool keepLooped;

    public AnimContainer(string name, int weight, bool interruptible)
    {
        this.name = name;
        this.weight = weight;
        this.interruptible = interruptible;
    }
}
