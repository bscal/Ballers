using MLAPI;
using MLAPI.Spawning;
using UnityEngine;

public class Movement : MonoBehaviour
{

    public const float AUTO_TURN_SPEED = 3.0f;

    private float m_horizontal;
    private float m_vertical;

    private Player m_player;
    private GameObject m_parent;

    private readonly float m_movementSpeed  = 8.0f;
    private readonly float m_sprintSpeed    = 16.0f;
    private readonly float m_turningSpeed   = 200.0f;

    void Start()
    {
        if (NetworkingManager.Singleton.IsServer && !NetworkingManager.Singleton.IsHost)
        {
            Destroy(this);
        }

        m_player = GetComponentInParent<Player>();
        m_parent = m_player.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_player.isMoving)
        {
            m_horizontal = Input.GetAxis("Horizontal") * m_turningSpeed * Time.deltaTime;
            m_parent.transform.Rotate(0, m_horizontal, 0);

            m_vertical = Input.GetAxis("Vertical") * (m_player.isSprinting ? m_sprintSpeed : m_movementSpeed) * Time.deltaTime;
            m_parent.transform.Translate(0, 0, m_vertical);
        }
        else
        {
            // Determine which direction to rotate towards
            Vector3 targetDirection = GameManager.Singleton.baskets[GameManager.GetBallHandling().Possession].gameObject.transform.position - m_parent.transform.position;

            // Rotate the forward vector towards the target direction by one step
            Vector3 newDirection = Vector3.RotateTowards(m_parent.transform.forward, targetDirection, AUTO_TURN_SPEED * Time.deltaTime, 0.0f);

            // Calculate a rotation a step closer to the target and applies rotation to this object
            m_parent.transform.rotation = Quaternion.LookRotation(newDirection);

        }
    }

}
