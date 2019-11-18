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

    private GameObject m_player;
    private Movement m_movement;
    private GameObject m_handAnimPoint;
    private GameObject m_ball;
    private BallState m_state;

    public GameObject m_left;
    public GameObject m_right;
    public bool isRightHanded = true;

    private float m_startTime;
    private float m_length;
    private bool m_dribbled;
    private Vector3 m_floor;

    // Start is called before the first frame update
    void Start()
    {
        m_player = GameObject.Find("PlayerObject");
        m_movement = m_player.GetComponent<Movement>();
        m_handAnimPoint = GameObject.Find("HandLAnimPos");

        m_left = GameObject.Find("HandL");
        m_right = GameObject.Find("HandR");
        BallHand = m_left;
        mesh = text.GetComponent<TextMeshProUGUI>();
        m_ball = gameObject;
        m_state = BallState.LOOSE;
    }

    // Update is called once per frame
    private void Update()
    {
        mesh.text = "Ball State: " + m_state.ToString();
        m_ball.transform.position = m_handAnimPoint.transform.position;
    }

    // FixedUpdate is called 50x per frame
    void FixedUpdate()
    {

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
    TO_HAND,
    DRIBBLE,
    HELD,
    SHOOT
}
