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

    public bool isRightHanded = true;
    public bool isDribbling = false;
    public bool isMoving = false;
    public bool isSprinting = false;
    public bool isInsideThree = false;

    public NetworkedVar<Vector3> rightHand = new NetworkedVar<Vector3>(settings);
    public NetworkedVar<GameObject> leftHand = new NetworkedVar<GameObject>(settings);

    public NetworkedDictionary<string, int> skills = new NetworkedDictionary<string, int>(settings);

    public override void NetworkStart()
    {
        GameManager.Singleton.OnStartGame += StartGame;

        if (IsServer && !IsHost)
        {
            username = "Server";
        }
        else
        {
            GameManager.AddPlayer(this, NetworkedObject);
            rightHand.Value = GameObject.Find("right hand").transform.position;
        }
        id = username.GetHashCode();
    }

    // Update is called once per frame
    void Update()
    {
        Debugger.Instance.Print(string.Format("D:{0}, W:{1}, S:{2}", isDribbling, isMoving, isSprinting), 0);
        Debugger.Instance.Print(string.Format("2pt:{0}", isInsideThree), 3);
    }

    public void StartGame()
    {

    }

    internal void OnShoot()
    {
        GameManager.GetBallHandling().OnShoot(this);
    }

}
