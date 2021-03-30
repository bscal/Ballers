using MLAPI.NetworkVariable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//TODO
public static class NetworkConstants
{
    public readonly static NetworkVariableSettings GAME_STATE_CHANNEL = new NetworkVariableSettings() {
        SendNetworkChannel = MLAPI.Transports.NetworkChannel.Internal,
        ReadPermission = NetworkVariablePermission.Everyone, // The var values will be synced to everyone
        ReadPermissionCallback = null, // Only used when using "Custom" read permission
        SendTickrate = 5, // The var will sync no more than 4 times per second
        WritePermission = NetworkVariablePermission.ServerOnly, // Only the owner of this object is allowed to change the value
        WritePermissionCallback = null // Only used when write permission is "Custom"
    };

    public readonly static NetworkVariableSettings PLAYER_CHANNEL = new NetworkVariableSettings() {
        SendNetworkChannel = MLAPI.Transports.NetworkChannel.Internal,
        ReadPermission = NetworkVariablePermission.Everyone, 
        ReadPermissionCallback = null, 
        SendTickrate = 20,
        WritePermission = NetworkVariablePermission.ServerOnly,
        WritePermissionCallback = null
    };

    public readonly static NetworkVariableSettings SHOT_CHANNEL = new NetworkVariableSettings() {
        SendNetworkChannel = MLAPI.Transports.NetworkChannel.Internal,
        ReadPermission = NetworkVariablePermission.Everyone,
        SendTickrate = 0,
        WritePermission = NetworkVariablePermission.ServerOnly,
    };

    public readonly static NetworkVariableSettings BALL_CHANNEL = new NetworkVariableSettings() {
        SendNetworkChannel = MLAPI.Transports.NetworkChannel.Internal,
        ReadPermission = NetworkVariablePermission.Everyone,
        SendTickrate = 10,
        WritePermission = NetworkVariablePermission.ServerOnly,
    };


}
