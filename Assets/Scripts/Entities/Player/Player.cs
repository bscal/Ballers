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

public class Player : NetworkedBehaviour
{

    // Local Player Events
    //  These are not synced over the network and only used by local client.
    public event Action<Player> PreShoot;
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

    public bool isRightHanded = true;
    public bool isDribbling = false;
    public bool isMoving = false;
    public bool isShiftLeft = false;
    public bool isShiftRight = false;
    public bool isShiftBack = false;
    public bool isSprinting = false;
    public bool isInsideThree = false;
    public bool isScreening = false;
    public bool isHardScreening = false;
    public bool isShooting = false;
    public bool isHelping = false;
    public bool isMovementFrozen = false;

    public bool IsBallInLeftHand = false;
    public bool HasBall 
    { get 
        { 
            return GameManager.GetBallHandling().PlayerWithBall == OwnerClientId || isDummy && GameManager.GetBallHandling().PlayerWithBall == BallHandling.DUMMY_PLAYER;
        }
    }
    public Vector3 RightHand { get { return m_rightHand.transform.position; } }
    public Vector3 LeftHand { get { return m_leftHand.transform.position; } }
    public Vector3 CenterPos { get { return m_center.transform.position; } }

    public Player Assignment { get
        {
            if (isHelping) return GameManager.BallHandler;

            else return GetNearestEnemy();
        } }

    [SerializeField]
    private GameObject m_rightHand;
    [SerializeField]
    private GameObject m_leftHand;
    private GameObject m_center;
    private ShotMeter m_shotmeter;
    private Animator m_animator;

    public override void NetworkStart()
    {
        if (!IsOwner)
            return;

        if (!isDummy)
        {
            GameManager.AddPlayer(NetworkedObject);

            if (IsClient)
            {
                //NetworkEvents.Singleton.RegisterEvent(NetworkEvent.GAME_START, this, OnGameStarted);
                GameManager.Singleton.GameStarted += OnGameStarted;
                m_shotmeter = GetComponent<ShotMeter>();
            }
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
    }

    void Update()
    {
        if (!IsOwner || isDummy)
            return;

        m_animator.SetBool("hasBall", HasBall);
        m_animator.SetBool("hasBallInLeft", IsBallInLeftHand);

        Debugger.Instance.Print(string.Format("D:{0}, W:{1}, S:{2}", isDribbling, isMoving, isSprinting), 0);
        Debugger.Instance.Print(string.Format("2pt:{0}", isInsideThree), 3);

        GameObject.Find("Cube").transform.position = transform.position + transform.forward * 3 + transform.up * 3;
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
        print("released");
    }

    [ServerRPC]
    public void ServerShootBall(ulong id, float targetHeight)
    {
        float speed = UnityEngine.Random.Range(3, 6);
        float startOffset = 0f;
        float endOffset = 0f;
        InvokeClientRpcOnClient(ClientShootBall, id, speed, startOffset, endOffset);
        GameManager.Singleton.GetShotManager().OnShoot(id, speed, targetHeight, startOffset, endOffset);
        GameManager.GetBallHandling().OnShoot(id, speed, targetHeight, startOffset, endOffset);
        //GameManager.GetBallHandling().ShootBall(id, speed, height, startOffset, endOffset);
    }

    [ClientRPC]
    public void ClientShootBall(float speed, float start, float end)
    {
        isShooting = true;
        m_shotmeter.OnShoot(this, speed, start, end);
    }

    public float Dist(Vector3 other)
    {
        return Vector3.Distance(transform.position, other);
    }

    public GameObject GetLeftHand()
    {
        return m_leftHand;
    }

    public GameObject GetRightHand()
    {
        return m_rightHand;
    }

    private void OnGameStarted()
    {
        print("called");
    }

    private Player GetPlayerByPosition(Team team, int position)
    {
        return (team.players[position]) ? team.players[position] : team.players[0];
    }


    private Player GetNearestEnemy()
    {
        Player shortestPlayer = null;
        float shortestDist = 0;

        Team enemyTeam = GameManager.Singleton.teams[teamID ^ 1];
        for (int i = 0; i < GameManager.Singleton.teamSize; i++)
        {
            Player p = enemyTeam.players[i];
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
}
