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

    public NetworkedVar<Vector3> rightHand = new NetworkedVar<Vector3>();
    public NetworkedVar<GameObject> leftHand = new NetworkedVar<GameObject>();

    public NetworkedDictionary<string, int> skills = new NetworkedDictionary<string, int>();

    public override void NetworkStart()
    {
        id = username.GetHashCode();

        //leftHand.Value = transform.Find("HandLAnimPos").gameObject;
        GameManager.AddPlayer(this, NetworkedObject);
    }

    // Update is called once per frame
    void Update()
    {
        rightHand.Value = transform.Find("Skeleton/Body/ArmR/LowerArmR/HandRAnimPos").transform.position;
        Debugger.Instance.Print(string.Format("D:{0}, W:{1}, S:{2}", isDribbling, isMoving, isSprinting), 0);
    }

    public void OnStartGame()
    {
    }

    internal void OnShoot()
    {
        GameManager.GetBallHandling().OnShoot(this);
    }

}
