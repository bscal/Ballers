using MLAPI;
using MLAPI.Spawning;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private float m_horizontal;
    private float m_vertical;

    private Player m_player;

    private readonly float m_movementSpeed  = 8.0f;
    private readonly float m_sprintSpeed    = 16.0f;
    private readonly float m_turningSpeed   = 200.0f;

    void Start()
    {
        if (NetworkingManager.Singleton.IsServer && !NetworkingManager.Singleton.IsHost)
        {
            Destroy(this);
        }

        m_player = GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_player.isMoving)
        {
            m_horizontal = Input.GetAxis("Horizontal") * m_turningSpeed * Time.deltaTime;
            transform.Rotate(0, m_horizontal, 0);

            m_vertical = Input.GetAxis("Vertical") * (m_player.isSprinting ? m_sprintSpeed : m_movementSpeed) * Time.deltaTime;
            transform.Translate(0, 0, -m_vertical);
        }
        else
        {
            Quaternion rotation = Quaternion.Lerp(
                                    transform.rotation, 
                                    Quaternion.LookRotation(GameManager.Singleton.baskets[GameManager.GetBallHandling().Possession].transform.position),
                                    Time.deltaTime);

            transform.rotation = rotation;
        }


    }

}
