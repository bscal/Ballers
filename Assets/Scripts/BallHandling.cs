using UnityEngine;
using TMPro;
using MLAPI;
using MLAPI.Spawning;

public class BallHandling : NetworkedBehaviour
{
    public GameObject text;

    private NetworkedObject m_playerObj;
    private Player m_player;
    private GameObject m_handAnimPoint;
    public GameObject m_left;
    public GameObject m_right;

    private GameObject m_ball;
    private Rigidbody m_body;
    private BallState m_state;

    // Start is called before the first frame update
    public override void NetworkStart()
    {
        m_playerObj = SpawnManager.GetLocalPlayerObject();
        m_player = m_playerObj.GetComponent<Player>();
        m_handAnimPoint = GameObject.Find("HandLAnimPos");
        m_left = GameObject.Find("HandL");
        m_right = GameObject.Find("HandR");

        m_ball = gameObject;
        m_body = gameObject.GetComponent<Rigidbody>();

        m_state = BallState.LOOSE;
    }

    // Update is called once per frame
    void Update()
    {
        m_body.AddExplosionForce(5.0f, new Vector3(0,.5f,-1), 10);
        Debugger.Instance.Print(string.Format("Ball: S:{0}, D:{1}", m_state.ToString(), Vector3.Distance(m_ball.transform.position, m_playerObj.transform.position)), 2);
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
    }
}

public enum BallState
{
    NONE,
    LOOSE,
    HELD,
    SHOT
}
