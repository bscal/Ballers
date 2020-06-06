using MLAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Players connection status
/// </summary>
public enum ServerPlayerStatus
{
    NONE,
    FAILED_CONNECT,
    CONNECTED,
    DISCONNECTED,
    ABANDONED
}

/// <summary>
/// Players State
/// </summary>
public enum ServerPlayerState
{
    NONE,
    JOINED,
    LOADED,
    READY
}

/// <summary>
/// User, Character, and Player data stored on the server. This is for server side use only.
/// </summary>
public class ServerPlayer
{

    public ulong steamId;

    public ServerPlayerStatus status;
    public ServerPlayerState state;

    public ServerPlayer(ulong steamId)
    {
        this.steamId = steamId;
    }

    public void SetStatus(ServerPlayerStatus status)
    {
        this.status = status;
    }

}
