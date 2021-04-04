using System;
using System.IO;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;
using Ballers;

public class Player : CommonPlayer, INetworkSerializable
{
    // Local Player Events
    //  These are not synced over the network and only used by local client.
    public event Action<ShotData, ShotBarData> Shoot;
    public event Action<float, float> Release;
    public event Action<float, float> StartRoundMeter;
    public event Action<float> StopRoundMeter;

    [Header("User Ids")]
    public string username = "test";
    [Header("CPU or Dummy Controls")]
    public bool isDummy = false;
    public int aiPlayerID = 0;
    public int slot = 3;
    public float height = 2.35f;
    public int teamID;
    public bool hasReadyUp;
    public bool isAI;
    [NonSerialized]
    public ulong steamID;
    [NonSerialized]
    public CharacterData cData;

    [Header("Screen Hitbox")]
    public GameObject m_screenHitboxes;
    public BoxCollider m_screenCollider;
    public BoxCollider m_hardScreenCollider;

    [Header("Contest Hitbox")]
    public SphereCollider m_innerCollider;
    public SphereCollider m_outerCollider;

    [Header("Player Collider")]
    public BoxCollider playerCollider;

    // Client Values
    public bool isRightHanded;
    public bool isMoving;
    public bool isSprinting;
    public bool isScreening;
    public bool isHardScreening;
    public bool isShooting;
    public bool isHelping;
    public bool isBallInLeftHand;
    public bool isCtrlDown;
    public bool isAltDown;
    public bool movingFoward;
    public bool movingBack;
    public bool movingLeft;
    public bool movingRight;
    public bool isContesting;
    public bool isBlocking;
    public bool isStealing;
    public bool isDribbling;


    // Server values
    public bool isInsideThree;
    public bool isInbounds;
    public bool isPostShot;
    public bool isPostMove;


    public int OtherTeam { get { return FlipTeamID(teamID); } }
    /// <summary>
    /// Returns true if Player is an AI or is a Dummy
    /// </summary>
    public bool IsNpc { get { return isAI || isDummy; } }
    public bool HasBall { get { return NetworkObjectId == GameManager.GetBallHandling().PlayerWithBall; } }
    public Vector3 RightHandPos { get { return m_rightHand.transform.position; } }
    public Vector3 LeftHandPos { get { return m_leftHand.transform.position; } }
    public Vector3 CurHandPos { get { return (isBallInLeftHand) ? LeftHandPos : RightHandPos; } }
    public Vector3 CenterPos { get { return m_center.transform.position; } }
    public Transform OwnBasket { get { return GameManager.Singleton.baskets[teamID].transform; } }
    public Transform OtherBasket { get { return GameManager.Singleton.baskets[FlipTeamID(teamID)].transform; } }
    public bool OnLeftSide { get { return transform.position.x < 0; } }
    private Vector3 m_target;
    public Vector3 TargetPos { get { return m_target; } }
    public Quaternion TargetRotation { get { return Quaternion.LookRotation(m_target); } }
    public float DistanceFromTarget { get { return Vector3.Distance(transform.position, m_target); } }
    private Player m_assignment;
    public Player Assignment
    {
        get
        {
            if (IsOnOffense() || GameManager.GetBallHandling().IsBallLoose()) return null;
            if (isHelping) return GameManager.Singleton.BallHandler;
            else if (m_assignment == null) m_assignment = GameManager.GetPlayerBySlot(FlipTeamID(teamID), slot);
            return (m_assignment == null) ? null : m_assignment;
        }
        set { m_assignment = value; }
    }

    [SerializeField]
    private GameObject m_rightHand;
    [SerializeField]
    private GameObject m_leftHand;
    [SerializeField]
    private LineTracker m_targetTracker;
    private GameObject m_center;
    private ShotMeter m_shotmeter;
    private RoundShotMeter m_roundShotMeter;
    private Animator m_animator;
    private PlayerAnimHandler m_animHandler;
    private Movement m_movement;
    private ShotManager m_shotManager;
    private SpriteRenderer m_playerCircle;

    private void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        // Initialize Player values
        m_playerCircle = GetComponentInChildren<SpriteRenderer>();
        m_center = transform.Find("Center").gameObject;
        m_movement = GetComponent<Movement>();

