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
    public int team;
    public int slot;
    public bool hasBeenSetup;

    public ServerPlayerStatus status;
    public ServerPlayerState state;

    public ServerPlayer(ulong id)
    {
        this.id = id;
    }

    public bool IsFullyConnected()
    {
        return (state == ServerPlayerState.IDLE || state == ServerPlayerState.READY) && status == ServerPlayerStatus.CONNECTED;
    }
}
