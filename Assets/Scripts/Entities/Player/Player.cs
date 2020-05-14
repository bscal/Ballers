using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkedVar.Collections;
using MLAPI.NetworkedVar;
using MLAPI.Prototyping;
using System;
using MLAPI.Serialization;
using System.IO;
using MLAPI.Serialization.Pooled;

public class Player : NetworkedBehaviour, IBitWritable
{

    // Local Player Events
    //  These are not synced over the network and only used by local client.
    public event Action<Player> Shoot;
    public event Action<Player> Release;

    private static readonly NetworkedVarSettings settings = new NetworkedVarSettings() {
        SendChannel = "PlayerChannel", // The var value will be synced over this channel
        ReadPermission = NetworkedVarPermission.Everyone, // The var values will be synced to everyone
        ReadPermissionCallback = null, // Only used when using "Custom" read permission
        SendTickrate = 2, // The var will sync no more than 2 times per second
        WritePermission = NetworkedVarPermission.OwnerOnly, // Only the owner of this object is allowed to change the value
        WritePermissionCallback = null // Only used when write permission is "Custom"
    };

    [Header("User Ids")]
    public int id;
    public string username = "test";
    [Header("CPU or Dummy Controls")]
    public bool isDummy = false;
    public int position = 3;
    public float height = 2.35f;

    [Header("Screen Hitbox")]
    public GameObject m_screenHitboxes;
    public BoxCollider m_screenCollider;
    public BoxCollider m_hardScreenCollider;

    [Header("Player Data")]
    public int teamID;

    // Client Values
    public bool isRightHanded = true;

    public bool isMoving = false;
    public bool isSprinting = false;
    public bool isScreening = false;
    public bool isHardScreening = false;
    public bool isShooting = false;
    public bool isHelping = false;
    public bool isMovementFrozen = false;
    public bool isBallInLeftHand = false;
    public bool isCtrlDown = false;
    public bool isAltDown = false;

    public bool isDribUp = false;
    public bool isDribDown = false;
    public bool isDribLeft = false;
    public bool isDribRight = false;

    // Server values
    public bool isDribbling = false;
    public bool isInsideThree = false;
    public bool isInbounds = false;

    public ClientPlayer ClientPlayer { get; private set; }
    public bool HasBall { get; set; } = false;
    public Vector3 RightHand { get { return m_rightHand.transform.position; } }
    public Vector3 LeftHand { get { return m_leftHand.transform.position; } }
    public Vector3 GetHand { get { return (isBallInLeftHand) ? LeftHand : RightHand; } }
    public Vector3 CenterPos { get { return m_center.transform.position; } }
    public Transform OwnBasket { get { return GameManager.Singleton.baskets[teamID].transform; } }
    public bool OnLeftSide { get { return transform.position.x < 0; } }

    private Vector3 m_target;
    public Vector3 LookTarget { get { return m_target; } }
    public Quaternion LookRotation { get { return Quaternion.LookRotation(m_target); } }
    public float DistanceFromTarget { get { return Vector3.Distance(transform.position, m_target); } }
    private Player m_assignment;
    public Player Assignment
    {
        get
        {
            if (isHelping) return GameManager.Singleton.BallHandler;
            else if (!OnOffense()) return m_assignment;
            else return GetNearestEnemy();
        }
        set { m_assignment = value; }
    }

    [SerializeField]
    private GameObject m_rightHand;
    [SerializeField]
    private GameObject m_leftHand;
    private GameObject m_center;
    private ShotMeter m_shotmeter;
    private Animator m_animator;
    private ShotController m_shotController;
    private ShotManager m_shotManager;
    private SpriteRenderer m_playerCircle;

    private void Start()
    {
        m_playerCircle = GetComponentInChildren<SpriteRenderer>();
        if (!isDummy)
        {
            if (IsClient)
            {
                GameManager.Singleton.InitLocalPlayer(OwnerClientId);
                //NetworkEvents.Singleton.RegisterEvent(NetworkEvent.GAME_START, this, OnGameStarted);
                GameManager.Singleton.GameStarted += OnGameStarted;
                m_shotmeter = GetComponent<ShotMeter>();
                m_shotController = GetComponent<ShotController>();
            }

            if (IsServer && !IsHost)
            {
                username = "Server";
            }
            else
            {
                m_rightHand = transform.Find("root/body/right arm/forearm/hand").gameObject;
                m_leftHand = transform.Find("root/body/left arm/forearm/hand").gameObject;
                m_center = transform.Find("Center").gameObject;
                m_animator = GetComponentInChildren<Animator>();
            }

            id = username.GetHashCode();
            m_shotManager = GameObject.Find("GameManager").GetComponent<ShotManager>();
        }

    }

    public override void NetworkStart()
    {
        if (!IsOwner)
            return;
    }

    void Update()
    {
        if (!IsOwner || isDummy) return;

        if (!GameManager.Singleton.HasStarted) return;

        m_animator.SetBool("hasBall", HasBall);
        m_animator.SetBool("hasBallInLeft", isBallInLeftHand);

        Debugger.Instance.Print(string.Format("{0} : {1}", transform.position.ToString(), Vector3.Distance(transform.position, LookTarget)), 0);
        Debugger.Instance.Print(string.Format("2pt:{0}", isInsideThree), 3);

        GameObject.Find("Cube").transform.position = transform.position + transform.forward * 3 + transform.up * 3;

        m_target = GameManager.Singleton.baskets[GameManager.Singleton.Possession].gameObject.transform.position;
    }

