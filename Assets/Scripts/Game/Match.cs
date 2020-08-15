using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class TeamGlobals
{
    public static List<ulong> HomeIds { get; set; }
    public static List<ulong> AwayIds { get; set; }
    public static Dictionary<int, ulong> HomeIdsByPosition { get; set; }
    public static Dictionary<int, ulong> AwayIdsByPosition { get; set; }

}

public static class Match
{
    public static NetworkLobby NetworkLobby { get; set; }
    public static MatchSettings MatchSettings { get; set; }
    public static int PlayersNeeded { get; set; }
    public static ulong MatchID { get; set; }
    public static CSteamID HostID { get; set; }
    public static bool HostServer { get; set; }
    public static bool HasGameStarted { get { return GameManager.Singleton.HasStarted; } }
    public static MatchTeam[] matchTeams = new MatchTeam[] {
        new MatchTeam(),
        new MatchTeam()
    };

    private static int m_curTeam = 0;

    public static void ResetDefault()
    {
        NetworkLobby = null;
        MatchID = 0;
        HostID = CSteamID.Nil;
        HostServer = false;
        matchTeams = new MatchTeam[] {
            new MatchTeam(),
            new MatchTeam()
        };
    }

    public static void AddPlayer(ulong steamid)
    {
        if (m_curTeam > 1 || m_curTeam < 0) m_curTeam = 0;

        MatchTeam team = matchTeams[m_curTeam];

        if (team.teamSize >= MatchSettings.TeamSize)
        {
            team = matchTeams[m_curTeam & 1];
        }

        team.playerIds.Add(steamid);
        team.numOfPlayers++;
        team.teamSize++;

        // Switches between 0 and 1
        m_curTeam &= 1;
    }

    public static int GetPlayersTeam(ulong steamid)
    {
        for(int i = 0; i < 2; i++)
        {
            foreach (ulong sid in matchTeams[i].playerIds)
            {
                if (sid == steamid) return i;
            }
        }
        return 0;
    }

}
