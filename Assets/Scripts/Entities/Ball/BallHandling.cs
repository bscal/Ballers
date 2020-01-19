using UnityEngine;
using MLAPI;
using MLAPI.Spawning;
using System.Collections.Generic;
using MLAPI.Connection;
using System.Linq;
using MLAPI.NetworkedVar;
using System.Collections;

public class BallHandling : NetworkedBehaviour
{
    public GameObject text;

    private NetworkedObject m_playerObj;
    private Player m_player;

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

    private GameManager m_gameManager;
    private NetworkedObject m_netPlayer;
    private NetworkedObject m_netBall;
    private GameObject m_ball;
    private Rigidbody m_body;
    private BallState m_state;

    private bool m_topCollision;

    private Dictionary<ulong, float> m_playerDistances = new Dictionary<ulong, float>();

    public override void NetworkStart()
    {
        m_gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        m_playerObj = SpawnManager.GetLocalPlayerObject();
        m_player = m_playerObj.GetComponent<Player>();

        m_ball = NetworkedObject.gameObject;
        m_body = gameObject.GetComponent<Rigidbody>();
        m_body.AddForce(new Vector3(1, 1, 1), ForceMode.Impulse);
        m_state = BallState.LOOSE;
    }

    void Update()
    {
        Debugger.Instance.Print(string.Format("1:{0} 2:{1} bs:{2}", playerWithBall.Value, playerLastPossesion.Value, m_state), 2);
    }

    // FixedUpdate is called 50x per frame
    void FixedUpdate()
    {
        if (!IsOwner || !IsServer)
            return;

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

        else if (m_state == BallState.SHOT)
        {
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer)
        {
            if (other.gameObject.name == "HitboxTop")
                m_topCollision = true;

            else if (m_topCollision && other.gameObject.name == "HitboxBot")
            {
                OnBasketScored();
                m_gameManager.AddScore(other.GetComponentInParent<Basket>().isHome, 2);
            }
        }
    }

    private void OnBasketScored()
    {
        print("scored");
    }

    private IEnumerator FollowArc(Vector3 start, Vector3 end, float height, float duration)
    {
        float startTime = Time.time;
        float fracComplete = 0;
        while (fracComplete < duration)
        {
            Vector3 center = (start + end) * 0.5F;
            center -= Vector3.up * height;

            Vector3 riseRelCenter = start - center;
            Vector3 setRelCenter = end - center;

            fracComplete = (Time.time - startTime) / duration;

            transform.position = Vector3.Slerp(riseRelCenter, setRelCenter, fracComplete);
            transform.position += center;
            yield return null;
        }
    }

    public void StopBall()
    {
        m_body.velocity = Vector3.zero;
    }

    public void OnShoot(Player player)
    {
        m_state = BallState.SHOT;
        StartCoroutine(FollowArc(m_ball.transform.position, m_gameManager.m_basketLeft.netPos.position, 1.0f, 1.0f));
    }
}

public enum BallState
{
    NONE,
    LOOSE,
    HELD,
    SHOT
}
