using UnityEngine;
using TMPro;

public class BallHandling : MonoBehaviour
{
    public GameObject text;
    public TextMeshProUGUI mesh;

    float speed = 1.0f;
    float dibble_speed = 4.0f;

    public GameObject PlayerHand { get; set; }
    public float FloorLevel { get; set; } = 0.5f;


    private GameObject m_ball;
    private BallState m_state;
    private float m_startTime;
    private float m_length;
    private bool m_dribbled;
    private Vector3 m_floor;

    // Start is called before the first frame update
    void Start()
    {
        PlayerHand = GameObject.Find("Hand");
        mesh = text.GetComponent<TextMeshProUGUI>();
        m_ball = gameObject;
        m_state = BallState.LOOSE;
    }

    private void Update()
    {
        mesh.text = "Ball State: " + m_state.ToString();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        print(m_state);

        if (m_state == BallState.LOOSE)
        {
            m_length = Vector3.Distance(m_ball.transform.position, PlayerHand.transform.position);
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
            m_ball.transform.position = Vector3.Lerp(m_ball.transform.position, PlayerHand.transform.position, fractionOfJourney);

            if (fractionOfJourney > .3f)
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
                m_floor = m_ball.transform.position;
                m_floor.y = FloorLevel;
            }
            else
            {
                // Distance moved equals elapsed time times speed..
                float distCovered = (Time.time - m_startTime) * dibble_speed;

                // Fraction of journey completed equals current distance divided by total distance.
                float fractionOfJourney = distCovered / m_length;

                // Moves position
                m_ball.transform.position = Vector3.Lerp(m_ball.transform.position, m_floor, fractionOfJourney);

                if (fractionOfJourney > .3f)
                {
                    m_length = Vector3.Distance(m_ball.transform.position, PlayerHand.transform.position);
                    m_startTime = Time.time;
                    m_dribbled = false;
                    m_state = BallState.TO_HAND;
                }
            }
        }
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
