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
    SHOT,
    INBOUND,
    DEAD_BALL
}

public enum PassType
{
    CHESS,
    BOUNCE,
    LOP,
    FLASHY,
    ALLEY_OOP
}

public class BallHandling : NetworkedBehaviour
{
    // =================================== Constants ===================================

    private const ulong NO_PLAYER = ulong.MaxValue;
    private const ulong DUMMY_PLAYER = ulong.MaxValue - 1;

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

    private NetworkedVarSByte m_possession;
    public int Possession { get { return m_possession.Value; } set { if (value < -1 || value > 1) value = -1; m_possession.Value = (sbyte)value; } }
    public int PossessionOrHome { get { return (Possession == -1) ? 0 : Possession; } }

    // =================================== Public Varibles ===================================



    //get { return (m_state == BallState.JUMPBALL || m_currentPlayer == null) ? -1 : m_currentPlayer.teamID; } }



    // =================================== Private Varibles ===================================

    private GameManager m_gameManager;
    private NetworkedObject m_playerObj;
    private Player m_currentPlayer;
    private GameObject m_ball;
    private Rigidbody m_body;
    private BallState m_state;
    private BallState m_lastState;

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

        m_playerWithBall = new NetworkedVarULong(settings, 0);
        m_playerLastTouched = new NetworkedVarULong(settings, 0);
        m_playerLastPossesion = new NetworkedVarULong(settings, 0);
        m_possession = new NetworkedVarSByte(settings, -1);
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
            
            m_body.isKinematic = false;
            var pairs = from pair in m_playerDistances orderby pair.Value descending select pair;

            foreach (KeyValuePair<ulong, float> pair in pairs)
            {
                if (pair.Value < 1.5f)
                {
                    m_state = BallState.HELD;

                    ChangeBallHandler(pair.Key);
                }
            }
        }

        else if (m_state == BallState.HELD)
        {
            m_currentPlayer = GameManager.GetPlayer(PlayerWithBall);
            ChangePossession(m_currentPlayer.teamID, false, false);
            if (!m_currentPlayer) return;
            m_body.isKinematic = true;
            if (m_currentPlayer.IsBallInLeftHand)
                m_ball.transform.position = m_currentPlayer.GetLeftHand().transform.position;
            else
                m_ball.transform.position = m_currentPlayer.GetRightHand().transform.position;
        }

        else if (m_state == BallState.SHOT)
        {
            m_body.isKinematic = false;
            if (m_currentPlayer) ChangeBallHandler(NO_PLAYER);
        }
    }

    // =================================== RPCs ===================================
    [ServerRPC]
    public void OnShoot(ulong pid)
    {
        Player player = GameManager.GetPlayer(pid);
        PlayerLastTouched = pid;
    }

    [ServerRPC]
    public void OnRelease(ulong pid)
    {
        m_state = BallState.SHOT;
        Player player = GameManager.GetPlayer(pid);
        PlayerLastTouched = pid;
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

    public void BallFollowArc()
    {
        Player player = GameManager.GetPlayer(PlayerLastTouched);
        StartCoroutine(FollowArc(m_ball.transform.position, m_gameManager.baskets[player.teamID].netPos.position, 1.0f, 1.0f));
    }

    /// Returns the team that does not have possession.
    public int OtherTeam()
    {
        return Possession ^ 1;
    }

    public void TryPassBall(Player passer, Player target, PassType type)
    {
        if (!passer.HasBall) return;
        print("passing");
        InvokeServerRpc(PassBallServer, passer, target, type);
    }

    public void TryPassBall(ulong passerPid, ulong targetPid, PassType type)
    {

        InvokeServerRpc(PassBallServer, passerPid, targetPid, type);
    }

    [ServerRPC]
    public void PassBallServer(ulong passerPid, ulong targetPid, PassType type)
    {
        Player passer = GameManager.GetPlayer(passerPid);
        Player target = GameManager.GetPlayer(targetPid);
        Vector3 position = target.transform.position;

        InvokeClientRpcOnClient(PassBallClient, targetPid, passerPid, position, type);

        StartCoroutine(Pass(passer, target, passerPid, targetPid, position, false, 2.0f));
    }

    [ServerRPC]
    public void PassBallServer(Player passer, Player target, PassType type)
    {
        print("passing2");
        Vector3 position = target.transform.position;

        ulong passerPid = NO_PLAYER;
        ulong targetPid = NO_PLAYER;

        if (!passer.isDummy)
        {
            passerPid = passer.OwnerClientId;
        }

        if (!target.isDummy)
        {
            targetPid = target.OwnerClientId;
            InvokeClientRpcOnClient(PassBallClient, target.OwnerClientId, NO_PLAYER, position, type);
        }

        StartCoroutine(Pass(passer, target, passerPid, targetPid, position, false, 2.0f));
    }

    [ClientRPC]
    public void PassBallClient(ulong passerPid, Vector3 pos, PassType type)
    {
        Player passer = GameManager.GetPlayer(passerPid);
        print("passing4");
    }

    private IEnumerator Pass(Player passer, Player target, ulong passerPid, ulong targetPid, Vector3 pos, bool halfPos, float speed)
    {
        print("passing3");
        // Keep a note of the time the movement started.
        float startTime = Time.time;

        // Calculate the journey length.
        float journeyLength = Vector3.Distance(m_ball.transform.position, pos);

        float fractionOfJourney = 0;

        m_state = BallState.PASS;
        while (fractionOfJourney < journeyLength)
        {
            // Distance moved equals elapsed time times speed..
            float distCovered = (Time.time - startTime) * speed;

            // Fraction of journey completed equals current distance divided by total distance.
            fractionOfJourney = distCovered / journeyLength;

            // Set our position as a fraction of the distance between the markers.
            transform.position = Vector3.Lerp(m_ball.transform.position, pos, Time.time * speed);

            yield return null;
        }

        Player p = (Player)target;
        m_state = BallState.HELD;

        if (p.isDummy)
        {
            m_currentPlayer = p;
            StartCoroutine(p.GetComponent<PassingDummy>().ThrowPass(passerPid));
        }

        else
            ChangeBallHandler(p.NetworkId);
        

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

    private void ChangeBallHandler(ulong newPlayer)
    {
        PlayerLastPossesion = PlayerWithBall;
        if (m_currentPlayer) m_currentPlayer.isDribbling = false;
        PlayerWithBall = newPlayer;
        m_currentPlayer = (newPlayer == NO_PLAYER) ? null : GameManager.GetPlayer(newPlayer);
        if (m_currentPlayer)
        {
            m_currentPlayer.isDribbling = true;
            m_currentPlayer.IsBallInLeftHand = !m_currentPlayer.isRightHanded;
            if (Possession != m_currentPlayer.teamID) Possession = m_currentPlayer.teamID;
        }
    }

    private void SetupInbound(int team, GameObject inbound)
    {
        // Fade Screen

        // Move players/ball

        // Setup Inbound Coroutine
    }

    public void ChangePossession(int team, bool inbound, bool loose)
    {
        GameObject inboundObj = GameManager.Singleton.GetClosestInbound(m_ball.transform.position);

        //ChangeBallHandler(NO_PLAYER);

        Possession = team;

        if (inbound)
        {
            m_state = BallState.INBOUND;

            SetupInbound(team, inboundObj);
        }
        else if (loose)
        {
            m_state = BallState.LOOSE;
        }


    }

}
