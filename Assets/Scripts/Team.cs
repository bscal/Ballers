using System.Collections;
using System.Collections.Generic;

public class Team
{

    public int[] players;
    public int points = 0;
    public int fouls = 0;

    public Team(int t_size)
    {
        players = new int[t_size];
    }

}

public enum TeamTag
{
    HOME,
    AWAY
}
