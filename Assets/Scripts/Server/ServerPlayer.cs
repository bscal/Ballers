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
/// User, Character, and Player data stored on the server. This is for server side use only.
/// </summary>
public class ServerPlayer
{


    public readonly ulong id;
    public int cid;
    public ulong steamId;

    public ServerPlayerStatus status;
    public bool isReady;
    public bool hasJoined;
    public bool hasLoaded;
    public bool hasEnteredGame;

    public ServerPlayer(ulong id)
    {
        this.id = id;
    }

}
