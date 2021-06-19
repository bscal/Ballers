using MLAPI;
using MLAPI.Spawning;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{

    public const float AUTO_TURN_SPEED = 3.0f;

    public Animator animator;
    public PlayerAnimHandler playerAnim;
    public Player m_player;
    public GameObject m_parent;

    public bool isMovementEnabled = true;

    private Controls actions;
    private Vector3 m_targetDirection;

    private readonly float m_movementSpeed  = 6.0f;
    private readonly float m_backpeddleSpeed = 2.5f;
    private readonly float m_strafeSpeed = 5.0f;
    private readonly float m_sprintSpeed    = 12.0f;
    private readonly float m_turningSpeed   = 200.0f;

    private void OnEnable()
    {
        actions = new Controls();
        actions.Enable();
    }

    private void OnDisable()
    {
        actions.Disable();
    }

    void Start()
    {
        if (m_player == null)
            m_player = GetComponentInParent<Player>();
        if (m_parent == null)
            m_parent = m_player.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_player.IsOwner && isMovementEnabled && m_player.clientControlsEnabled)
        {
            HandleTargetDirection();
            MovePlayer();
            RotatePlayer();
        }
    }

    private void HandleTargetDirection()
    {
        int possesion = GameManager.Instance.ballController.PossessionOrHome;
        if (m_player.HasBall && possesion != -1)
        {
            m_targetDirection = GameManager.Instance.baskets[possesion].gameObject.transform.position - m_parent.transform.position;
        }
        else if (m_player.Assignment)
        {
            m_targetDirection = m_player.Assignment.transform.position - m_parent.transform.position;
        }
        else
        {
            m_targetDirection = GameManager.Instance.baskets[possesion].gameObject.transform.position - m_parent.transform.position;
        }
    }

    private void MovePlayer()
    {
        float ms = (m_player.props.isSprinting ? m_sprintSpeed : m_movementSpeed) * Time.deltaTime;
        Vector3 mov = (GameManager.Instance.Possession == 0 ? Vector3.forward : -Vector3.forward) * ms;

        Vector2 move = actions.Keyboard.Move.ReadValue<Vector2>();
        m_player.props.isMoving = move != Vector2.zero;

        bool foward = move.y > 0;    //1
        bool back = move.y < 0;  //-1
        bool left = move.x < 0;  //-1
        bool right = move.x > 0; //1

        // Crossover the ball
        if (left && m_player.props.movingRight && m_player.props.isSprinting)
        {
            print("ball right to left");
        }
        // Crossover the ball
        else if (right && m_player.props.movingLeft && m_player.props.isSprinting)
        {
            print("ball left to right");
        }

        m_player.props.movingFoward = foward; //1
        m_player.props.movingBack = back;     //-1
        m_player.props.movingLeft = left;     //-1
        m_player.props.movingRight = right;   //1

        if (m_player.props.movingFoward)
        {
            m_parent.transform.position += mov;
        }
        else if (m_player.props.movingBack)
        {
            m_parent.transform.position -= mov;
        }

        mov = (GameManager.Instance.Possession == 0 ? Vector3.left : -Vector3.left) * ms;
        if (m_player.props.movingLeft)
        {
            m_parent.transform.position += mov;
        }
        else if (m_player.props.movingRight)
        {
            m_parent.transform.position -= mov;
        }

    }

    private void RotatePlayer()
    {
        if (m_targetDirection != null)
        {
            m_targetDirection.y = 0f;
            // Rotate the forward vector towards the target direction by one step
            Vector3 newDirection = Vector3.RotateTowards(m_parent.transform.forward, m_targetDirection, AUTO_TURN_SPEED * Time.deltaTime, 0.0f);
            // Calculate a rotation a step closer to the target and applies rotation to this object
            m_parent.transform.rotation = Quaternion.LookRotation(newDirection);
        }
    }

    private void HandleDribblingMovement()
    {

    }
}

public enum MoveDir
{
    NONE,
    FOWARD,
    BACK,
    LEFT,
    RIGHT,
}
