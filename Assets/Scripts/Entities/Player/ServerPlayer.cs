using MLAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// User, Character, and Player data stored on the server. This is for server side use only.
/// </summary>
public class ServerPlayer : NetworkedBehaviour
{

    public ulong steamId;

    public ServerPlayer(ulong steamId)
    {
        this.steamId = steamId;
    }

}
