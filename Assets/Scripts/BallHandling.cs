using UnityEngine;
using MLAPI;
using MLAPI.Spawning;
using System.Collections.Generic;
using MLAPI.Connection;
using System.Linq;
using MLAPI.NetworkedVar;

public class BallHandling : NetworkedBehaviour
{
    public GameObject text;

    private NetworkedObject m_playerObj;
    private Player m_player;
    private GameObject m_handAnimPoint;
    public GameObject m_left;
    public GameObject m_right;

    private static readonly NetworkedVarSettings settings = new NetworkedVarSettings()
    {
        SendChannel = "BallChannel", // The var value will be synced over this channel
        ReadPermission = NetworkedVarPermission.Everyone, // The var values will be synced to everyone
        ReadPermissionCallback = null, // Only used when using "Custom" read permission
        SendTickrate = 2, // The var will sync no more than 2 times per second
        WritePermission = NetworkedVarPermission.ServerOnly, // Only the owner of this object is allowed to change the value
        WritePermissionCallback = null // Only used when write permission is "Custom"
    };

    private NetworkedVarULong playerWithBall = new NetworkedVarULong(settings, 5);
    private NetworkedVarULong playerLastTouched = new NetworkedVarULong(settings, 5);
    private NetworkedVarULong playerLastPossesion = new NetworkedVarULong(settings, 5);

    private NetworkedObject m_netPlayer;
    private NetworkedObject m_netBall;
    private GameObject m_ball;
    private Rigidbody m_body;
    private BallState m_state;

    private Dictionary<ulong, float> m_playerDistances = new Dictionary<ulong, float>();

    //private NetworkedObject m_playerWithBall;
    //private NetworkedObject m_playerLastTouched;
    //private NetworkedObject m_playerLastPossesion;

    // Start is called before the first frame update
    public override void NetworkStart()
    {
        if (IsServer)
        {
        }

        m_playerObj = SpawnManager.GetLocalPlayerObject();
        m_player = m_playerObj.GetComponent<Player>();
        m_handAnimPoint = GameObject.Find("HandLAnimPos");
        m_left = GameObject.Find("HandL");
        m_right = GameObject.Find("HandR");

        m_ball = NetworkedObject.gameObject;
        m_body = gameObject.GetComponent<Rigidbody>();
        m_body.AddForce(new Vector3(1, 1, 1), ForceMode.Impulse);
        m_state = BallState.LOOSE;
       // m_body.AddExplosionForce(5.0f, new Vector3(0, .5f, -1), 10);
    }

    // Update is called once per frame
    void Update()
    {
        //m_body.AddExplosionForce(5.0f, new Vector3(0,.5f,-1), 10);
        Debugger.Instance.Print(string.Format("1:{0} 2:{1} bs:{2}", playerWithBall.Value, playerLastPossesion.Value, m_state), 2);
    }

    // FixedUpdate is called 50x per frame
    void FixedUpdate()
    {
        foreach (KeyValuePair<ulong, NetworkedClient> pair in NetworkingManager.Singleton.ConnectedClients)
        {
            float dist = Vector3.Distance(m_ball.transform.position, pair.Value.PlayerObject.transform.position);
            m_playerDistances[pair.Key] = dist;

            if (pair.Value.PlayerObject.GetComponent<BoxCollider>().bounds.Intersects(m_ball.GetComponentInChildren<SphereCollider>().bounds))
            {
                playerLastPossesion.Value = pair.Key;
            }
        }

        if (m_state == BallState.LOOSE)
        {
            var pairs = from pair in m_playerDistances orderby pair.Value descending select pair;

            foreach (KeyValuePair<ulong, float> pair in pairs)
            {
                if (pair.Value < 1.5f)
                {
                    m_state = BallState.HELD;

                    playerWithBall.Value = pair.Key;

                    // Stops old player from dribbling
                    m_player.isDribbling = false;

                    m_player = SpawnManager.GetPlayerObject(playerWithBall.Value).gameObject.GetComponent<Player>();

                    // Sets new player to dribble
                    m_player.isDribbling = true;
                }
            }
        }

        else if (m_state == BallState.HELD)
        {
            m_ball.transform.position = m_player.rightHand.Value;
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
