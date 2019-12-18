using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkedVar.Collections;
using MLAPI.NetworkedVar;
using MLAPI.Prototyping;

public class Player : NetworkedBehaviour
{

    public int id;
    public string username = "test";
    public NetworkedDictionary<string, int> skills = new NetworkedDictionary<string, int>(new NetworkedVarSettings() {
        SendChannel = "MySendChannel", // The var value will be synced over this channel
        ReadPermission = NetworkedVarPermission.Everyone, // The var values will be synced to everyone
        ReadPermissionCallback = null, // Only used when using "Custom" read permission
        SendTickrate = 0, // The var will sync no more than 2 times per second
        WritePermission = NetworkedVarPermission.OwnerOnly, // Only the owner of this object is allowed to change the value
        WritePermissionCallback = null // Only used when write permission is "Custom"
    });

    public bool IsRightHanded { get; set; }    = true;
    public bool IsDribbling { get; set; }      = false;
    public bool IsMoving { get; set; }         = false;
    public bool IsSprinting { get; set; }      = false;


    public override void NetworkStart()
    {
        id = username.GetHashCode();
    }

    // Update is called once per frame
    void Update()
    {
        Debugger.Instance.Print(string.Format("D:{0}, W:{1}, S:{2}", IsDribbling, IsMoving, IsSprinting), 0);
    }

}
