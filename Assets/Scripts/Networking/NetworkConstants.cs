using MLAPI.NetworkedVar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//TODO
public static class NetworkConstants
{
    public readonly static NetworkedVarSettings GAME_STATE_CHANNEL = new NetworkedVarSettings() {
        SendChannel = "GAME_STATE", // The var value will be synced over this channel
        ReadPermission = NetworkedVarPermission.Everyone, // The var values will be synced to everyone
        ReadPermissionCallback = null, // Only used when using "Custom" read permission
        SendTickrate = 5, // The var will sync no more than 4 times per second
        WritePermission = NetworkedVarPermission.ServerOnly, // Only the owner of this object is allowed to change the value
        WritePermissionCallback = null // Only used when write permission is "Custom"
    };

    public readonly static NetworkedVarSettings PLAYER_CHANNEL = new NetworkedVarSettings() {
        SendChannel = "PLAYER",
        ReadPermission = NetworkedVarPermission.Everyone, 
        ReadPermissionCallback = null, 
        SendTickrate = 20,
        WritePermission = NetworkedVarPermission.ServerOnly,
        WritePermissionCallback = null
    };

    public readonly static NetworkedVarSettings SHOT_CHANNEL = new NetworkedVarSettings() {
        SendChannel = "SHOT",
        ReadPermission = NetworkedVarPermission.Everyone,
        SendTickrate = 0,
        WritePermission = NetworkedVarPermission.ServerOnly,
    };

    public readonly static NetworkedVarSettings BALL_CHANNEL = new NetworkedVarSettings() {
        SendChannel = "BALL",
        ReadPermission = NetworkedVarPermission.Everyone,
        SendTickrate = 10,
        WritePermission = NetworkedVarPermission.ServerOnly,
    };


}