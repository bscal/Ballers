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

    // PlayerID with ball
    private readonly NetworkedVarULong m_playerWithBall = new NetworkedVarULong(NetworkConstants.BALL_CHANNEL, NO_PLAYER);
    public ulong PlayerWithBall { get { return m_playerWithBall.Value; } set { m_playerWithBall.Value = (value); } }

    // PlayerID last touched ball
    private readonly NetworkedVarULong m_playerLastTouched = new NetworkedVarULong(NetworkConstants.BALL_CHANNEL, NO_PLAYER);
    public ulong PlayerLastTouched { get { return m_playerLastTouched.Value; } set { m_playerLastTouched.Value = (value); } }

    // PlayerID last possession
    private readonly NetworkedVarULong m_playerLastPossesion = new NetworkedVarULong(NetworkConstants.BALL_CHANNEL, NO_PLAYER);
    public ulong PlayerLastPossesion { get { return m_playerLastPossesion.Value; } set { m_playerLastPossesion.Value = (value); } }

    // BallState
    private readonly NetworkedVarByte m_state = new NetworkedVarByte(NetworkConstants.BALL_CHANNEL, 0);
    public BallState State { get { return (BallState) Enum.ToObject(typeof(BallState), m_state.Value); } set { m_state.Value = (byte)value; } }

    // TeamID with possession
    private readonly NetworkedVarSByte m_possession = new NetworkedVarSByte(NetworkConstants.BALL_CHANNEL, -1);
    public int Possession { get { return m_possession.Value; } set { if (value < -1 || value > 1) value = -1; m_possession.Value = (sbyte)value; } }
    public int PossessionOrHome { get { return (Possession != 1) ? 0 : 1; } }

    // =================================== Public Varibles ===================================

    public float pass_speed = 6.0f;
    public float walk_offset = 2.5f;
    public float sprint_offset = 6.0f;

    // =================================== Private Varibles ===================================

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

    private void Awake()
    {
        GameManager.Singleton.PlayerLoaded += OnPlayerLoaded;
        GameManager.Singleton.GameStarted += OnGameStarted;
    }

    void Start()
    {

    }

    public override void NetworkStart()
    {

        if (!IsServer)
        {
            return;
        }
        m_playerDistances = new Dictionary<ulong, float>();

        m_shotManager = GameManager.Singleton.gameObject.GetComponent<ShotManager>();
        m_ball = NetworkedObject.gameObject;

        m_body = gameObject.GetComponent<Rigidbody>();
        m_body.AddForce(new Vector3(1, 1, 1), ForceMode.Impulse);
    }

    void Update()
    {
        Debugger.Instance.Print(string.Format("1:{0} 2:{1} bs:{2}", m_playerWithBall.Value, m_playerLastPossesion.Value, State), 2);

        if (IsServer)
        {
            if (m_ballShot && State != BallState.SHOT)
            {
                ShotMissed(GameManager.GetPlayerByNetworkID(PlayerLastTouched));
            }
        }

        
    }

    // FixedUpdate is called 50x per frame
    void FixedUpdate()
    {
        if (!IsServer) return;
        if (!Match.HasGameStarted) return;

        // ============ Loose ball ============
        if (State == BallState.LOOSE)
        {
            m_body.isKinematic = false;
            if (m_pairs == null) {
                Debug.LogWarning("loose ball::pairs::is null");
                return;
            }
            foreach (KeyValuePair<ulong, float> pair in m_pairs)
            {
                if (pair.Value < 1.5f)
                {
                    State = BallState.HELD;
                    ChangeBallHandler(pair.Key);
                    Debug.Log(pair.Key + " picked up ball");
                }
            }
        }

        // ============ ball held ============
        else if (State == BallState.HELD)
        {
//             m_currentPlayer = GameManager.GetPlayer(PlayerWithBall);
//             if (!m_currentPlayer) return;
//             if (m_currentPlayer.OwnerClientId != PlayerWithBall)
//                 ChangePossession(m_currentPlayer.teamID, false, false);  

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

    public void OnPlayerLoaded(Player p)
    {
    }

    public void OnGameStarted()
    {
/*        m_playerObj = SpawnManager.GetLocalPlayerObject();*/
/*        m_currentPlayer = m_playerObj.GetComponent<Player>();*/
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (IsServer)
        {
            StartCoroutine(UpdatePlayerDistances());
            StopBall();
            gameObject.transform.position = new Vector3(5, 3, 5);
            State = BallState.LOOSE;
        }


    }

    // =================================== RPCs ===================================
    [ServerRPC]
    public void OnShoot(ulong netID, ShotBarData shotBarData, float endOffset)
    {
        PlayerLastTouched = netID;
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
        
    }

    // =================================== Public Functions ===================================
    public void StopBall()
    {
        m_body.velocity = Vector3.zero;
    }

    public void BallFollowArc(ulong netID)
    {
        Player player = GameManager.GetPlayerByNetworkID(netID);
        print($"{player} | BALLFOLLOWARC | {PlayerLastTouched} {netID}");


        State = BallState.SHOT;
        m_body.isKinematic = false;

        ShotData shot = ShotManager.Singleton.GetShotData();
        print($"{shot} | shotdata");
        float h = ShotController.GetShotRange(shot.type) == ShotRange.LONG ? UnityEngine.Random.Range(1.5f, 3f) : UnityEngine.Random.Range(.3f, .8f);
        float d = shot.distance / (SHOT_SPEED + UnityEngine.Random.Range(0, 1)); 

        if (shot.bankshot == BankType.NONE)
            StartCoroutine(FollowArc(m_ball.transform.position, GameManager.Singleton.baskets[player.TeamID].netPos.position, h, d));
        else
            StartCoroutine(FollowBackboard(shot, m_ball.transform.position, GameManager.Singleton.baskets[player.TeamID].netPos.position, h, d));

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
    private IEnumerator FollowBackboard(ShotData shot, Vector3 start, Vector3 end, float height, float duration)
    {
        Vector3 bankPos = GameManager.Singleton.baskets[GameManager.Singleton.Possession].banks[(int)shot.bankshot].transform.position;
        float startTime = Time.time;
        float fracComplete;

        // This is the loop for slerping the ball position from the player to the bank position
        // This statement will break itself at 99% completion of journey
        for (;;)
        {
            // Sets the center
            Vector3 center = (start + bankPos) * 0.5f;
            // Adjusts the height of the center point based on shot type
            center -=  (shot.type == ShotType.SHOT) ? Vector3.up * height : Vector3.up;

            // Gets the points to Slerp with based on center
            Vector3 riseRelCenter = start - center;
            Vector3 setRelCenter = bankPos - center;

            fracComplete = (Time.time - startTime) / duration;

            // Breaks loop if gotten to destination
            if (fracComplete > .99)
                break;

            transform.position = Vector3.Slerp(riseRelCenter, setRelCenter, fracComplete);
            transform.position += center;
            yield return null;
        }

        //Resets values
        startTime = Time.time;
        fracComplete = 0;
        // This is the loop for lerping ball position from bank spot on backboard to basket.
        while (fracComplete < .99)
        {
            // Using duration here does not makes sense since its a constant distance between the bank and the
            // basket. Think about moveing this to a static const or some better way?
            fracComplete = (Time.time - startTime) / (duration * .75f);

            transform.position = Vector3.Lerp(bankPos, end, fracComplete);
            yield return null;
        }

        State = BallState.LOOSE;
    }

    /// Returns the team that does not have possession.
    public int FlipTeam()
    {
        return Mathf.Clamp(Possession ^ 1, 0, 1);
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
        Player passer = GameManager.GetPlayerByNetworkID(passerPid);
        Player target = GameManager.GetPlayerByNetworkID(targetPid);
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
        Player passer = GameManager.GetPlayerByNetworkID(passerPid);

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
        while (true)
        {
            if (!Match.HasGameStarted)
            {
                yield return new WaitForSeconds(.33f);
                continue;
            }
            // ============ Lists Closest Players ============
            foreach (Player player in GameManager.GetPlayers())
            {
                if (player == null) continue;
                float dist = Vector3.Distance(m_ball.transform.position, player.transform.position);

                m_playerDistances[player.NetworkId] = dist;

                if (player.GetComponent<BoxCollider>().bounds.Intersects(m_ball.GetComponentInChildren<SphereCollider>().bounds))
                {
                    PlayerLastPossesion = player.NetworkId;
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
        m_currentPlayer = GameManager.GetPlayerByNetworkID(PlayerWithBall);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer)
        {
            if (other.gameObject.name == "Hitbox Top")
                m_topCollision = true;


            if (m_topCollision && other.gameObject.name == "Hitbox Bot")
            {
                ShotMade?.Invoke(GameManager.GetPlayerByNetworkID(PlayerLastTouched));
                OnBasketScored();
                GameManager.Singleton.AddScore(other.GetComponentInParent<Basket>().id, 2);
            }
        }
    }

    private void OnBasketScored()
    {
        print("scored");
    }

    [ClientRPC]
    public void SetPlayerHandler(ulong netID, bool isHandler)
    {
        Player passer = GameManager.GetPlayerByNetworkID(netID);
        passer.isDribbling = isHandler;

        if (isHandler)
        {
            m_currentPlayer.isBallInLeftHand = !m_currentPlayer.isRightHanded;
        }
    }

    /// <summary>
    /// Changes the Player with ball and updates clients that have the ball.<br></br>
    /// If NO_PLAYER id is set sets State to LOOSE.<br></br>
    /// Possession is also changed to correct possession if needed.
    /// </summary>
    private void ChangeBallHandler(ulong newNetworkID)
    {
        if (newNetworkID == PlayerWithBall) return;

        PlayerLastPossesion = PlayerWithBall;
        PlayerWithBall = newNetworkID;

        if (newNetworkID == DUMMY_PLAYER) return;

        // Alerts the last player with possession if not null hes not holding the ball.
        if (m_currentPlayer != null)
            InvokeClientRpcOnClient(SetPlayerHandler, m_currentPlayer.OwnerClientId, m_currentPlayer.NetworkId, false);

        int teamToSwitch = (int)TeamType.NONE;
        // If newPlayer is set to NO_PLAYER id the ball should be loose.
        // There is no need to update that client or change possession until ball is picked up.
        if (newNetworkID == NO_PLAYER)
        {
            State = BallState.LOOSE;
        }
        else
        {
            m_currentPlayer = GameManager.GetPlayerByNetworkID(newNetworkID);
            InvokeClientRpcOnClient(SetPlayerHandler, m_currentPlayer.OwnerClientId, m_currentPlayer.NetworkId, true);
            // if old id and new id are same don't change teams.
            if (Possession == m_currentPlayer.TeamID) return;

            // Temp like this in case i want to change something.
            if (IsBallLoose())
                teamToSwitch = m_currentPlayer.TeamID;
            else
                teamToSwitch = m_currentPlayer.TeamID;
        }
        ChangePossession(teamToSwitch, false, false);
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
        print("changing to " + team);
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

    public bool IsBallLoose()
    {
        return Possession == -1;
    }

    public bool IsBallNotInPlay()
    {
        return Possession < -1 && Possession > 1;
    }
}
