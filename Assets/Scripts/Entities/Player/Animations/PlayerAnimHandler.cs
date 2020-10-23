using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.AI;

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

    public const string REBOUND = "player@rebound";
    public const string STRAFE_RIGHT = "player@strafe_right";
    public const string STRAFE_LEFT = "player@strafe_left";
    public const string STRAFE_DRIBBLE_RIGHT = "player@strafe_dribble_right";
    public const string STRAFE_DRIBBLE_LEFT = "player@strafe_dribble_left";
    public const string BACKPEDDLE = "player@backpeddle";
    public const string BACKPEDDLE_BALL = "player@backpeddle_dribble";
    public const string TRIPLE_THREAT = "player@triple_threat";
    public const string TRIPLE_THREAT_UP = "player@triple_threat_up";
    public const string TRIPLE_THREAT_DOWN = "player@triple_threat_down";


    // TODO
    public const string CROSS_L_TO_R = "player@left_to_right";
    public const string CROSS_R_TO_L = "player@right_to_left";
    public const string BLOCK = "player@block";
    public const string CONTEST_UP = "player@contest_up";
    public const string CONTEST_AT = "player@contest_at";
    public const string STEAL = "player@steal";
    public const string SWIPE = "player@swipe";
    public const string PASS_NORMAL1 = "player@pass_normal1";
    public const string DEF_STANCE = "player@def_stance";
    public const string DEF_STANCE_STRAFE_LEFT = "player@def_stance_left";
    public const string DEF_STANCE_STRAFE_RIGHT = "player@def_stance_right";
}

public class PlayerAnimHandler : MonoBehaviour
{
    private Player m_player;
    private Animator m_animator;

    private string m_curState = "";
    private string m_newState;
    private bool m_override = false;

    void Awake()
    {
        m_player = GetComponent<Player>();
        Assert.IsNotNull(m_player);
    }

    private void Update()
    {
        if (m_override && m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            return;
        }
        m_override = false;
        m_newState = null;

        if (m_player.isMoving)
        {
            if (!m_player.isSprinting)
            {
                if (m_player.movingFoward)
                {
                    if (m_player.isDribbling)
                        TryNewState(AnimNames.JOG_DRIB);
                    else
                        TryNewState(AnimNames.JOG);
                }
                else if (m_player.movingBack)
                {
                    if (m_player.isDribbling)
                        TryNewState(AnimNames.BACKPEDDLE_BALL);
                    else
                        TryNewState(AnimNames.BACKPEDDLE);
                }
                if (m_player.movingLeft)
                {
                    if (m_player.isDribbling)
                        TryNewState(AnimNames.STRAFE_DRIBBLE_LEFT);
                    else
                        TryNewState(AnimNames.STRAFE_LEFT);
                }
                else if (m_player.movingRight)
                {
                    if (m_player.isDribbling)
                        TryNewState(AnimNames.STRAFE_DRIBBLE_RIGHT);
                    else
                        TryNewState(AnimNames.STRAFE_RIGHT);
                }
            }
            else
            {
                if (m_player.isDribbling)
                    TryNewState(AnimNames.RUN_DRIB);
                else
                    TryNewState(AnimNames.RUN);
            }
        }

        if (m_player.isDribbling)
            TryNewState(AnimNames.IDLE_DRIB);
        else if (m_player.HasBall)
            TryNewState(AnimNames.TRIPLE_THREAT);
        else
            TryNewState(AnimNames.IDLE);


        ApplyState(m_newState);
    }

    public void SetAnimator(Animator animator)
    {
        m_animator = animator;
    }

    public void Play(string state)
    {
        if (m_curState == state)
            return;

        m_override = true;
        m_newState = state;
        ApplyState(state);
    }

    private bool TryNewState(string state)
    {
        if (string.IsNullOrEmpty(m_newState))
        {
            m_newState = state;
            return true;
        }
        return false;
    }

    private void ApplyState(string state)
    {
        m_curState = state;
        m_animator.Play(state);
    }
}
