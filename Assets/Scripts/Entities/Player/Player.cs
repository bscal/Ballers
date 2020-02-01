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

    public event Action<Player> Shoot;

    private static readonly NetworkedVarSettings settings = new NetworkedVarSettings() {
        SendChannel = "PlayerChannel", // The var value will be synced over this channel
        ReadPermission = NetworkedVarPermission.Everyone, // The var values will be synced to everyone
        ReadPermissionCallback = null, // Only used when using "Custom" read permission
        SendTickrate = 2, // The var will sync no more than 2 times per second
        WritePermission = NetworkedVarPermission.OwnerOnly, // Only the owner of this object is allowed to change the value
        WritePermissionCallback = null // Only used when write permission is "Custom"
    };

    public int id;
    public string username = "test";
    public uint teamID;

    public bool isRightHanded = true;
    public bool isDribbling = false;
    public bool isMoving = false;
    public bool isSprinting = false;
    public bool isInsideThree = false;

    public NetworkedVar<Vector3> rightHand;
    public NetworkedVar<Vector3> leftHand;
    public NetworkedDictionary<string, int> skills;

    private void Awake()
    {
        if (!IsOwner)
            return;

        rightHand = new NetworkedVar<Vector3>(settings);
        leftHand = new NetworkedVar<Vector3>(settings);
        skills = new NetworkedDictionary<string, int>(settings);


        //TEAMID
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
            GameManager.AddPlayer(NetworkedObject);
            rightHand.Value = GameObject.Find("right hand").transform.position;
        }
        id = username.GetHashCode();
    }

    void Update()
    {
        if (!IsOwner)
            return;

        Debugger.Instance.Print(string.Format("D:{0}, W:{1}, S:{2}", isDribbling, isMoving, isSprinting), 0);
        Debugger.Instance.Print(string.Format("2pt:{0}", isInsideThree), 3);
    }

    public void ShootBall()
    {
        GameManager.GetBallHandling().ShootBall(OwnerClientId);
    }

    private void OnGameStarted()
    {

    }

}