    public void StartLoad()
    {
        if (!isDummy)
        {
            if (IsClient)
            {
                GameManager.Singleton.InitLocalPlayer(OwnerClientId);
                //NetworkEvents.Singleton.RegisterEvent(NetworkEvent.GAME_START, this, OnGameStarted);
                GameManager.Singleton.GameStarted += OnGameStarted;
                m_shotmeter = GetComponent<ShotMeter>();
                m_shotController = GetComponent<ShotController>();
            }

            if (IsServer && !IsHost)
            {
                username = "Server";
            }
            else
            {
                m_rightHand = transform.Find("root/body/right arm/forearm/hand").gameObject;
                m_leftHand = transform.Find("root/body/left arm/forearm/hand").gameObject;
                m_center = transform.Find("Center").gameObject;
                m_animator = GetComponentInChildren<Animator>();
                m_playerCircle = transform.Find("Player Circle").GetComponent<SpriteRenderer>();
            }

            id = username.GetHashCode();
            m_shotManager = GameObject.Find("GameManager").GetComponent<ShotManager>();
        }

    }

    public void ShootBall()
    {
        isShooting = true;
        InvokeServerRpc(ServerShootBall, OwnerClientId, m_shotmeter.targetHeight);
    }

    public void ReleaseBall()
    {
        isShooting = false;
        Release?.Invoke(this);
        GameManager.GetBallHandling().InvokeServerRpc(GameManager.GetBallHandling().OnRelease, OwnerClientId);
    }

    [ServerRPC]
    public void ServerShootBall(ulong id, float targetHeight)
    {
        float speed = UnityEngine.Random.Range(3, 6);
        float startOffset = 0f;
        float endOffset = 0f;
        float bonusHeight = UnityEngine.Random.Range(0, 4);

        GameManager.Singleton.GetShotManager().OnShoot(id, this, speed, targetHeight, bonusHeight, startOffset, endOffset);
        GameManager.GetBallHandling().OnShoot(id, speed, targetHeight, startOffset, endOffset);
    }

    [ClientRPC]
    public void ClientShootBall(ShotType type, bool leftHanded, float speed, float bonusHeight, float start, float end)
    {
        print(type);
        isShooting = true;
        if (isCtrlDown)
            ChangeHand();
        m_shotmeter.OnShoot(this, speed, bonusHeight, start, end);
        PlayAnimationForType(type, leftHanded);
    }

    [ClientRPC]
    public void ClientReleaseBall(float distance)
    {
    }

    /// <summary>
    /// Plays an animation given the type and hand.
    /// </summary>
    public void PlayAnimationForType(ShotType type, bool leftHanded)
    {
        switch (type)
        {
            case ShotType.SHOT:
                m_animator.Play("Shoot");
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

    public float Dist(Vector3 other)
    {
        return Vector3.Distance(transform.position, other);
    }

    public bool OnOffense()
    {
        return GameManager.Singleton.Possession == teamID;
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

    private void OnGameStarted()
    {
    }

    private Player GetNearestEnemy()
    {
        Player shortestPlayer = null;
        float shortestDist = 0;

        Team enemyTeam = GameManager.Singleton.teams[teamID ^ 1];
        for (int i = 0; i < GameManager.Singleton.teamSize; i++)
        {
            Player p = GameManager.GetPlayer(enemyTeam.playersInPosition[i]);
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

    [ClientRPC]
    public void ReadPlayerFromServer(Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            isInsideThree = reader.ReadBit();
            isInbounds = reader.ReadBit();
            HasBall = reader.ReadBit();
        }
    }

    public void Read(Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            teamID = Convert.ToInt32(reader.ReadBit());

            isRightHanded = reader.ReadBool();
            isMoving = reader.ReadBool();
            isSprinting = reader.ReadBool();
            isDribbling = reader.ReadBit();
            isScreening = reader.ReadBool();
            isHardScreening = reader.ReadBool();
            isShooting = reader.ReadBool();
            isHelping = reader.ReadBool();

            isMovementFrozen = reader.ReadBool();
            isBallInLeftHand = reader.ReadBool();
            isCtrlDown = reader.ReadBool();
            isAltDown = reader.ReadBool();
            isDribUp = reader.ReadBool();
            isDribDown = reader.ReadBool();
            isDribLeft = reader.ReadBool();
            isDribRight = reader.ReadBool();

            m_target = reader.ReadVector3Packed();
        }
    }

    public void Write(Stream stream)
    {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream))
        {
            writer.WriteBit(Convert.ToBoolean(teamID));

            writer.WriteBool(isRightHanded);
            writer.WriteBool(isMoving);
            writer.WriteBool(isSprinting);
            writer.WriteBool(isDribbling);
            writer.WriteBool(isScreening);
            writer.WriteBool(isHardScreening);
            writer.WriteBool(isShooting);
            writer.WriteBool(isHelping);

            writer.WriteBool(isMovementFrozen);
            writer.WriteBool(isBallInLeftHand);
            writer.WriteBool(isCtrlDown);
            writer.WriteBool(isAltDown);
            writer.WriteBool(isDribUp);
            writer.WriteBool(isDribDown);
            writer.WriteBool(isDribLeft);
            writer.WriteBool(isDribRight);
            //

            writer.WriteVector3Packed(m_target);
        }
    }
}
