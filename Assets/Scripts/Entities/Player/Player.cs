using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkedVar.Collections;
using MLAPI.NetworkedVar;
using MLAPI.Prototyping;
using System;

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

    [Header("Screen Hitbox")]
    public GameObject m_screenHitboxes;
    public BoxCollider m_screenCollider;
    public BoxCollider m_hardScreenCollider;

    [Header("Player Data")]
    public uint teamID;

    public bool isRightHanded = true;
    public bool isDribbling = false;
    public bool isMoving = false;
    public bool isSprinting = false;
    public bool isInsideThree = false;
    public bool isScreening = false;
    public bool isHardScreening = false;
    public bool isShooting = false;

    public bool IsBallInLeftHand = false;

    private GameObject m_rightHand;
    private GameObject m_leftHand;

    private void Awake()
    {
        if (!IsOwner)
            return;
    }

    public override void NetworkStart()
    {
        if (!IsOwner)
            return;

        GameManager.Singleton.GameStarted += OnGameStarted;

        if (IsServer && !IsHost)
        {
            username = "Server";
        }
        else
        {
            if (!isDummy)
                GameManager.AddPlayer(NetworkedObject);
            m_rightHand = GameObject.Find("right arm/forearm/hand");
            m_leftHand = GameObject.Find("left arm/forearm/hand");
        }
        id = username.GetHashCode();
    }

    void Update()
    {
        if (!IsOwner || isDummy)
            return;

        Debugger.Instance.Print(string.Format("D:{0}, W:{1}, S:{2}", isDribbling, isMoving, isSprinting), 0);
        Debugger.Instance.Print(string.Format("2pt:{0}", isInsideThree), 3);
    }

    public void ShootBall()
    {
        isShooting = true;
        Shoot(this);
        GameManager.GetBallHandling().ShootBall(OwnerClientId);
    }

    public void ReleaseBall()
    {
        isShooting = false;
        print("released");
        Release(this);
    }

    public float Dist(Vector3 other)
    {
        return Vector3.Distance(gameObject.transform.position, other);
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

    }

}
