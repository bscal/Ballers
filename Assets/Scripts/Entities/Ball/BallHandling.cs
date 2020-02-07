using UnityEngine;
using MLAPI;
using MLAPI.Connection;
using MLAPI.NetworkedVar;
using MLAPI.Messaging;
using MLAPI.Spawning;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum BallState
{
    NONE,
    LOOSE,
    HELD,
    PASS,
    JUMPBALL,
    SHOT
}

public class BallHandling : NetworkedBehaviour
{

    // =================================== Events ===================================

    public event Action<Player> ShotMade;
    public event Action<Player> ShotMissed;
    public event Action<BallState> BallStateChange;
    public event Action<int> BallPossesionChange;

    // =================================== Networking Variables ===================================

    private static readonly NetworkedVarSettings settings = new NetworkedVarSettings()
    {
        SendChannel = "BallChannel", // The var value will be synced over this channel
        ReadPermission = NetworkedVarPermission.Everyone, // The var values will be synced to everyone
        ReadPermissionCallback = null, // Only used when using "Custom" read permission
        SendTickrate = 2, // The var will sync no more than 2 times per second
        WritePermission = NetworkedVarPermission.ServerOnly, // Only the owner of this object is allowed to change the value
        WritePermissionCallback = null // Only used when write permission is "Custom"
    };

    private NetworkedVarULong m_playerWithBall;
    public ulong PlayerWithBall { get { return m_playerWithBall.Value; } set { m_playerWithBall.Value = (value); } }

    private NetworkedVarULong m_playerLastTouched;
    public ulong PlayerLastTouched { get { return m_playerLastTouched.Value; } set { m_playerLastTouched.Value = (value); } }

    private NetworkedVarULong m_playerLastPossesion;
    public ulong PlayerLastPossesion { get { return m_playerLastPossesion.Value; } set { m_playerLastPossesion.Value = (value); } }

    // =================================== Public Varibles ===================================

    public int Possession { get { return (m_state == BallState.JUMPBALL || m_currentPlayer == null) ? -1 : m_currentPlayer.teamID; } }

    // =================================== Private Varibles ===================================

    private GameManager m_gameManager;
    private NetworkedObject m_playerObj;
    private Player m_currentPlayer;
    private GameObject m_ball;
    private Rigidbody m_body;
    private BallState m_state;

    private bool m_ballShot = false;
    private bool m_topCollision;

    private Dictionary<ulong, float> m_playerDistances;

    // =================================== Functions ===================================

    void Start()
    {
    }

    public override void NetworkStart()
    {
        if (!IsServer)
        {
            return;
        }

        m_playerWithBall = new NetworkedVarULong(settings, 5);
        m_playerLastTouched = new NetworkedVarULong(settings, 5);
        m_playerLastPossesion = new NetworkedVarULong(settings, 5);
        m_playerDistances = new Dictionary<ulong, float>();

        m_gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        m_playerObj = SpawnManager.GetLocalPlayerObject();
        m_currentPlayer = m_playerObj.GetComponent<Player>();
        m_ball = NetworkedObject.gameObject;
        m_body = gameObject.GetComponent<Rigidbody>();
        m_body.AddForce(new Vector3(1, 1, 1), ForceMode.Impulse);
        m_state = BallState.LOOSE;
    }

    void Update()
    {
        Debugger.Instance.Print(string.Format("1:{0} 2:{1} bs:{2}", m_playerWithBall.Value, m_playerLastPossesion.Value, m_state), 2);

        if (IsServer)
        {
            if (m_ballShot && m_state != BallState.SHOT)
            {
                ShotMissed(GameManager.GetPlayer(PlayerLastTouched));
            }
        }
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
                PlayerLastPossesion = pair.Key;
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

                    PlayerWithBall = pair.Key;

                    // Stops old player from dribbling
                    m_currentPlayer.isDribbling = false;

                    m_currentPlayer = GameManager.GetPlayer(PlayerWithBall);

                    // Sets new player to dribble
                    m_currentPlayer.isDribbling = true;
                }
            }
        }

        else if (m_state == BallState.HELD)
        {
            m_currentPlayer = GameManager.GetPlayer(PlayerWithBall);
            if (m_currentPlayer.IsBallInLeftHand)
                m_ball.transform.position = m_currentPlayer.GetLeftHand().transform.position;
            else
                m_ball.transform.position = m_currentPlayer.GetRightHand().transform.position;
        }

        else if (m_state == BallState.SHOT)
        {
        }
    }

    // =================================== RPCs ===================================
    [ServerRPC]
    private void OnShoot(ulong pid)
    {
        Player player = GameManager.GetPlayer(pid);
        PlayerLastTouched = pid;
        m_state = BallState.SHOT;
        StartCoroutine(FollowArc(m_ball.transform.position, m_gameManager.baskets[player.teamID].netPos.position, 1.0f, 1.0f));
    }

    [ServerRPC]
    private void OnRelease(ulong pid)
    {
        Player player = GameManager.GetPlayer(pid);
        PlayerLastTouched = pid;
        m_state = BallState.SHOT;
        StartCoroutine(FollowArc(m_ball.transform.position, m_gameManager.baskets[player.teamID].netPos.position, 1.0f, 1.0f));
    }

    // =================================== Public Functions ===================================
    public void StopBall()
    {
        m_body.velocity = Vector3.zero;
    }

    public void ShootBall(ulong pid)
    {
        InvokeServerRpc(OnShoot, pid);
    }

    private void SetBallHandler(ulong id)
    {
        PlayerWithBall = id;
        m_currentPlayer = GameManager.GetPlayer(PlayerWithBall);
    }

    public void RegisterPlayers()
    {
        GameManager.GetPlayers().ForEach((p) => {

        });
    }

    // =================================== Private Functions ===================================

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer)
        {
            if (other.gameObject.name == "Hitbox Top")
                m_topCollision = true;


            if (m_topCollision && other.gameObject.name == "Hitbox Bot")
            {
                ShotMade?.Invoke(GameManager.GetPlayer(PlayerLastTouched));
                OnBasketScored();
                m_gameManager.AddScore(other.GetComponentInParent<Basket>().id, 2);
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
        m_state = BallState.LOOSE;
    }

}
