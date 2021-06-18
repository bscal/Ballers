using MLAPI.Serialization;
using Steamworks;

public static class Match
{
    public static NetworkLobby NetworkLobby { get; set; }
    public static MatchSettings MatchSettings { get; set; }
    public static int PlayersNeeded { get; set; }
    public static ulong MatchID { get; set; }
    public static CSteamID HostID { get; set; }
    public static bool HostServer { get; set; }
    public static bool HasGameStarted { get; set; }

    public static MatchTeam[] matchTeams;

    public static bool initilized = false;

    public static void InitMatch(MatchSettings settings)
    {
        initilized = true;
        MatchSettings = settings;
        matchTeams = new MatchTeam[] {
            new MatchTeam(settings.TeamSize),
            new MatchTeam(settings.TeamSize)
        };
    }

    public static void ResetDefaults()
    {
        MatchID = 0;
        HostID = CSteamID.Nil;
        HostServer = false;
        matchTeams = null;
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
        player.props.teamID = teamId;
    }

    public static void AssignPlayerWithTeam(Player player, int teamId)
    {
        MatchTeam team = matchTeams[teamId];
        team.AddSlot(team.NextSlot(), player);
        player.props.teamID = teamId;
    }

    public static void RemovePlayer(ulong steamid)
    {
        ServerManager.Instance.players.Remove(steamid);
    }
}
