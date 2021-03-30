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

    public static bool initilized = false;

    public static void InitMatch()
    {
        initilized = true;
        ResetDefaults();
        ServerManager.Singleton.ResetDefaults();
    }

    public static void ResetDefaults()
    {
        MatchID = 0;
        HostID = CSteamID.Nil;
        HostServer = false;
        matchTeams = new MatchTeam[] {
            new MatchTeam(),
            new MatchTeam()
        };
    }

    public static void AddPlayer(ulong steamid, int cid)
    {
        if (ServerManager.Singleton.ContainsPlayer(steamid))
            return;
        ServerManager.Singleton.AddPlayer(steamid, cid);

        int teamID;

        if (matchTeams[0].numOfPlayers >= MatchSettings.TeamSize)
            teamID = 1;
        else if (matchTeams[1].numOfPlayers >= MatchSettings.TeamSize)
            teamID = 0;
        else if (matchTeams[0].numOfPlayers > matchTeams[1].numOfPlayers)
            teamID = 1;
        else
            teamID = 0;

        MatchTeam team = matchTeams[teamID];
        team.playerIds.Add(steamid);

        ServerManager.Singleton.AssignPlayer(steamid, teamID);
        team.numOfPlayers++;
        team.teamSize++;
    }

    public static void RemovePlayer(ulong steamid)
    {

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