        if (!IsNpc)
        {
            // Initialize Human Player values
            //GameManager.Singleton.RegisterLocalPlayerToServer(OwnerClientId);
        }
    }

    public override void NetworkStart()
    {
        base.NetworkStart();

        if (IsOwner && !isAI)
            RequestIdsClient();

        if (IsServer)
            print("PLAYER START SERVER");
        else if (IsOwner)
            print("PLAYER START CLIENT");
    }

    void Update()
    {
        if (isDummy || !hasEnteredGame || !GameManager.Singleton.HasStarted) return;

        if (IsOwner && !isAI)
        {
            if (IsOnDefense())
                m_targetTracker.gameObject.SetActive(true);
            else 
                m_targetTracker.gameObject.SetActive(false);

            m_target = GameManager.Singleton.baskets[GameManager.Singleton.Possession].gameObject.transform.position;
        }

    }

    /// <summary>
    /// Called when the model is Instantiated
    /// </summary>
    public void InitilizeModel()
    {
        m_animator = GetComponentInChildren<Animator>();
        m_animHandler = GetComponent<PlayerAnimHandler>();
        m_animHandler.SetAnimator(m_animator);

        m_rightHand = FindTransformInChild(transform, "RightBallPos").gameObject;
        m_leftHand = FindTransformInChild(transform, "LeftBallPos").gameObject;

        Movement movement = gameObject.GetComponent<Movement>();
        if (!IsNpc && movement != null)
        {
            movement.animator = m_animator;
            movement.playerAnim = m_animHandler;
        }
    }

    public static Transform FindTransformInChild(Transform transform, string objectName)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).name.Equals(objectName))
            {
                return transform.GetChild(i);
            }

            Transform res = FindTransformInChild(transform.GetChild(i), objectName);
            if (res != null)
                return res;
        }
        return null;
    }


    public void ShootBall()
    {
        ShootBallServerRpc(NetworkObjectId);
    }

    [ServerRpc]
    public void ShootBallServerRpc(ulong netId)
    {
        if (isShooting && !HasBall) return;
        isShooting = true;
        ulong clientID = GameManager.GetPlayerByNetworkID(netId).OwnerClientId;
        ulong rtt = ServerManager.GetRTT(clientID);
        m_shotManager.OnShoot(netId, this, (rtt / 1000 / 2));
    }

    // Client
    public void ReleaseBall()
    {
        if (!isShooting && !HasBall) return;
        isShooting = false;
        ReleaseServerRpc(NetworkObjectId);
    }

    [ServerRpc]
    public void ReleaseServerRpc(ulong netID)
    {
        ulong clientID = GameManager.GetPlayerByNetworkID(netID).OwnerClientId;
        ulong rtt = ServerManager.GetRTT(clientID);
        m_shotManager.OnRelease(netID, (rtt / 1000 / 2));
    }


    [ClientRpc]
    public void ClientShootBallClientRpc(ulong netID, ShotData shotData, ShotBarData shotBarData, ClientRpcParams cParams = default)
    {
        Player p = GameManager.GetPlayerByNetworkID(netID);
        if (p.isCtrlDown)
            p.ChangeHand();
        
        if (!p.isAI)
            m_shotmeter.OnShoot(p, shotData, shotBarData);
        p.PlayAnimationForType(shotData.type, shotData.leftHanded);
        Shoot?.Invoke(shotData, shotBarData);
    }

    [ClientRpc]
    public void ClientReleaseBallClientRpc(float distance, float difference, ClientRpcParams cParams = default)
    {
        Release?.Invoke(distance, difference);
    }

    /// <summary>
    /// Plays an animation given the type and hand.
    /// </summary>
    public void PlayAnimationForType(ShotType type, bool leftHanded)
    {
        switch (type)
        {
            case ShotType.SHOT:
                m_animHandler.Play(AnimNames.REG_JUMPSHOT);
                break;
            case ShotType.LAYUP:
                if (leftHanded) m_animator.Play("LayupL");
                else m_animator.Play("Layup");
                break;
            case ShotType.DUNK:
                if (leftHanded) m_animator.Play("1hand_dunkL");
                else m_animator.Play("1hand_dunk");
                StartCoroutine(PlayerUtils.Dunk(this, GameManager.GetBasket()));
                break;
            default:
                break;
        }

    }

    public void CallForBall()
    {
        if (IsOwner)
        {
            GameManager.GetBallHandling().PlayerCallForBallServerRpc(NetworkObjectId);
        }
    }

    [ClientRpc]
    public void TriggerRoundShotMeterClientRpc(float speed, float difficulty, ClientRpcParams clientRpcParams = default)
    {
        m_roundShotMeter.StartMeter(speed, difficulty);
        StartRoundMeter?.Invoke(speed, difficulty);
    }


    [ClientRpc]
    public void ResponseRoundShotMeterClientRpc(float score)
    {
        m_roundShotMeter.Response(score);
        StopRoundMeter?.Invoke(score);
    }

    [ServerRpc]
    public void ReleaseRoundShotMeterServerRpc(ulong clientID, float result)
    {
        ulong rtt = ServerManager.GetRTT(clientID);
        m_roundShotMeter.StopMeter(result - (rtt / 1000 /2));
    }

    public void ServerCheckRSM(ulong clientID, ulong netID, float speed, float difficulty, Action<ulong, float> cb)
    {
        if (IsServer)
        {
            TriggerRoundShotMeterClientRpc(speed, difficulty, RPCParams.ClientParamsOnlyClient(clientID));
            StartCoroutine(m_roundShotMeter.ServerTimer(netID, speed, difficulty, cb));
        }
    }

    public void ReleasePass()
    {
        ReleaseRoundShotMeterServerRpc(OwnerClientId, m_roundShotMeter.GetTime());
    }

    [ServerRpc]
    public void PumpfakeServerRpc()
    {
        isDribbling = false;
    }

    public void Jump()
    {
        m_animHandler.Play(AnimNames.REBOUND);
    }

    public void Block() { }
    public void Contest() { }
    public void Steal() { }
    public void Swipe() { }
    public void DriveLeft() { }
    public void DriveRight() { }
    public void DriveStraight() { }
    public void ProtectedBall() { }
    public void TripleThreat() { }
    public void PostUp() { }


    public float Dist(Vector3 other)
    {
        return Vector3.Distance(transform.position, other);
    }

    public bool IsOnOffense()
    {
        return GameManager.GetBallHandling().Possession == teamID;
    }

    public bool IsOnDefense()
    {
        return FlipTeamID(GameManager.Singleton.Possession) == teamID;
    }

    public GameObject GetLeftHand()
    {
        return m_leftHand;
    }

    public GameObject GetRightHand()
    {
        return m_rightHand;
    }

    public void SetCircleColor(Color color)
    {
        m_playerCircle.color = color;
    }

    protected override void PlayerEnteredGame()
    {
        base.PlayerEnteredGame();

        Instantiate(ServerManager.PrefabFromTeamID(Match.GetPlayersTeam(OwnerClientId)), transform);
        InitilizeModel();

        m_shotManager = GameObject.Find("GameManager").GetComponent<ShotManager>();
        if (!IsNpc)
        {
            m_shotmeter = gameObject.AddComponent<ShotMeter>();
            m_roundShotMeter = GameObject.Find("HUD/Canvas/RoundShotMeter").GetComponent<RoundShotMeter>();
        }
    }

    protected override void OnGameStarted()
    {
        base.OnGameStarted();

        Debug.Log("OnGameStarted");
    }

    private Player GetNearestEnemy()
    {
        Player shortestPlayer = null;
        float shortestDist = float.MaxValue;

        Team enemyTeam = GameManager.Singleton.teams[FlipTeamID(teamID)];
        for (int i = 0; i < Match.MatchSettings.TeamSize; i++)
        {
            Player p = enemyTeam.teamSlots[i];
            if (!p) continue;
            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < shortestDist)
            {
                shortestPlayer = p;
                shortestDist = dist;
            }

        }

        return shortestPlayer;
    }

    public void ChangeHand()
    {
        isBallInLeftHand = !isBallInLeftHand;
    }

    public bool SameTeam(Player other)
    {
        return teamID == other.teamID;
    }

    public float GetContestRating()
    {
        float result = 0;
        for (int i = 0; i < Match.MatchSettings.TeamSize; i++)
        {
            Player otherPlayer = GameManager.GetPlayerBySlot(OtherTeam, i);

            if (m_outerCollider.bounds.Contains(otherPlayer.transform.position))
            {
                result += GetPlayerContestRating(otherPlayer,
                    Vector3.Distance(otherPlayer.transform.position, transform.position), 0);
            }
            if (m_innerCollider.bounds.Contains(otherPlayer.transform.position))
            {
                result += GetPlayerContestRating(otherPlayer,
                    Vector3.Distance(otherPlayer.transform.position, transform.position), 1);
            }
        }
        return result;
    }

    private float GetPlayerContestRating(Player other, float dist, float mod)
    {
        float res = dist + mod;
        if (other.isBlocking) res += .5f * other.cData.stats.blocking;
        if (other.isContesting) res += .25f * other.cData.stats.blocking;
        if (WithinFOV(GetForwardAngle(transform, other.transform), 45f)) res += .5f;
        if (WithinFOV(GetForwardAngle(transform, other.transform), 20f)) res += .5f;
        DebugController.Singleton.PrintConsoleValues("Contest", new object[] {
            dist, mod, other.isBlocking, other.isContesting, GetForwardAngle(transform, other.transform), WithinFOV(GetForwardAngle(transform, other.transform), 5f)
        }, LogType.WARNING);
        return res;
    }

    /// <summary>
    /// Returns the angle in degrees between source.foward and target.foward.
    /// 180 degrees is front, 90 degrees is side, 0 degrees is behind.
    /// </summary>
    public static float GetForwardAngle(Transform source, Transform target)
    {
        return Vector3.Angle(target.forward - source.forward, source.forward);
    }

    public Movement GetMovement() => m_movement;

    /// <summary>
    /// Returns true if value is greater then 180 - degrees.
    /// Angle and rangeOfDegrees should be between 0 and 180.
    /// </summary>
    public static bool WithinFOV(float angle, float rangeOfDegrees)
    {
        return Mathf.Clamp(angle, 0f, 180f) > 180f - Mathf.Clamp(rangeOfDegrees, 0f, 180f);
    }

    /// <summary>
    /// Flips a TeamID to other team
    /// </summary>
    public static int FlipTeamID(int teamid)
    {
        return Mathf.Clamp(1 - teamid, 0, 1);
    }

    public void ReadPlayerFromServer(Stream stream)
    {
        using (PooledNetworkReader reader = PooledNetworkReader.Get(stream))
        {
            // THIS CURRENT ISNT UPDATED BECAUSE LOOP NEVER SENDS
            isInsideThree = reader.ReadBool();
            isInbounds = reader.ReadBool();
            //HasBall = reader.ReadBool();
        }
    }

    public void SetReadyStatus(bool state)
    {
        hasReadyUp = state;
        ServerReadyUpServerRpc(hasReadyUp);
    }

    [ServerRpc]
    public void ServerReadyUpServerRpc(bool isReady)
    {
        this.hasReadyUp = isReady;
    }

    public void Read(Stream stream)
    {
        using (PooledNetworkReader reader = PooledNetworkReader.Get(stream))
        {
            teamID = reader.ReadInt32Packed();

            isAI = reader.ReadBool();
            isRightHanded = reader.ReadBool();
            isMoving = reader.ReadBool();
            isSprinting = reader.ReadBool();
            isDribbling = reader.ReadBit();
            isScreening = reader.ReadBool();
            isHardScreening = reader.ReadBool();
            isShooting = reader.ReadBool();

            isHelping = reader.ReadBool();
            isBallInLeftHand = reader.ReadBool();
            isCtrlDown = reader.ReadBool();
            isAltDown = reader.ReadBool();
            movingFoward = reader.ReadBool();
            movingBack = reader.ReadBool();
            movingLeft = reader.ReadBool();
            movingRight = reader.ReadBool();

            isContesting = reader.ReadBool();
            isBlocking = reader.ReadBool();
            isStealing = reader.ReadBool();

            m_target = reader.ReadVector3Packed();
        }
    }

    public void Write(Stream stream)
    {
        using (PooledNetworkWriter writer = PooledNetworkWriter.Get(stream))
        {
            writer.WriteInt32Packed(teamID);

            writer.WriteBool(isAI);
            writer.WriteBool(isRightHanded);
            writer.WriteBool(isMoving);
            writer.WriteBool(isSprinting);
            writer.WriteBool(isDribbling);
            writer.WriteBool(isScreening);
            writer.WriteBool(isHardScreening);
            writer.WriteBool(isShooting);

            writer.WriteBool(isHelping);
            writer.WriteBool(isBallInLeftHand);
            writer.WriteBool(isCtrlDown);
            writer.WriteBool(isAltDown);
            writer.WriteBool(movingFoward);
            writer.WriteBool(movingBack);
            writer.WriteBool(movingLeft);
            writer.WriteBool(movingRight);

            writer.WriteBool(isContesting);
            writer.WriteBool(isBlocking);
            writer.WriteBool(isStealing);

            writer.WriteVector3Packed(m_target);
        }
    }

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref teamID);
        serializer.Serialize(ref isAI);
        serializer.Serialize(ref isRightHanded);
        serializer.Serialize(ref isMoving);
        serializer.Serialize(ref isSprinting);
        serializer.Serialize(ref isDribbling);
        serializer.Serialize(ref isScreening);
        serializer.Serialize(ref isHelping);
        serializer.Serialize(ref isBallInLeftHand);
        serializer.Serialize(ref isCtrlDown);
        serializer.Serialize(ref isAltDown);
        serializer.Serialize(ref movingFoward);
        serializer.Serialize(ref movingBack);
        serializer.Serialize(ref movingLeft);
        serializer.Serialize(ref movingRight);
        serializer.Serialize(ref isContesting);
        serializer.Serialize(ref isBlocking);
        serializer.Serialize(ref isStealing);
        serializer.Serialize(ref m_target);
    }
}
