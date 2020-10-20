﻿using System;
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

    public const string JUMP1 = "player@jump1";
    public const string CROSS_L_TO_R = "player@left_to_right";
    public const string CROSS_R_TO_L = "player@right_to_left";
    public const string BACKPEDDLE = "player@backpeddle";
    public const string BACKPEDDLE_BALL = "player@backpeddle_ball";
    public const string BLOCK = "player@block";
    public const string CONTEST_UP = "player@contest_up";
    public const string CONTEST_AT = "player@contest_at";
    public const string STEAL = "player@steal";
    public const string SWIPE = "player@swipe";
    public const string TRIPLE_THREAT = "player@triple_threat";
    public const string PASS_NORMAL1 = "player@pass_normal1";
    public const string DEF_STANCE = "player@def_stance";
    public const string DEF_STANCE_STRAFE_LEFT = "player@def_stance_left";
    public const string DEF_STANCE_STRAFE_RIGHT = "player@def_stance_right";
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
