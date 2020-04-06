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

    public const ulong NO_PLAYER = ulong.MaxValue;
    public const ulong DUMMY_PLAYER = ulong.MaxValue - 1;

    [SerializeField]
    private const float SHOT_SPEED = 21.0f;

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

    private NetworkedVarByte m_state;
    public BallState State { get { return (BallState) Enum.ToObject(typeof(BallState), m_state.Value); } set { m_state.Value = (byte)value; } }

    private NetworkedVarSByte m_possession;
    public int Possession { get { return m_possession.Value; } set { if (value < -1 || value > 1) value = -1; m_possession.Value = (sbyte)value; } }
    public int PossessionOrHome { get { return (Possession == -1) ? 0 : Possession; } }

    // =================================== Public Varibles ===================================

    public float pass_speed = 6.0f;
    public float walk_offset = 2.5f;
    public float sprint_offset = 6.0f;

    // =================================== Private Varibles ===================================

    private GameManager m_gameManager;
    private NetworkedObject m_playerObj;
    private Player m_currentPlayer;
    private GameObject m_ball;
    private Rigidbody m_body;
    private BallState m_lastState;
    private ShotManager m_shotManager;

    private bool m_ballShot = false;
    private bool m_topCollision;

    private Dictionary<ulong, float> m_playerDistances;
    private IOrderedEnumerable<KeyValuePair<ulong, float>> m_pairs;

    // =================================== Functions ===================================

    void Start()
    {
        StartCoroutine(UpdatePlayerDistances());
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
        m_state = new NetworkedVarByte(settings, 0);
        m_possession = new NetworkedVarSByte(settings, -1);
        m_playerDistances = new Dictionary<ulong, float>();


        GameObject gamemanager = GameObject.Find("GameManager");
        m_gameManager = gamemanager.GetComponent<GameManager>();
        m_shotManager = gamemanager.GetComponent<ShotManager>();
        m_playerObj = SpawnManager.GetLocalPlayerObject();
        m_currentPlayer = m_playerObj.GetComponent<Player>();
        m_ball = NetworkedObject.gameObject;
        m_body = gameObject.GetComponent<Rigidbody>();
        m_body.AddForce(new Vector3(1, 1, 1), ForceMode.Impulse);
        State = BallState.LOOSE;
    }

    void Update()
    {
        Debugger.Instance.Print(string.Format("1:{0} 2:{1} bs:{2}", m_playerWithBall.Value, m_playerLastPossesion.Value, State), 2);

        if (IsServer)
        {
            if (m_ballShot && State != BallState.SHOT)
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

        // ============ Loose ball ============
        if (State == BallState.LOOSE)
        {
            m_body.isKinematic = false;

            foreach (KeyValuePair<ulong, float> pair in m_pairs)
            {
                if (pair.Value < 1.5f)
                {
                    State = BallState.HELD;

                    ChangeBallHandler(pair.Key);
                }
            }
        }
        // ============ ball held ============
        else if (State == BallState.HELD)
        {
            m_currentPlayer = GameManager.GetPlayer(PlayerWithBall);
            if (!m_currentPlayer) return;

            ChangePossession(m_currentPlayer.teamID, false, false);  

            m_body.isKinematic = true;

            if (m_currentPlayer.isBallInLeftHand)
                m_ball.transform.position = m_currentPlayer.GetLeftHand().transform.position;
            else
                m_ball.transform.position = m_currentPlayer.GetRightHand().transform.position;
        }
        // ============ ball shoot ============
        else if (State == BallState.SHOT)
        {
            m_body.isKinematic = false;
            m_body.AddRelativeTorque(Vector3.forward * 10);
            if (m_currentPlayer) ChangeBallHandler(NO_PLAYER);
        }
    }

    // =================================== RPCs ===================================
    [ServerRPC]
    public void OnShoot(ulong pid, float speed, float height, float startOffset, float endOffset)
    {
        PlayerLastTouched = pid;
    }

    [ServerRPC]
    public void OnRelease(ulong pid)
    {
        PlayerLastTouched = pid;
        m_shotManager.OnRelease(pid);
    }

    [ServerRPC]
    public void OnAnimationRelease()
    {
        BallFollowArc();
    }

    // =================================== Public Functions ===================================
    public void StopBall()
    {
        m_body.velocity = Vector3.zero;
    }

    public void BallFollowArc()
    {
        Player player = GameManager.GetPlayer(PlayerLastTouched);

        State = BallState.SHOT;
        m_body.isKinematic = false;

        ShotData shot = ShotManager.Singleton.ShotData.Value;
        float h = ShotController.GetShotRange(shot.type) == ShotRange.LONG ? UnityEngine.Random.Range(1.5f, 3f) : UnityEngine.Random.Range(.3f, .8f);
        float d = shot.distance / (SHOT_SPEED + UnityEngine.Random.Range(0, 1)); 

        StartCoroutine(FollowArc(m_ball.transform.position, m_gameManager.baskets[player.teamID].netPos.position, h, d));
    }

    private IEnumerator FollowArc(Vector3 start, Vector3 end, float height, float duration)
    {
        float startTime = Time.time;
        float fracComplete = 0;
        while (fracComplete < .99f)
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
        State = BallState.LOOSE;
    }
    //TODO
    private IEnumerator FollowBackboard(Vector3 start, Vector3 end, float height, float duration)
    {
        Vector3 bank = GameManager.Singleton.baskets[GameManager.Possession].ban;
        float startTime = Time.time;
        float fracComplete = 0;
        while (fracComplete < .99f)
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
        State = BallState.LOOSE;
    }

    /// Returns the team that does not have possession.
    public int OtherTeam()
    {
        return Possession ^ 1;
    }

    // =================================== Passing ===================================

    public void TryPassBall(Player passer, Player target, PassType type)
    {
        if (!passer.HasBall) return;
        InvokeServerRpc(PassBallServer, passer, target, type);
    }

    public void TryPassBall(ulong passerPid, ulong targetPid, PassType type)
    {

        InvokeServerRpc(PassBallServer, passerPid, targetPid, type);
    }

    [ServerRPC]
    public void PassBallServer(ulong passerPid, ulong targetPid, PassType type)
    {
        State = BallState.PASS;
        Player passer = GameManager.GetPlayer(passerPid);
        Player target = GameManager.GetPlayer(targetPid);
        Vector3 position = GetPassPosition(target, 1);

        InvokeClientRpcOnClient(PassBallClient, targetPid, passerPid, position, type);
        ChangeBallHandler(targetPid);
        StartCoroutine(Pass(passer, target, passerPid, targetPid, position, false, pass_speed));
    }

    [ServerRPC]
    public void PassBallServer(Player passer, Player target, PassType type)
    {
        State = BallState.PASS;
        Vector3 position = GetPassPosition(target, 1);

        ulong passerPid = NO_PLAYER;
        ulong targetPid = NO_PLAYER;

        if (!passer.isDummy)
        {
            passerPid = passer.OwnerClientId;
        }

        if (!target.isDummy)
        {
            targetPid = target.OwnerClientId;
            InvokeClientRpcOnClient(PassBallClient, targetPid, passerPid, position, type);
        }
        ChangeBallHandler(NO_PLAYER);
        StartCoroutine(Pass(passer, target, passerPid, targetPid, position, false, pass_speed));
    }

    [ClientRPC]
    public void PassBallClient(ulong passerPid, Vector3 pos, PassType type)
    {
        Player passer = GameManager.GetPlayer(passerPid);

        StartCoroutine(AutoCatchPass(GameManager.GetPlayer(), pos));
    }

    private IEnumerator Pass(Player passer, Player target, ulong passerPid, ulong targetPid, Vector3 pos, bool halfPos, float speed)
    {
        // Keep a note of the time the movement started.
        float startTime = Time.time;

        // Calculate the journey length.
        float journeyLength = Vector3.Distance(m_ball.transform.position, pos);

        float fractionOfJourney = 0;

        while (fractionOfJourney < 1.0f)
        {
            // Distance moved equals elapsed time times speed..
            float distCovered = (Time.time - startTime) * speed;

            // Fraction of journey completed equals current distance divided by total distance.
            fractionOfJourney = distCovered / journeyLength;

            // Set our position as a fraction of the distance between the markers.
            m_ball.transform.position = Vector3.Lerp(m_ball.transform.position, pos, fractionOfJourney);

            yield return null;
        }

        if (target.isDummy)
        {
            m_currentPlayer = target;
            target.isDribbling = true;
            ChangeBallHandler(DUMMY_PLAYER);
            StartCoroutine(target.GetComponent<PassingDummy>().ThrowPass(passerPid));
        }
        else
        {
            ChangeBallHandler(targetPid);
        }

        State = BallState.HELD;
    }

    private Vector3 GetPassPosition(Player target, int skill)
    {
        Vector3 pos;

        if (target.isSprinting)
            pos = target.CenterPos + target.transform.forward * sprint_offset;
        else if (target.isMoving)
            pos = target.CenterPos + target.transform.forward * walk_offset;
        else
            pos = target.CenterPos + target.transform.forward;

        return pos;
    }

    private IEnumerator AutoCatchPass(Player target, Vector3 pos)
    {
        Vector3 truePos = pos - new Vector3(0, target.height, 0);

        while (State == BallState.PASS)
        {
            float dist = Vector3.Distance(target.CenterPos, pos);
            target.isMovementFrozen = true;
            if (dist > .2)
            {
                target.transform.position = Vector3.Lerp(target.transform.position, truePos, Time.deltaTime * 6.0f);
            }

            yield return null;
        }
        target.isMovementFrozen = false;
    }

    // =================================== End Passing ===================================

    // =================================== Private Functions ===================================

    private IEnumerator UpdatePlayerDistances()
    {
        for (; ; )
        {
            // ============ Lists Closest Players ============
            foreach (KeyValuePair<ulong, NetworkedClient> pair in NetworkingManager.Singleton.ConnectedClients)
            {
                float dist = Vector3.Distance(m_ball.transform.position, pair.Value.PlayerObject.transform.position);
                m_playerDistances[pair.Key] = dist;

                if (pair.Value.PlayerObject.GetComponent<BoxCollider>().bounds.Intersects(m_ball.GetComponentInChildren<SphereCollider>().bounds))
                {
                    PlayerLastPossesion = pair.Key;
                }
            }

            // ============ Sorts Closest Players ============
            m_pairs = from pair in m_playerDistances orderby pair.Value descending select pair;

            yield return new WaitForSeconds(.1f);
        }
    }

    private void SetBallHandler(ulong id)
    {
        PlayerWithBall = id;
        m_currentPlayer = GameManager.GetPlayer(PlayerWithBall);
    }

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

    private void ChangeBallHandler(ulong newPlayer)
    {
        if (newPlayer == DUMMY_PLAYER)
        {
            PlayerWithBall = DUMMY_PLAYER;
            return;
        }

        PlayerLastPossesion = PlayerWithBall;
        if (m_currentPlayer) m_currentPlayer.isDribbling = false;
        PlayerWithBall = newPlayer;
        m_currentPlayer = (newPlayer == NO_PLAYER) ? null : GameManager.GetPlayer(newPlayer);
        if (m_currentPlayer)
        {
            m_currentPlayer.isDribbling = true;
            m_currentPlayer.isBallInLeftHand = !m_currentPlayer.isRightHanded;
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
            State = BallState.INBOUND;

            SetupInbound(team, inboundObj);
        }
        else if (loose)
        {
            State = BallState.LOOSE;
        }


    }

}
