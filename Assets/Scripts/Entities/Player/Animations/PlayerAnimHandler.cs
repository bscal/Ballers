using UnityEngine;
using UnityEngine.Assertions;

public static class AnimNames
{
    public static readonly int IDLE = Animator.StringToHash("Armature|idle");
    public static readonly int WALK = Animator.StringToHash("Armature|walk");
    public static readonly int JOG = Animator.StringToHash("Armature|jog");
    public static readonly int SPRINT = Animator.StringToHash("Armature|sprint");
    public static readonly int JUMP = Animator.StringToHash("Armature|jump");

    public static readonly int IDLE_DRIBBLE = Animator.StringToHash("Armature|idle_dribble");
    public static readonly int WALK_DRIBBLE = Animator.StringToHash("Armature|walk_dribble");
    public static readonly int JOG_DRIBBLE = Animator.StringToHash("Armature|jog_dribble");
    public static readonly int SPRINT_DRIBBLE = Animator.StringToHash("Armature|sprint_dribble");

    public static readonly int REG_PUMPFAKE = Animator.StringToHash("Armature|reg_pumpfake");
    public static readonly int REG_JUMPSHOT = Animator.StringToHash("Armature|reg_standing_shot");

    public static readonly int ILDE_TRIPLE_THREAT = Animator.StringToHash("Armature|idle_triple_threat");
    
    public static readonly int STRAFE_RIGHT = Animator.StringToHash("Armature|strafe_right");
    public static readonly int STRAFE_LEFT = Animator.StringToHash("Armature|strafe_left");
    public static readonly int STRAFE_DRIBBLE_RIGHT = Animator.StringToHash("Armature|strafe_right");
    public static readonly int STRAFE_DRIBBLE_LEFT = Animator.StringToHash("Armature|strafe_left");
    
    //public const string REBOUND = "player@rebound";
    //public const string BACKPEDDLE = "player@backpeddle";
    //public const string BACKPEDDLE_BALL = "player@backpeddle_dribble";


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
    [SerializeField]
    public Player m_player;
    [SerializeField]
    public Animator m_animator;

    private int m_curAnimHash = -1;
    private int m_newAnimHash;
    private bool m_override = false;
    
    private void Update()
    {
        if (!m_player.clientControlsEnabled || !m_player.IsOwner)
            return;

        if (m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            return;

        m_override = false;
        m_newAnimHash = -1;

        if (m_player.props.isMoving)
        {
            if (!m_player.props.isSprinting)
            {
                if (m_player.props.movingFoward)
                {
                    if (m_player.props.isDribbling)
                        Play(AnimNames.JOG_DRIBBLE);
                    else
                        Play(AnimNames.JOG);
                }
                else if (m_player.props.movingBack)
                {
                    //if (m_player.props.isDribbling)
                        //Play(AnimNames.BACKPEDDLE_BALL);
                    //else
                        //Play(AnimNames.BACKPEDDLE);
                }
                if (m_player.props.movingLeft)
                {
                    if (m_player.props.isDribbling)
                        Play(AnimNames.STRAFE_LEFT);
                    else
                        Play(AnimNames.STRAFE_LEFT);
                }
                else if (m_player.props.movingRight)
                {
                    if (m_player.props.isDribbling)
                        Play(AnimNames.STRAFE_RIGHT);
                    else
                        Play(AnimNames.STRAFE_RIGHT);
                }
            }
            else
            {
                if (m_player.props.isDribbling)
                    Play(AnimNames.SPRINT_DRIBBLE);
                else
                    Play(AnimNames.SPRINT);
            }
        }

        if (m_player.props.isDribbling)
            Play(AnimNames.IDLE_DRIBBLE);
        else if (m_player.HasBall)
            Play(AnimNames.ILDE_TRIPLE_THREAT);
        else
            Play(AnimNames.IDLE);
    }

    public void SetAnimator(Animator animator)
    {
        m_animator = animator;
    }

    public void Play(string animName)
    {
        Play(Animator.StringToHash(animName));
    }
    
    public void Play(int animHash)
    {
        if (m_curAnimHash == animHash)
            return;

        m_override = true;
        m_curAnimHash = animHash;
        m_animator.Play(animHash);
    }

    public Animator GetAnimator()
    {
        return m_animator;
    }
}
