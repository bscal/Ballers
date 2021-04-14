using Steamworks;

public static class Match
{
    public static NetworkLobby NetworkLobby { get; set; }
    public static MatchSettings MatchSettings { get; set; }
    public static int PlayersNeeded { get; set; }
    public static ulong MatchID { get; set; }
    public static CSteamID HostID { get; set; }
    public static bool HostServer { get; set; }
    public static bool HasGameStarted { get { return GameManager.Singleton.HasStarted; } }

    public static Player localPlayer;

    public static MatchTeam[] matchTeams;

    public static bool initilized = false;

    public static void InitMatch(int size)
    {
        initilized = true;
        PlayersNeeded = size;
        matchTeams = new MatchTeam[] {
            new MatchTeam(size),
            new MatchTeam(size)
        };
    }

    public static void ResetDefaults()
    {
        MatchID = 0;
        HostID = CSteamID.Nil;
        HostServer = false;
        matchTeams = null;
    }

    public static void SetupPlayer(ulong id, ulong steamid, int cid)
    {
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
        team.playerIds.Add(id);

        ServerManager.Singleton.AssignPlayer(id, teamID);
        team.numOfPlayers++;
        team.teamSize++;
    }

    public static void AssignPlayer(Player player)
    {
        int openHome = matchTeams[0].GetNumOfOpenSlots();
        int openAway = matchTeams[1].GetNumOfOpenSlots();

        int teamId;

        if (openHome > 0 && openHome >= openAway)
            teamId = 0;
        else
            teamId = 1;

        MatchTeam team = matchTeams[teamId];
        team.AddSlot(team.NextSlot(), player);
        team.numOfPlayers++;
        player.props.teamID = teamId;
    }

    public static void RemovePlayer(ulong steamid)
    {
        ServerManager.Singleton.players.Remove(steamid);
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
