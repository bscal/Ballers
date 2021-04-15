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
    JOINING,
    LOADING,
    WAITING_FOR_IDS,
    IDLE,
    ENTERED_GAME,
    READY
}

/// <summary>
/// User, Character, and Player data stored on the server. This is for server side use only.
/// </summary>
public class ServerPlayer
{

    public readonly ulong id;
    public int cid;
    public ulong steamId;

    public ServerPlayerStatus status;
    public ServerPlayerState state;

    public ServerPlayer(ulong id)
    {
        this.id = id;
    }

    public bool IsFullyLoaded()
    {
        return state == ServerPlayerState.IDLE && status == ServerPlayerStatus.CONNECTED;
    }

    public bool IsReady()
    {
        return state == ServerPlayerState.READY;
    }

    public bool IsConnected()
    {
        return status == ServerPlayerStatus.CONNECTED;
    }
}
