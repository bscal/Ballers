using UnityEngine;
using TMPro;

public class BallHandling : MonoBehaviour
{
    public GameObject text;
    public TextMeshProUGUI mesh;

    float speed = 1.0f;
    float dibble_speed = 4.0f;

    public GameObject BallHand { get; set; }
    public float FloorLevel { get; set; } = 0.5f;

    private GameObject m_playerObj;
    private Player m_player;
    private Movement m_movement;
    private GameObject m_handAnimPoint;
    public GameObject m_left;
    public GameObject m_right;

    private GameObject m_ball;
    private Rigidbody m_body;
    private BallState m_state;

    private float m_startTime;
    private float m_length;
    private bool m_dribbled;
    private Vector3 m_floor;

    // Start is called before the first frame update
    void Start()
    {
        m_playerObj = GameObject.Find("PlayerObject");
        m_player = m_playerObj.GetComponent<Player>();
        m_movement = m_playerObj.GetComponent<Movement>();
        m_handAnimPoint = GameObject.Find("HandLAnimPos");
        m_left = GameObject.Find("HandL");
        m_right = GameObject.Find("HandR");

        m_ball = gameObject;
        m_body = gameObject.GetComponent<Rigidbody>();

        mesh = text.GetComponent<TextMeshProUGUI>();

        m_state = BallState.LOOSE;
    }

    // Update is called once per frame
    void Update()
    {
        mesh.text = "Ball State: " + m_state.ToString() + " , " + Vector3.Distance(m_ball.transform.position, m_playerObj.transform.position);
        m_body.AddExplosionForce(5.0f, new Vector3(0,.5f,-1), 10);
        //m_ball.transform.position = m_handAnimPoint.transform.position;
    }

    // FixedUpdate is called 50x per frame
    void FixedUpdate()
    {

        if (m_state == BallState.LOOSE)
        {
            float length = Vector3.Distance(m_ball.transform.position, m_playerObj.transform.position);

            if (length < 1.5f)
            {
                m_state = BallState.HELD;
                m_player.IsDribbling = true;
            }
        }

        else if (m_state == BallState.HELD)
        {
            m_ball.transform.position = m_handAnimPoint.transform.position;
        }

        /*        if (m_state == BallState.LOOSE)
                {
                    m_length = Vector3.Distance(m_ball.transform.position, BallHand.transform.position);
                    if (m_length < 2.0f)
                    {
                        // Keep a note of the time the movement started.
                        m_startTime = Time.time;

                        m_state = BallState.TO_HAND;
                    }
                }
                else if (m_state == BallState.TO_HAND)
                {
                    // Distance moved equals elapsed time times speed..
                    float distCovered = (Time.time - m_startTime) * speed;

                    // Fraction of journey completed equals current distance divided by total distance.
                    float fractionOfJourney = distCovered / m_length;

                    // Moves position
                    m_ball.transform.position = Vector3.Lerp(m_ball.transform.position, BallHand.transform.position, 0.5f);

                    if (Vector3.Distance(m_ball.transform.position, BallHand.transform.position) < 0.2f)
                    {
                        m_state = BallState.DRIBBLE;
                    }
                }
                else if (m_state == BallState.DRIBBLE)
                {
                    if (!m_dribbled)
                    {
                        m_dribbled = true;
                        m_startTime = Time.time;
                        m_length = Vector3.Distance(m_ball.transform.position, m_floor);

                        m_ball.transform.rotation = m_player.transform.rotation;
                        m_floor = BallHand.transform.TransformPoint(-Vector3.forward * 2);
                        m_floor.y = FloorLevel;
                    }
                    else
                    {
                        // Distance moved equals elapsed time times speed..
                        float distCovered = (Time.time - m_startTime) * dibble_speed;

                        // Fraction of journey completed equals current distance divided by total distance.
                        float fractionOfJourney = distCovered / m_length;

                        // Moves position
                        m_ball.transform.position = Vector3.Lerp(m_ball.transform.position, m_floor, 0.1f);

                        if (Vector3.Distance(m_ball.transform.position, m_floor) < 0.2f)
                        {
                            m_length = Vector3.Distance(m_ball.transform.position, BallHand.transform.position);
                            m_startTime = Time.time;
                            m_dribbled = false;
                            m_state = BallState.TO_HAND;
                        }
                    }
                }*/
    }
}

public enum BallState
{
    NONE,
    LOOSE,
    HELD,
    SHOT
}
