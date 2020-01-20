using System.Collections;
using System.Collections.Generic;

public class Team
{

    public int id;
    public int[] players;
    public int points = 0;
    public int fouls = 0;

    public Team(int t_id, int t_size)
    {
        id = t_id;
        players = new int[t_size];
    }

}