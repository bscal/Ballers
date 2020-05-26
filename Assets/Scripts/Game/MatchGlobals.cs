using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class MatchGlobals
{
    public static NetworkLobby NetworkLobby { get; set; }
    public static BallersGamemode GameMode { get; set; }
    public static int TeamSize { get; set; }
    public static int QuarterLength { get; set; }
    public static int QuartersCount { get; set; }
    public static ulong MatchID { get; set; }
    public static CSteamID HostID { get; set; }

    public static bool HasGameStarted { get { return GameManager.Singleton.HasStarted; } }
    public static bool HasLoadedGame { get; set; }
    public static bool HasJoinedLobby { get; set; }
    public static bool HostServer { get; set; }

    public static void ResetDefault()
    {
        NetworkLobby = null;
        MatchID = 0;
        HostID = CSteamID.Nil;
        HasLoadedGame = false;
        HasJoinedLobby = false;
        HostServer = false;
    }

}
